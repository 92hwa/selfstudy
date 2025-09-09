using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace rawEx01
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        string selectedFilePath;
        WriteableBitmap wb;
        
        int width = 3072;
        int height = 3072;

        ushort[] buffer16;
        byte[] buffer8;
        int[] histogram;
        int sum;
        int avg;


        public MainWindow()
        {
            InitializeComponent();
            buffer16 = new ushort[(int)(width * height)];
            buffer8 = new byte[width * height];
        }


        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "RAW Files|*.raw|All Files|*.*";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;

                FileStream fs = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);

                for (int i = 0; i < width * height; i++)
                {
                    ushort value = (ushort)reader.ReadUInt16();
                    buffer16[i] = value;
                }

                for (int j = 0; j < width * height; j++)
                {
                    buffer8[j] = (byte)(buffer16[j] >> 8);
                }

                wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), buffer8, width, 0);
                imgBox.Source = wb;

                txtBox.Text = $"선택한 파일: {selectedFilePath} \n\n";

                reader.Close();
                fs.Close();
            }
        }


        private void btnHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null)
            {
                MessageBox.Show("먼저 RAW 파일을 불러와주세요.");
                return;
            }

            int width = wb.PixelWidth;
            int height = wb.PixelHeight;
            histogram = new int[256];

            byte[] pixels = new byte[width * height * 4];
            wb.CopyPixels(pixels, width * 4, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;
                    byte gray = (byte)((pixels[index] + pixels[index + 1] + pixels[index + 2]) / 3);
                    histogram[gray]++;
                }
            }

            txtBox.Text += "*** Histogram Output \n";

            for (int i = 0; i < 256; i++)
            {
                txtBox.Text += histogram[i] + "\t";
            }

            txtBox.Text += "\n\n";
        }


        private void btnHistogramEqualization_Click(object sender, RoutedEventArgs e)
        {
            txtBox.Text += "*** Histogram Equalization Output \n";
            
            for (int i = 0; i < 256; i++)
            {
                sum += histogram[i];
            }
            avg = sum / 256;

            for (int i = 0; i < 256; i++)
            {
                txtBox.Text += $"{histogram[i]/avg} \t";
            }

            txtBox.Text += "\n\n";
        }


        private void btnLUT_Click(object sender, RoutedEventArgs e)
        {

        }



        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
