using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Microsoft.Win32;
using FellowOakDicom;
using FellowOakDicom.Imaging;

namespace dcmEx01
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

        int[] histogram;
        ushort[] buffer16;
        byte[] buffer8;

        public MainWindow()
        {
            InitializeComponent();
            buffer16 = new ushort[(int)(width * height)];
            buffer8 = new byte[width * height];
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DICOM Files|*.dcm|All Files|*.*";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {

                // dcm 파일 열기
                selectedFilePath = openFileDialog.FileName;

                FileStream fs = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);

                reader.BaseStream.Seek(128, SeekOrigin.Begin);
                string dicm = new string(reader.ReadChars(4));
                if (dicm != "DICM")
                {
                    MessageBox.Show("DICOM 파일이 아닙니다.");
                    return;
                }

                long pixelDataOffset = 1024;
                int width = 512;
                int height = 512;

                reader.BaseStream.Seek(pixelDataOffset, SeekOrigin.Begin);
                ushort[] buffer16 = new ushort[width * height];

                for (int i = 0; i < width * height; i++)
                {
                    buffer16[i] = reader.ReadUInt16();
                }

                byte[] buffer8 = new byte[width * height];
                for (int i = 0; i < width * height; i++)
                {
                    buffer8[i] = (byte)(buffer16[i] >> 8);
                }

                WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), buffer8, width, 0);

                imgBox.Source = wb;
            }
        }

        private void btnCalculateHistogram_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("진행 중 ...");
        }

        private void btnHistogramChart_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("진행 중 ...");
        }

        private void btnLUT_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("진행 중 ...");
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
