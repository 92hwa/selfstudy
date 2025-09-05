using System;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Mat image = Cv2.ImRead(dlg.FileName);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                    label1.Text =
                        $"파일명: {System.IO.Path.GetFileName(dlg.FileName)}\n" +
                        $"크기: {image.Width} x {image.Height} \n " +
                        $"채널 수:  {image.Channels()} \n" +
                        $"타입: {image.Type()}";
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
    }
}
