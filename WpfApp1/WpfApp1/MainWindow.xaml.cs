using System;
using System.Windows;
//using OpenCvSharp;
using System.IO;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                label1.Content = $"선택한 파일: {selectedFilePath}\n";

                FileStream fs = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);

                int width = 3072, height = 3072;

                ushort[] buf = new ushort[(int)(width * height)];

                for (int i = 0; i < width * height; i++)
                {
                    ushort value = (ushort)reader.ReadUInt16();
                    buf[i] = value;
                    label1.Content += $"{ buf[i]}";
                }
            }
            
        }
    }
}
