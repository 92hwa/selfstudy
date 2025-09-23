using System;
using System.IO;
using System.Text;
using System.Globalization;


namespace wpfEx02.Model
{
    public class Load
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public double WindowC { get; private set; } = 0;
        public double WindowW { get; private set; } = 0;
        public byte[] Buffer8 { get; private set; }

        // raw
        public byte[] LoadRaw(string path)
        {
            return File.ReadAllBytes(path);
        }

        // dicom
        public void LoadDicom(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            byte[] pixelData = null;
            reader.BaseStream.Seek(128, SeekOrigin.Begin);
            string dicm = new string(reader.ReadChars(4));

            while(reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ushort group = reader.ReadUInt16();
                ushort element = reader.ReadUInt16();
                string vr = Encoding.ASCII.GetString(reader.ReadBytes(2));

                int vl = 0;
                if (vr == "OB" || vr == "OW" || vr == "SQ" || vr=="UN")
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
                            case 0x0010: Height = BitConverter.ToUInt16(valueBytes, 0); break;
                            case 0x0011: Width = BitConverter.ToUInt16(valueBytes, 0); break;
                            case 0x1050:
                                {
                                    string s = Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ');
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        var first = s.Split('\\')[0];
                                        if (double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out double wc)) WindowC = wc;
                                    }
                                }
                                break;
                            case 0x1051:
                                {
                                    string s = Encoding.ASCII.GetString(valueBytes).Trim('\0', ' ');
                                    if(!string.IsNullOrEmpty(s))
                                    {
                                        var first = s.Split('\\')[0];
                                        if (double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out double ww)) WindowW = ww;
                                    }
                                }
                                break;
                        }
                        break;
                    case 0x7FE0 when element == 0x0010: pixelData = valueBytes; break;
                }
                if (Height > 0 && Width > 0 && pixelData != null) break;
            }
            byte[] buffer = new byte[Width * Height];

            for(int i = 0; i < buffer.Length; i++)
            {
                if (i * 2 + 1 < pixelData.Length)
                {
                    ushort value16 = (ushort)(pixelData[i * 2] | (pixelData[i * 2 + 1] << 8));
                    buffer[i] = (byte)(value16 >> 8);
                }
            }
        }
    }
}
