using System;
using System.IO;
using System.Text;
using System.Globalization;
using wpfEx02.ViewModel;

namespace wpfEx02.Model
{
    public class Load
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public double WindowC { get; private set; } = 0;
        public double WindowW { get; private set; } = 0;

        public byte[] Buffer8 { get; private set; }

        public string LoadRaw(string path)
        {
            return path;
        }

        public byte[] LoadDicom(BinaryReader reader, MainViewModel vm = null)
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
                    case 0x0010 when element == 0x0010:
                        string pn = Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ');
                        if (vm != null) vm.PatientName = "PN  " + pn;
                        break;

                    case 0x0008 when element == 0x0020:
                        {
                            string studyDate = Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ');
                            if (vm != null) vm.StudyDate = $"CR StudyDate{studyDate}";
                        }break;
                    case 0x0028:
                        switch (element)
                        {
                            case 0x0010: Height = BitConverter.ToUInt16(valueBytes, 0); break;
                            case 0x0011:
                                {
                                    Width = BitConverter.ToUInt16(valueBytes, 0);
                                    vm.WidthHeight = $"{Width} x {Height}";
                                } break;
                            case 0x1050:
                                if (double.TryParse(Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ').Split('\\')[0],
                                        NumberStyles.Float, CultureInfo.InvariantCulture, out double wc))
                                    WindowC = wc;
                                break;
                            case 0x1051:
                                {
                                    if (double.TryParse(Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ').Split('\\')[0],
                                       NumberStyles.Float, CultureInfo.InvariantCulture, out double ww))
                                        WindowW = ww;
                                    vm.WWWC = $"{WindowW}/{WindowC}";
                                } break;
                        }
                        break;

                    case 0x7FE0 when element == 0x0010:
                        pixelData = valueBytes;
                        break;
                }
                if (Height > 0 && Width > 0 && pixelData != null) break;
            }

            if (pixelData == null)
                throw new Exception("픽셀 데이터를 읽을 수 없습니다.");

            Buffer8 = new byte[Width * Height];
            int numPixels = Math.Min(Buffer8.Length, pixelData.Length / 2);

            for (int i = 0; i < numPixels; i++)
            {
                ushort value16 = (ushort)(pixelData[i * 2] | (pixelData[i * 2 + 1] << 8));
                Buffer8[i] = (byte)(value16 >> 8);
            }
            return Buffer8;
        }
    }
}