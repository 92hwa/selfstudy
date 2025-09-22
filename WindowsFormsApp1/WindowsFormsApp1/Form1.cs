using System;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Size = OpenCvSharp.Size;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        private Mat loadedImage;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            //WindowState = FormWindowState.Maximized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    loadedImage = Cv2.ImRead(dlg.FileName, ImreadModes.Grayscale);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(loadedImage);
                    label1.Text =
                        $"파일명: {System.IO.Path.GetFileName(dlg.FileName)}\n" +
                        $"크기: {loadedImage.Width} x {loadedImage.Height} \n " +
                        $"채널 수:  {loadedImage.Channels()} \n" +
                        $"타입: {loadedImage.Type()}";
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Mat src = new Mat();


            if (loadedImage.Channels() == 3)
            {
                Cv2.CvtColor(loadedImage, src, ColorConversionCodes.BGR2GRAY);
            }

            else
            {
                src = loadedImage.Clone();
            }
           

            Mat dst = new Mat();

            Cv2.Sobel(src, dst, MatType.CV_8UC1, 1, 0, 3, 1, 0, BorderTypes.Reflect101);

            Cv2.ImShow("dst", dst);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }
    }
}
