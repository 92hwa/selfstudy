using System;                                       // 기본 클래스 (Console, Math 등)
using System.Windows.Forms;          // Windows Forms UI (버튼, 폼, PictureBox 등)
using System.IO;                                  // 파일 입출력 클래스 (FileStream, BinaryReader 등)
using System.Drawing;                       // 비트맵

namespace WindowsFormsApp3
{
    public partial class Form1 : Form           // Form1은 Form을 상속 (버튼, UI 이벤트 사용이 가능)
    {
        public Form1() // 생성자
        {
            InitializeComponent(); // Visual Studio 디자이너에서 만든 UI를 초기화하는 코드
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom; // 이미지 맞춤 표시
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = "20121023_100709_CHEST_PA.raw";
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read); // 파일을 열어서 스트림(바이트 단위 접근) 생성
            BinaryReader reader = new BinaryReader(fs); // 스트림에서 2진 데이터(정수, 실수 등)를 읽어주는 클래스

            int width = 3072, height = 3072; // 영상 해상도 정의, 3072 * 3072 픽셀로 흉부 엑스레이처럼 큰 이미지

            ushort[] buf = new ushort[(int)(width * height)];
            // 픽셀이 저장되는 버퍼
            // ushort 는 16비트 부호 없는 정수 (0 ~ 65535)
            // 크기는 width * height = 8,437,184 (약 9.4M 픽셀)


            for (int i = 0; i < width * height; i++) // 데이터 읽기 루프 , RAW 영상 데이터를 메모리로 로딩
            {
                ushort value = (ushort)reader.ReadUInt16(); // 파일에서 2바이트씩 읽어서 ushort 로 저장
                buf[i] = value; // 픽셀 데이터가 순서대로 buf 배열에 채워짐
            }


            // 리소스 해제
            reader.Close();
            fs.Close();


            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed); // ushort[] > 8비트 Grayscale Bitmap 변환


            // 팔레트(그레이스케일) 설정
            var palette = bmp.Palette;

            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = palette;


            // 픽셀 값 범위를 0~255 로 정규화
            ushort minVal = ushort.MaxValue;
            ushort maxVal = ushort.MinValue;
            foreach (ushort v in buf)
            {
                if (v < minVal) minVal = v;
                if (v > maxVal) maxVal = v;
            }

            // Bitmap 데이터 채우기
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);


            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        ushort val16 = buf[y * width + x];

                        // 0 ~ 255 로 스케일링 
                        byte val8 = (byte)((val16 - minVal) * 255.0 / (maxVal - minVal));
                        ptr[y * stride + x] = val8;
                    }
                }
            }
            bmp.UnlockBits(bmpData);

            // PictureBox 에 표시
            pictureBox1.Image = bmp;
        }
    }
}
