using System;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = "20121023_100709_CHEST_PA.raw";

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            int width = 3072, height = 3072;

            ushort[] buf = new ushort[(int)(width * height)];
            
            for (int i = 0; i < width * height; i++)
            {
                ushort value = (ushort)reader.ReadUInt16();
                buf[i] = value;
            }

            reader.Close();
            fs.Close();

            ushort minVal = ushort.MaxValue;
            ushort maxVal = ushort.MinValue;
            foreach (ushort v in buf)
            {
                if (v < minVal) minVal = v;
                if (v > maxVal) maxVal = v;
            }

            label1.Text =
                $"파일명: {Path.GetFileName(filePath)}\n" +
                $"총 파일 크기: {width} x {height} x 2 = {width * height * 2} 바이트 = {(width * height * 2) / 1048576}MB";
        }
    }
}
