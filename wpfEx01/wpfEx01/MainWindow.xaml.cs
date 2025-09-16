using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Linq;

namespace wpfEx01
{

    public partial class MainWindow : System.Windows.Window
    {

        #region 사용할 변수 선언
        OpenFileDialog openFileDialog;
        string selectedFilePath;
        string selectedFileExt;
        WriteableBitmap wb;

        int width;
        int height;

        ushort[] buffer16;
        byte[] buffer8;
        #endregion

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

                if (selectedFileExt == ".dcm")
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

        #region Histogram
        private void btnHistogramChart_Click(object sender, RoutedEventArgs e)
        {
            if (wb == null)
            {
                MessageBox.Show("먼저 파일을 불러와주세요.");
                return;
            }


            #region 히스토그램 계산
            int[] histogram = new int[256];
            for (int i = 0; i < buffer8.Length; i++)
            {
                histogram[buffer8[i]]++;
            }
            txtBox.Text += "Histogram Output *** \n";
            txtBox.Text += $"histogram.Max: {histogram.Max()} \n";
            txtBox.Text += $"histogram.Min: {histogram.Min()} \n";
            #endregion


            #region 히스토그램을 그릴 빈 이미지 생성 
            int histW = 500, histH = 400;
            WriteableBitmap histBitmap = new WriteableBitmap(histW, histH, 96, 96, PixelFormats.Bgr32, null);

            int stride = histW * 4;
            // stride는 한 줄(row)의 바이트 수
            // 픽셀 포맷이 Bgr32라서 1픽셀 = 4바이트
            // 따라서 한 줄에 필요한 바이트 수 = 가로 픽셀 * 4 = 2,000바이트

            byte[] pixels = new byte[histH * stride];
            // 히스토그램 이미지의 픽셀 데이터를 담을 배열
            // 길이는 세로 픽셀 * stride = 400 * 2,000 = 800,000
            // 이 배열에 RGB(A) 값을 직접 넣어서 이미지 그리기 가능

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 255;
            }
            #endregion


            #region 히스토그램 정규화 (y축 축소를 위한)
            byte[][] colors = new byte[3][]
            {
                new byte[]{ 255, 0, 0 },
                new byte[]{ 0, 255, 0 },
                new byte[]{ 0, 0, 255 }
            };
            int maxVal = histogram.Max();
            double[] histNormalized = new double[histogram.Length];
            double binW = (double)histW / histogram.Length;
            for (int i = 0; i < histogram.Length; i++)
            {
                int barheight = (int)((double)histogram[i] / maxVal * histH);
                int xStart = (int)(i * binW);
                int xEnd = (int)((i + 1) * binW);
                histNormalized[i] = (double)histogram[i] / maxVal;

                byte[] color = colors[i % 3];

                for (int j = histH - 1; j >= histH - barheight; j--)
                {
                    for (int k = xStart; k < xEnd; k++)
                    {
                        int index = j * stride + k * 4;
                        pixels[index] = color[2]; // B
                        pixels[index + 1] = color[1]; // G
                        pixels[index + 2] = color[0]; // R
                        pixels[index + 3] = 255; // A
                    }
                }
            }
            #endregion


            #region x축 16픽셀 간격 눈금 (총 16개)
            int xTickCount = 16;
            for (int i = 0; i < xTickCount; i++)
            {
                int x = (int)(i * (histW / (double)xTickCount));
                for (int j = histH - 15; j < histH - 10; j++)
                {
                    for (int dx = 0; dx < 2; dx++)
                    {
                        int idx = j * stride + (x + dx) * 4;
                        if (idx + 3 < pixels.Length)
                        {
                            pixels[idx] = 0; // B
                            pixels[idx + 1] = 0; // G
                            pixels[idx + 2] = 0; // R
                            pixels[idx + 3] = 255; // A
                        }
                    }
                }
            }
            #endregion

            /* txtBox.Text += $"Normalized Max: {histNormalized.Max()}\n";
             txtBox.Text += $"Normalized Min: {histNormalized.Min()} \n";*/

            #region y축 값 범위 표시 (0%, 25%, 50%, 75%, 100%)
            double[] yLabels = { 0.0, 0.25, 0.5, 0.75, 1.0 };
            for (int i = 0; i < yLabels.Length; i++)
            {
                int y = histH - (int)(yLabels[i] * histH);
                for (int j = 0; j < histW; j += 5)
                {
                    int idx = y * stride + j * 4;
                    if (idx + 3 < pixels.Length)
                    {
                        pixels[idx] = 0;
                        pixels[idx + 1] = 0;
                        pixels[idx + 2] = 0;
                        pixels[idx + 3] = 255;
                    }
                }
            }
            #endregion

            histBitmap.WritePixels(new Int32Rect(0, 0, histW, histH), pixels, stride, 0);
            ChildWindow1_Histogram childHistogram = new ChildWindow1_Histogram();
            childHistogram.SetImage(histBitmap);
            childHistogram.Owner = this;
            childHistogram.Show();
        }
        #endregion

        #region Contrast
        private void btnContrast_Click(object sender, RoutedEventArgs e)
        {
            if (imgBox.Source != null && buffer8 != null)
            {
                ChildWindow2_Contrast childContrast = new ChildWindow2_Contrast(imgBox.Source, buffer8);
                childContrast.Owner = this;
                childContrast.Show();
            }
            else
            {
                MessageBox.Show("이미지를 불러온 후에 Contrast 창을 열어주세요.");
            }
        }
        #endregion


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
