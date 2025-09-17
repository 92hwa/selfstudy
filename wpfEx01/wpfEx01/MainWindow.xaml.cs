using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;

namespace wpfEx01
{
    public partial class MainWindow : System.Windows.Window
    {
        OpenFileDialog openFileDialog;
        string selectedFilePath;
        string selectedFileExt;

        int width;
        int height;

        ushort[] buffer16;
        byte[] buffer8;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog
            {
                Filter = "All Files|*.*|RAW Files|*.raw|DICOM Files|*.dcm",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() != true) return;

            selectedFilePath = openFileDialog.FileName;
            selectedFileExt = Path.GetExtension(selectedFilePath);

            FileStream fs = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read); // 파일을 바이너리 모드로 열기
            BinaryReader reader = new BinaryReader(fs);

            if (selectedFileExt == ".dcm")
            {
                LoadDICOM(reader);
            }
            else if (selectedFileExt == ".raw")
            {
                LoadRAW(reader);
            }

            SetImage(buffer8, width, height, imgBoxOriginal, imgBoxOriginalHistogram);
            txtBox.Text = $"선택한 파일: {selectedFilePath} \n\n";
        }

        private void btnLUT_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;

            int lutW = 256;
            int lutH = 256;

            WriteableBitmap lutBitmap = new WriteableBitmap(lutW, lutH, 96, 96, PixelFormats.Bgr32, null);

            int lutStride = lutW * 4;
            byte[] pixels = new byte[lutH * lutStride];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 255;
            }

            int[] histogram = CalculateHistogram(buffer8);

            // 누적 분포 계산
            int sumHistogram = histogram.Sum();
            int[] cdf = new int[256];
            cdf[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            // LUT 만들기 (0 ~ 255 범위로 정규화)
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                lut[i] = (byte)(255.0 * cdf[i] / sumHistogram);
            }

            // LUT Curve 그리기
            for (int x = 0; x < 256; x++)
            {
                int y = lut[x];
                int yy = lutH - 1 - y;

                if (yy >= 0 && yy < lutH)
                {
                    int idx = yy * lutStride + x * 4;
                    pixels[idx] = pixels[idx + 1] = pixels[idx + 2] = 0;
                    pixels[idx + 3] = 255;
                }
            }
            lutBitmap.WritePixels(new Int32Rect(0, 0, lutW, lutH), pixels, lutStride, 0);

            LutDialog lutDialog = new LutDialog();
            lutDialog.imgBoxLut.Source = lutBitmap;
            lutDialog.ShowDialog();
        }

        private void btnContrastUp_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;
            InputDialog dialog = new InputDialog();

            if (dialog.ShowDialog() == false) return;
            double userContrast = dialog.userValue;

            byte[] contrastBuffer = new byte[buffer8.Length];
            for (int i = 0; i < buffer8.Length; i++)
            {
                double newValue = buffer8[i] * userContrast;
                if (newValue > 255) newValue = 255;
                if (newValue < 0) newValue = 0;
                contrastBuffer[i] = (byte)newValue;
            }
            SetImage(contrastBuffer, width, height, imgBoxResult, imgBoxResultHistogram);
        }

        private void btnContrastDown_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;
            InputDialog dialog = new InputDialog();

            if (dialog.ShowDialog() == false) return;
            double userContrast = dialog.userValue;

            byte[] contrastBuffer = new byte[buffer8.Length];
            for (int i = 0; i < buffer8.Length; i++)
            {
                double newValue = buffer8[i] / userContrast;
                if (newValue > 255) newValue = 255;
                if (newValue < 0) newValue = 0;
                contrastBuffer[i] = (byte)newValue;
            }
            SetImage(contrastBuffer, width, height, imgBoxResult, imgBoxResultHistogram);
        }

        private void btnContrastInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;
            SetImage(buffer8, width, height, imgBoxResult, imgBoxResultHistogram);
        }

        private void btnBrightnessUp_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;
            InputDialog dialog = new InputDialog();

            if (dialog.ShowDialog() == false) return;
            double userBrightness = dialog.userValue;

            byte[] contrastBuffer = new byte[buffer8.Length];
            for (int i = 0; i < buffer8.Length; i++)
            {
                double newValue = buffer8[i] + userBrightness;
                if (newValue > 255) newValue = 255;
                if (newValue < 0) newValue = 0;
                contrastBuffer[i] = (byte)newValue;
            }
            SetImage(contrastBuffer, width, height, imgBoxResult, imgBoxResultHistogram);
        }

        private void btnBrightnessDown_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;
            InputDialog dialog = new InputDialog();

            if (dialog.ShowDialog() == false) return;
            double userBrightness = dialog.userValue;

            byte[] contrastBuffer = new byte[buffer8.Length];
            for (int i = 0; i < buffer8.Length; i++)
            {
                double newValue = buffer8[i] - userBrightness;
                if (newValue > 255) newValue = 255;
                if (newValue < 0) newValue = 0;
                contrastBuffer[i] = (byte)newValue;
            }
            SetImage(contrastBuffer, width, height, imgBoxResult, imgBoxResultHistogram);
        }

        private void btnBrightnessInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;
            SetImage(buffer8, width, height, imgBoxResult, imgBoxResultHistogram);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Load File
        private void LoadDICOM(BinaryReader reader)
        {
            byte[] pixelData = null;

            reader.BaseStream.Seek(128, SeekOrigin.Begin);
            string dicm = new string(reader.ReadChars(4));

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ushort group = reader.ReadUInt16();
                ushort element = reader.ReadUInt16();
                string vr = Encoding.ASCII.GetString(reader.ReadBytes(2));

                int vl = 0;
                if (vr == "OB" || vr == "OW" || vr == "SQ" || vr == "UN")
                {
                    reader.ReadUInt16();
                    vl = (int)reader.ReadUInt32();
                }
                else
                {
                    vl = reader.ReadUInt16();
                }

                byte[] valueBytes = reader.ReadBytes(vl);

                if (group == 40 && element == 16) height = BitConverter.ToUInt16(valueBytes, 0);
                else if (group == 40 && element == 17) width = BitConverter.ToUInt16(valueBytes, 0);
                else if (group == 32736 && element == 16) pixelData = valueBytes;

                if (height > 0 && width > 0 && pixelData != null) break;
            }
            buffer8 = ConvertTo8Bit(pixelData, width, height);
        }

        private void LoadRAW(BinaryReader reader)
        {
            width = 3072;
            height = 3072;

            buffer16 = new ushort[width * height];
            for (int i = 0; i < buffer16.Length; i++)
            {
                buffer16[i] = reader.ReadUInt16();
            }

            buffer8 = new byte[width * height];
            for (int i = 0; i < buffer16.Length; i++)
            {
                buffer8[i] = (byte)(buffer16[i] >> 8);
            }
        }

        private byte[] ConvertTo8Bit(byte[] pixelData, int width, int height)
        {
            byte[] buffer = new byte[width * height];
            for (int i = 0; i < buffer.Length; i++)
            {
                if (i * 2 + 1 < pixelData.Length)
                {
                    ushort value16 = (ushort)(pixelData[i * 2] | (pixelData[i * 2 + 1] << 8));
                    buffer[i] = (byte)(value16 >> 8);
                }
            }
            return buffer;
        }
        #endregion


        #region Preview Image
        private void SetImage(byte[] buffer, int width, int height, Image imageControl, Image histControl)
        {
            WriteableBitmap wbLocal = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
            wbLocal.WritePixels(new Int32Rect(0, 0, width, height), buffer, width, 0);
            imageControl.Source = wbLocal;

            WriteableBitmap histBitmap = DrawHistogram(buffer);
            histControl.Source = histBitmap;
        }
        #endregion


        #region Calculate Histogram
        private static int[] CalculateHistogram(byte[] buffer)
        {
            int[] histogram = new int[256];
            if (buffer == null || buffer.Length == 0) return histogram;
            foreach (var b in buffer)
            {
                histogram[b]++;
            }
            return histogram;
        }
        #endregion


        #region Draw Histogram
        public static WriteableBitmap DrawHistogram(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return null;

            int histW = 500, histH = 400;
            WriteableBitmap histBitmap = new WriteableBitmap(histW, histH, 96, 96, PixelFormats.Bgr32, null);

            int histStride = histW * 4;
            byte[] pixels = new byte[histH * histStride];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 255;
            }

            int[] histogram = CalculateHistogram(buffer);
            int maxVal = histogram.Max();
            double binW = (double)histW / histogram.Length;

            // 막대 그리기
            for (int i = 0; i < histogram.Length; i++)
            {
                int barHeight = (int)((double)histogram[i] / maxVal * histH);
                int xStart = (int)(i * binW);
                int xEnd = (int)((i + 1) * binW);

                for (int j = histH - 1; j >= histH - barHeight; j--)
                {
                    for (int k = xStart; k < xEnd; k++)
                    {
                        if (k < 0 || k >= histW || j < 0 || j >= histH) continue; 
                        int idx = j * histStride + k * 4;
                        pixels[idx] = pixels[idx + 1] = pixels[idx + 2] = 0;
                        pixels[idx + 3] = 255;
                    }
                }
            }

            // x축
            for (int i = 0; i < 16; i++)
            {
                int x = (int)(i * (histW / 16.0));
                for (int j = histH - 15; j < histH - 10; j++)
                {
                    for (int dx = 0; dx < 2; dx++)
                    {
                        int xx = x + dx;

                        if (xx >= histW) continue;

                        int idx = j * histStride + xx * 4;
                        pixels[idx] = pixels[idx + 1] = pixels[idx + 2] = 0;
                        pixels[idx + 3] = 255;
                    }
                }
            }

            // y축
            double[] yLabels = { 0.0, 0.25, 0.5, 0.75, 1.0 };
            for (int i = 0; i < yLabels.Length; i++)
            {
                int y = histH - (int)(yLabels[i] * histH);

                for (int j = 0; j < histW; j += 5)
                {
                    if (y >= 0 && y < histH && j >= 0 && j < histW)  
                    {
                        int idx = y * histStride + j * 4; 
                        pixels[idx] = pixels[idx + 1] = pixels[idx + 2] = 0; 
                        pixels[idx + 3] = 255; 
                    }
                }
            }
            histBitmap.WritePixels(new Int32Rect(0, 0, histW, histH), pixels, histStride, 0);
            return histBitmap;
        }
    }
    #endregion
}