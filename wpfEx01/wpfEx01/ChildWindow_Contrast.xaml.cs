using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace wpfEx01
{
    /// <summary>
    /// ChildWindow_Contrast.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChildWindow_Contrast : System.Windows.Window
    {
        private BitmapSource src;
        private WriteableBitmap currentImg;

        public ChildWindow_Contrast(BitmapSource img)
        {
            InitializeComponent();
            src = img;
            currentImg = new WriteableBitmap(src);
            imgBox3.Source = currentImg;
        }

        private void btnContrastUp_Click(object sender, RoutedEventArgs e)
        {
            contrastFactor *= 1.1;
            CalculateContrast();
        }

        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            contrastFactor = 1.0;
            currentImg = new WriteableBitmap(src);
            imgBox3.Source = currentImg;
        }

        private void btnContrastDown_Click(object sender, RoutedEventArgs e)
        {
            contrastFactor /= 1.1;
            CalculateContrast();
        }

        private double contrastFactor = 1.0;

        private void CalculateContrast()
        {
            if (src == null) return;

            int width = src.PixelWidth;
            int height = src.PixelHeight;
            int stride = width;

            byte[] pixels = new byte[height * stride];
            src.CopyPixels(pixels, stride, 0);

            byte[] output = new byte[pixels.Length];

            for(int i = 0; i < pixels.Length; i++)
            {
                double val = (pixels[i] - 128) * contrastFactor + 128;
                if (val < 0) val = 0;
                if (val > 255) val = 255;
                output[i] = (byte)val;
            }
            currentImg.WritePixels(new Int32Rect(0, 0, width, height), output, stride, 0);
            imgBox3.Source = currentImg;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
