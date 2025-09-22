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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "RAW Files|*.raw|All Files|*.*";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                MessageBox.Show("선택한 파일: " + selectedFilePath);

                FileStream fs = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);

                int width = 3072, height = 3072;
                ushort[] buffer16 = new ushort[(int)(width * height)]; // unsigned short 타입의 버퍼 배열은 3072 * 3072 크기를 갖도록 초기화

                for (int i = 0; i < width * height; i++)
                {
                    ushort value = (ushort)reader.ReadUInt16();
                    buffer16[i] = value;
                }

                byte[] buffer8 = new byte[width * height]; 
                
                for (int j=0; j < width * height; j++)
                {
                    buffer8[j] = (byte)(buffer16[j] >> 8);
                }

                WriteableBitmap wb = new WriteableBitmap(
                    width, height, 96, 96, PixelFormats.Gray8, null);

                wb.WritePixels(
                    new Int32Rect(0, 0, width, height),
                    buffer8, width, 0);

                imgBox.Source = wb;

                reader.Close();
                fs.Close();
            }
        }
    }
}
