using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Microsoft.Win32;
using OpenCvSharp;

namespace rawEx01
{

    public partial class MainWindow : System.Windows.Window
    {
        string selectedFilePath;
        WriteableBitmap wb;
        
        int width = 3072;
        int height = 3072;

        int[] histogram;
        int sum;
        int avg;

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

        private void btnCalculateHistogram_Click(object sender, RoutedEventArgs e)
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

        private void btnHistogramChart_Click(object sender, RoutedEventArgs e)
        {
            if (histogram == null)
            {
                MessageBox.Show("먼저 히스토그램을 계산 해 주세요.");
                return;
            }

            // 차트 크기
            int histW = 512;
            int histH = 400;
            int binW = histW / 256;

            //최대값 찾기 (정규화용)
            int maxVal = histogram.Max();

            // 비트맵 생성
            WriteableBitmap histBitmap = new WriteableBitmap(histW, histH, 96, 96, PixelFormats.Bgr32, null);
            int stride = histW * 4;
            byte[] pixels = new byte[histH * stride];
            
            // 전체 배경을 흰색으로 초기화
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;        // Blue
                pixels[i + 1] = 255;    // Green
                pixels[i + 2] = 255;    // Red
                pixels[i + 3] = 255;    // Alpha
            }

            // 히스토그램 막대 그리기 
            for (int i = 0; i < 256; i++)
            {
                int barHeight = (int)((double)histogram[i] / maxVal * histH);

                for (int y = histH - 1; y >= histH - barHeight; y--)
                {
                    for (int x = i * binW; x < (i + 1) * binW; x++)
                    {
                        int index = y * stride + x * 4;

                        pixels[index] = 0;              // Blue
                        pixels[index + 1] = 0;        // Green
                        pixels[index + 2] = 0;        // Red
                        pixels[index + 3] = 255;    // Alpha
                    }
                }
            }

            histBitmap.WritePixels(new Int32Rect(0, 0, histW, histH), pixels, stride, 0);

            ChildWindow child = new ChildWindow();
            child.SetImage(histBitmap);
            child.Owner = this;
            child.Show();


            /*
            // OpenCV 사용
            Mat histImage = new Mat(histH, histW, MatType.CV_8UC3, Scalar.All(255));

            // 값을 0 ~ hist 범위로 변환
            int maxVal = histogram.Max();
            float[] normHist = new float[256];

            for (int i = 0; i < 256; i++)
            {
                normHist[i] = (float)histogram[i] / maxVal * histW;
            }

            for (int i = 1; i < 256; i++)
            {
                Cv2.Line(histImage,
                    new OpenCvSharp.Point((i - 1) * binW, histH - (int)normHist[i - 1]),
                    new OpenCvSharp.Point(i * binW, histH - (int)normHist[i]),
                    Scalar.Black, 2, LineTypes.AntiAlias, 0);
            }

            Cv2.ImShow("Histogram", histImage);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
            */
        }



        private void btnLUT_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("진행 중");
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
