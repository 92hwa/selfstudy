using System;
using System.Globalization;
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

        #region Variables
        string selectedFilePath, selectedFileExt;

        int width, height;
        ushort[] buffer16;
        byte[] buffer8;

        double alpha = 1.0;
        int beta = 0;

        double windowW;
        double windowC;

        byte[] iBuffer;
        byte[] oBuffer;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Load File
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
            SetImage(buffer8, width, height, imgBoxResult, imgBoxResultHistogram);
            txtBox.Text = $"선택한 파일: {selectedFilePath} \n";
        }
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

                switch (group)
                {
                    case 0x0028:
                        switch (element)
                        {
                            case 0x0010: height = BitConverter.ToUInt16(valueBytes, 0); break;
                            case 0x0011: width = BitConverter.ToUInt16(valueBytes, 0); break;
                            case 0x1050:
                                {
                                    string s = Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ');
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        var first = s.Split('\\')[0];
                                        if (double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out double wc))
                                            windowC = wc;
                                    }
                                }
                                break;
                            case 0x1051:
                                {
                                    string s = Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ');
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        var first = s.Split('\\')[0];
                                        if (double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out double ww))
                                            windowW = ww;
                                    }
                                }
                                break;
                        }
                        break;

                    case 0x7FE0 when element == 0x0010:
                        pixelData = valueBytes;
                        break;
                }

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
        private void SetImage(byte[] buffer, int width, int height, Image imageControl, Image histControl)
        {
            WriteableBitmap wbLocal = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
            wbLocal.WritePixels(new Int32Rect(0, 0, width, height), buffer, width, 0);
            imageControl.Source = wbLocal;

            WriteableBitmap histBitmap = Draw(buffer);
            histControl.Source = histBitmap;
        }
        #endregion


        #region Contrast (1.0 <= alpha <= 3.0)
        private void btnContrastUp_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for (int i = 0; i < iBuffer.Length; i++)
            {
                double contrastOValue = iBuffer[i] * alpha;

                // 오버플로우 방지
                if (contrastOValue > 255) contrastOValue = 255;
                if (contrastOValue < 0) contrastOValue = 0;

                oBuffer[i] = (byte)contrastOValue;
            }

            iBuffer = (byte[])oBuffer.Clone();

            SetImage(iBuffer, width, height, imgBoxResult, imgBoxResultHistogram);

            alpha = alpha + 0.1;
            txtBox.Text += $"현재 Contrast 값: {alpha:F2} \n";

            if (alpha > 3.0)
            {
                MessageBox.Show("더 이상 증가시킬 수 없습니다.");
                alpha = 3.0;
            }
        }
        private void btnContrastDown_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for (int i = 0; i < iBuffer.Length; i++)
            {
                double contrastOValue = iBuffer[i] * (1 / alpha);
                if (contrastOValue > 255) contrastOValue = 255;
                if (contrastOValue < 0) contrastOValue = 0;
                oBuffer[i] = (byte)contrastOValue;
            }
            iBuffer = (byte[])oBuffer.Clone();

            SetImage(oBuffer, width, height, imgBoxResult, imgBoxResultHistogram);

            alpha = alpha - 0.1;
            txtBox.Text += $"현재 Contrast 값: {alpha} \n";

            if (alpha < 0)
            {
                MessageBox.Show("더 이상 감소시킬 수 없습니다.");
                alpha = 0.1;
            }
        }
        private void btnContrastInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];
            alpha = 1;

            for (int i = 0; i < iBuffer.Length; i++)
            {
                double contrastOValue = buffer8[i];
                if (contrastOValue > 255) contrastOValue = 255;
                if (contrastOValue < 0) contrastOValue = 0;
                oBuffer[i] = (byte)contrastOValue;
            }

            iBuffer = (byte[])oBuffer.Clone();

            SetImage(iBuffer, width, height, imgBoxResult, imgBoxResultHistogram);

            txtBox.Text += $"현재 Contrast 값: {alpha} \n";
        }
        #endregion


        #region Brightness (0 <= beta <= 100)
        private void btnBrightUp_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for (int i = 0; i < iBuffer.Length; i++)
            {
                int brightOValue = iBuffer[i] + beta;
                if (brightOValue > 255) brightOValue = 255;
                if (brightOValue < 0) brightOValue = 0;
                oBuffer[i] = (byte)brightOValue;
            }
            iBuffer = (byte[])oBuffer.Clone();

            SetImage(iBuffer, width, height, imgBoxResult, imgBoxResultHistogram);

            beta = beta + 10;
            txtBox.Text += $"현재 Brightness 값: {beta} \n";

            if (beta > 100)
            {
                MessageBox.Show("더 이상 증가시킬 수 없습니다. \n");
                beta = 100;
            }
        }
        private void btnBrightDown_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for (int i = 0; i < iBuffer.Length; i++)
            {
                double brightOValue = iBuffer[i] - beta;
                if (brightOValue > 255) brightOValue = 255;
                if (brightOValue < 0) brightOValue = 0;
                oBuffer[i] = (byte)brightOValue;
            }
            iBuffer = (byte[])oBuffer.Clone();

            SetImage(iBuffer, width, height, imgBoxResult, imgBoxResultHistogram);

            beta = beta - 10;
            txtBox.Text += $"현재 Brightness 값: {beta} \n";

            if (beta < 0)
            {
                MessageBox.Show("더 이상 감소시킬 수 없습니다. \n");
                beta = 0;
            }
        }
        private void btnBrightInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])buffer8.Clone();

            beta = 0;

            for (int i = 0; i < iBuffer.Length; i++)
            {
                double brightOValue = iBuffer[i];
                if (brightOValue > 255) brightOValue = 255;
                if (brightOValue < 0) brightOValue = 0;
                oBuffer[i] = (byte)brightOValue;
            }

            iBuffer = (byte[])oBuffer.Clone();

            SetImage(iBuffer, width, height, imgBoxResult, imgBoxResultHistogram);

            txtBox.Text += $"현재 Brightness 값: {beta}";
        }
        #endregion


        #region LUT
        private void btnLut_Click(object sender, RoutedEventArgs e)
        {
            if (buffer16 == null && buffer8 == null) return;

            ushort[] sourceBuffer16;

            if (buffer16 != null) sourceBuffer16 = buffer16;
            else
            {
                sourceBuffer16 = new ushort[buffer8.Length];
                for (int i = 0; i < buffer8.Length; i++)
                    sourceBuffer16[i] = (ushort)(buffer8[i] << 8);
            }

            if (iBuffer == null)
                iBuffer = (byte[])buffer8.Clone();

            byte[] resultBuffer = new byte[iBuffer.Length];
            byte[] lutArr = new byte[65536];

            for (int i = 0; i < 65536; i++)
            {
                double lutVal;
                if (i <= windowC - 0.5 - (windowW - 1) / 2.0) lutVal = 0;
                else if (i > windowC - 0.5 + (windowW - 1) / 2.0) lutVal = 255;
                else lutVal = ((i - (windowC - 0.5)) / (windowW - 1) + 0.5) * 255.0;

                lutVal = lutVal * alpha + beta;

                if (lutVal < 0) lutVal = 0;
                if (lutVal > 255) lutVal = 255;

                lutArr[i] = (byte)lutVal;
            }

            for (int i = 0; i < sourceBuffer16.Length; i++)
                resultBuffer[i] = lutArr[sourceBuffer16[i]];

            WriteableBitmap wbLut = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
            wbLut.WritePixels(new Int32Rect(0, 0, width, height), resultBuffer, width, 0);
            imgBoxResult.Source = wbLut;

            WriteableBitmap histBitmap = Draw(resultBuffer, lutArr);
            imgBoxResultHistogram.Source = histBitmap;

            //txtBox.Text += $"적용된 alpha: {alpha:F2}, 적용된 beta: {beta} \n";
            txtBox.Text += $"WW/WC 기반 LUT 적용: Window Center: {windowC}, Window Width: {windowW}";
        }
        private static void DrawLine(byte[] pixels, int stride, int width, int height, int x1, int y1, int x2, int y2, Color color)
        {
            int dx = Math.Abs(x2 - x1), dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                {
                    int idx = y1 * stride + x1 * 4;
                    pixels[idx] = color.B;
                    pixels[idx + 1] = color.G;
                    pixels[idx + 2] = color.R;
                    pixels[idx + 3] = 255;
                }

                if (x1 == x2 && y1 == y2) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x1 += sx; }
                if (e2 < dx) { err += dx; y1 += sy; }
            }
        }
        #endregion


        #region Histogram
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
        public static WriteableBitmap Draw(byte[] buffer, byte[] lut = null)
        {
            if (buffer == null || buffer.Length == 0) return null;

            // 캔버스 그리기
            int histW = 512, histH = 400;
            WriteableBitmap histBitmap = new WriteableBitmap(histW, histH, 96, 96, PixelFormats.Bgr32, null);

            int histStride = histW * 4;
            byte[] pixels = new byte[histH * histStride];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 255;

            // 히스토그램 계산
            int[] histogram = CalculateHistogram(buffer);
            int maxVal = histogram.Max();
            double binW = (double)histW / histogram.Length;

            // 히스토그램 그리기
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

            //LUT 곡선 그리기 
            if (lut != null)
            {
                for (int x = 0; x < 255; x++)
                {
                    int x1 = (int)((double)x / 255 * histW);
                    int y1 = histH - 1 - (int)((double)lut[x] / 255 * histH);

                    int x2 = (int)((double)(x + 1) / 255 * histW);
                    int y2 = histH - 1 - (int)((double)lut[x + 1] / 255 * histH);

                    DrawLine(pixels, histStride, histW, histH, x1, y1, x2, y2, Colors.Red);
                }
            }
            histBitmap.WritePixels(new Int32Rect(0, 0, histW, histH), pixels, histStride, 0);
            return histBitmap;
        }
        #endregion

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}