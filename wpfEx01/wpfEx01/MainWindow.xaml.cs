using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCvSharp;
using System.Linq;

namespace wpfEx01
{

    public partial class MainWindow : System.Windows.Window
    {
        OpenFileDialog openFileDialog;
        string selectedFilePath;
        string selectedFileExt;
        WriteableBitmap wb;

        int width;
        int height;

        int[] histogram;
        ushort[] buffer16;
        byte[] buffer8;
        byte[] pixels;
        byte gray;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Image Load
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files|*.*|RAW Files|*.raw|DICOM Files|*.dcm|JPEG Files|*.jpeg;*.jpg";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                selectedFileExt = Path.GetExtension(selectedFilePath);

                FileStream fs = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read); // 파일을 바이너리 모드로 열기
                BinaryReader reader = new BinaryReader(fs);

                if (selectedFileExt == ".dcm") // DICOM 파일 읽기
                {
                    byte[] pixelData = null;

                    reader.BaseStream.Seek(128, SeekOrigin.Begin); // Preamble 건너뛰기
                    string dicm = new string(reader.ReadChars(4)); // Prefix 확인

                    while (reader.BaseStream.Position < reader.BaseStream.Length) // DICM 이후 Tag 읽기
                    {
                        ushort group = reader.ReadUInt16(); // Tag Group
                        ushort element = reader.ReadUInt16(); // Tag Element
                        string vr = Encoding.ASCII.GetString(reader.ReadBytes(2)); // VR

                        int vl = 0;
                        if (vr == "OB" || vr == "OW" || vr == "SQ" || vr == "UN") // Explicit VR
                        {
                            reader.ReadUInt16();
                            vl = (int)reader.ReadUInt32();
                        }

                        else // Implicit VR
                        {
                            vl = reader.ReadUInt16();
                        }
                        byte[] valueBytes = reader.ReadBytes(vl); // Field

                        if (group == 40 && element == 16) // Rows
                        {
                            height = BitConverter.ToUInt16(valueBytes, 0);
                        }

                        else if (group == 40 && element == 17) // Columns
                        {
                            width = BitConverter.ToUInt16(valueBytes, 0);
                        }

                        else if (group == 32736 && element == 16) // Pixel Data
                        {
                            pixelData = valueBytes;
                        }

                        else if (height > 0 && width > 0 && pixelData != null)
                        {
                            break;
                        }
                    }

                    buffer8 = new byte[height * width];
                    for (int i = 0; i < buffer8.Length; i++)
                    {
                        if (i * 2 + 1 < pixelData.Length)
                        {
                            ushort value16 = (ushort)(pixelData[i * 2] | (pixelData[i * 2 + 1] << 8)); // 16비트 픽셀
                            buffer8[i] = (byte)(value16 >> 8); // 상위 8비트만 저장
                        }
                    }
                }

                else if (selectedFileExt == ".raw")
                {
                    width = 3072;
                    height = 3072;

                    buffer16 = new ushort[(int)(width * height)];
                    buffer8 = new byte[width * height];

                    for (int i = 0; i < width * height; i++)
                    {
                        ushort value = (ushort)reader.ReadUInt16();
                        buffer16[i] = value;
                    }

                    for (int j = 0; j < width * height; j++)
                    {
                        buffer8[j] = (byte)(buffer16[j] >> 8);
                    }
                }

                wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null); //  8비트 흑백 비트맵 메모리 공간 생성
                wb.WritePixels(new Int32Rect(0, 0, width, height), buffer8, width, 0); // buffer8의 데이터를 비트맵에 그대로 복사해서 화면에 띄우기
                imgBox.Source = wb;

                reader.Close();
                fs.Close();
            }
            txtBox.Text = $"선택한 파일: {selectedFilePath} \n\n";
        }
        #endregion


        #region Draw Histogram Chart
        private void btnHistogramChart_Click(object sender, RoutedEventArgs e)
        {
            if (wb == null)
            {
                MessageBox.Show("먼저 파일을 불러와주세요.");
                return;
            }

            int[] histogram = new int[256];
            int[] histogramAvg = new int[256];

            for (int i = 0; i < buffer8.Length; i++)
            {
                histogram[buffer8[i]]++;
            }

            txtBox.Text += "Histogram Output *** \n";
            for (int i = 0; i < 256; i++)
            {
                //txtBox.Text += $"[{i}]: {(histogram[i]/histogram.Average())}\n";
                histogramAvg[i] = (int)((histogram[i]) / (histogram.Average()));
                txtBox.Text += $"histogramAvg[{i}]: {histogramAvg[i]} \n";
            }
            /*txtBox.Text += $"Max: {histogram.Max()}\n";
            txtBox.Text += $"Min: {histogram.Min()}\n";
            txtBox.Text += $"Sum: {histogram.Sum()}\n";
            txtBox.Text += $"Avg: {histogram.Average()}\n";*/

            txtBox.Text += $"Max: {histogramAvg.Max()}\n";
            txtBox.Text += $"Min: {histogramAvg.Min()}\n";

            int histW = 500;
            int histH = 400;
            WriteableBitmap histBitmap = new WriteableBitmap(histW, histH, 96, 96, PixelFormats.Bgr32, null);
            int stride = histW * 4;
            byte[] pixels = new byte[histH * stride];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 255;
            }

            double binW = (double)histW / histogram.Length; // x축 단위
            int maxVal = histogram.Max(); // y축 최대값

            for (int i = 0; i < histogram.Length; i++)
            {
                int barHeight = (int)((double)histogram[i] / maxVal * histH);

                int xStart = (int)(i * binW);
                int xEnd = (int)((i + 1) * binW);

                for (int j = histH - 1; j >= histH - barHeight; j--) // y축
                {
                    for (int k = xStart; k < xEnd; k++) // x축
                    {
                        int index = j * stride + k * 4;
                        pixels[index] = 0;       // Blue
                        pixels[index + 1] = 0;   // Green
                        pixels[index + 2] = 0;   // Red
                        pixels[index + 3] = 255; // Alpha
                    }
                }
            }


            histBitmap.WritePixels(new Int32Rect(0, 0, histW, histH), pixels, stride, 0);

            ChildWindow child = new ChildWindow();
            child.SetImage(histBitmap);
            child.Owner = this;
            child.Show();
        }
        #endregion



        private void btnLUT_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null || histogram == null)
            {
                MessageBox.Show("먼저 파일을 불러오고 히스토그램을 계산 해 주세요.");
                return;
            }

            // LUT 생성 (히스토그램 평활화)
            int totalPixels = buffer8.Length;
            int[] cumHist = new int[256];
            cumHist[0] = histogram[0];

            for (int i = 1; i < 256; i++)
            {
                cumHist[i] = cumHist[i - 1] + histogram[i];
            }

            byte[] LUT = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                LUT[i] = (byte)((cumHist[i] - cumHist[0]) * 255 / (totalPixels - 1));
            }

            // LUT 적용
            byte[] lutBuffer = new byte[buffer8.Length];
            for (int i = 0; i < buffer8.Length; i++)
            {
                lutBuffer[i] = LUT[buffer8[i]];
            }

            // 새로운 WriteableBitmap 생성
            WriteableBitmap lutBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
            lutBitmap.WritePixels(new Int32Rect(0, 0, width, height), lutBuffer, width, 0);

            // 이미지 컨트롤에 적용
            ChildWindow child = new ChildWindow();
            child.SetImage(lutBitmap);
            child.Owner = this;
            child.Show();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
