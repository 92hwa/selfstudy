using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace wpfEx01
{
    /// <summary>
    /// ChildWindow_Contrast.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChildWindow2_Contrast : Window
    {
        private byte[] buffer8;
        private byte[] contrastBuffer;
        private ImageSource originalSrc;

        public ChildWindow2_Contrast(ImageSource src, byte[] buffer)
        {
            InitializeComponent();
            imgBox3.Source = src;
            buffer8 = buffer;
            contrastBuffer = new byte[buffer8.Length];

            originalSrc = src;
        }

        private void btnContrastUp_Click(object sender, RoutedEventArgs e)
        {
            ChildWindow3_InputDialog dialog = new ChildWindow3_InputDialog();
            if (dialog.ShowDialog() == true)
            {
                double userContrast = dialog.contrastValue;

                for (int i = 0; i < buffer8.Length; i++)
                {
                    double newValue = buffer8[i] * userContrast;

                    if (newValue > 255) newValue = 255;
                    if (newValue < 0) newValue = 0;

                    contrastBuffer[i] = (byte)newValue;
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < contrastBuffer.Length; i++)
                {
                    sb.Append(contrastBuffer[i] + " ");
                }

                int width = (int)imgBox3.Source.Width;
                int height = (int)imgBox3.Source.Height;
                int stride = width;
                WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), contrastBuffer, stride, 0);

                imgBox3.Source = wb;

                int[] histogram = new int[256];
                #region 히스토그램 계산
                for (int i = 0; i < contrastBuffer.Length; i++)
                {
                    histogram[contrastBuffer[i]]++;
                }
                #endregion

                ChildWindow1_Histogram childHistogramContrast = new ChildWindow1_Histogram();
                childHistogramContrast.SetImage(MainWindow.CreateHistogramBitmap(contrastBuffer));
                childHistogramContrast.Show();
            }

            else
            {
                return;
            }
        }

        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            imgBox3.Source = originalSrc;
        }

        private void btnContrastDown_Click(object sender, RoutedEventArgs e)
        {
            ChildWindow3_InputDialog dialog = new ChildWindow3_InputDialog();
            if (dialog.ShowDialog() == true)
            {
                double userContrast = dialog.contrastValue;

                for (int i = 0; i < buffer8.Length; i++)
                {
                    double newValue = buffer8[i] / userContrast;

                    if (newValue > 255) newValue = 255;
                    if (newValue < 0) newValue = 0;

                    contrastBuffer[i] = (byte)newValue;
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < contrastBuffer.Length; i++)
                {
                    sb.Append(contrastBuffer[i] + " ");
                }

                int width = (int)imgBox3.Source.Width;
                int height = (int)imgBox3.Source.Height;
                int stride = width;
                WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), contrastBuffer, stride, 0);

                imgBox3.Source = wb;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        


    }
}
