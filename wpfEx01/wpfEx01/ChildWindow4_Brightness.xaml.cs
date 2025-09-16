using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wpfEx01
{
    /// <summary>
    /// ChildWindow4_Brightness.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChildWindow4_Brightness : Window
    {
        private byte[] buffer8;
        private byte[] brightnessBuffer;
        private ImageSource originalSrc;

        public ChildWindow4_Brightness(ImageSource src, byte[] buffer)
        {
            InitializeComponent();
            imgBox4.Source = src;
            buffer8 = buffer;
            brightnessBuffer = new byte[buffer8.Length];

            originalSrc = src;
        }

        private void btnBrightnessUp_Click(object sender, RoutedEventArgs e)
        {
            ChildWindow3_InputDialog dialog = new ChildWindow3_InputDialog();

            if (dialog.ShowDialog() == false) return;
            else
            {
                double userBrightness = dialog.userValue;

                for (int i = 0; i < buffer8.Length; i++)
                {
                    double newValue = buffer8[i] + userBrightness;

                    if (newValue > 255) newValue = 255;
                    if (newValue < 0) newValue = 0;

                    brightnessBuffer[i] = (byte)newValue;
                }

                int width = (int)imgBox4.Source.Width;
                int height = (int)imgBox4.Source.Height;
                int stride = width;
                WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), brightnessBuffer, stride, 0);

                imgBox4.Source = wb;

                int[] histogram = new int[256];
                for (int i = 0; i < brightnessBuffer.Length; i++)
                {
                    histogram[brightnessBuffer[i]]++;
                }

                ChildWindow1_Histogram childHistogramBrightness = new ChildWindow1_Histogram();
                childHistogramBrightness.SetImage(MainWindow.CreateHistogramBitmap(brightnessBuffer));
                childHistogramBrightness.Show();
            }
        }

        private void btnBrightnessDown_Click(object sender, RoutedEventArgs e)
        {
            ChildWindow3_InputDialog dialog = new ChildWindow3_InputDialog();

            if (dialog.ShowDialog() == false) return;
            else
            {
                double userBrightness = dialog.userValue;

                for (int i = 0; i < buffer8.Length; i++)
                {
                    double newValue = buffer8[i] - userBrightness;

                    if (newValue > 255) newValue = 255;
                    if (newValue < 0) newValue = 0;

                    brightnessBuffer[i] = (byte)newValue;
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < brightnessBuffer.Length; i++)
                {
                    sb.Append(brightnessBuffer[i] + " ");
                }

                int width = (int)imgBox4.Source.Width;
                int height = (int)imgBox4.Source.Height;
                int stride = width;
                WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), brightnessBuffer, stride, 0);

                imgBox4.Source = wb;

                int[] histogram = new int[256];
                for (int i = 0; i < brightnessBuffer.Length; i++)
                {
                    histogram[brightnessBuffer[i]]++;
                }

                ChildWindow1_Histogram childHistogramBrightness = new ChildWindow1_Histogram();
                childHistogramBrightness.SetImage(MainWindow.CreateHistogramBitmap(brightnessBuffer));
                childHistogramBrightness.Show();
            }
        }

        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            imgBox4.Source = originalSrc;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
