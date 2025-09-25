using System.IO;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using wpfEx02.Model;

namespace wpfEx02.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Load _loader = new Load();

        private byte[] iBuffer;
        private byte[] oBuffer;

        private double alpha = 1.0;
        private int beta = 0;

        [ObservableProperty]
        private BitmapSource image;

        public BitmapSource Image 
        { 
            get => image; 
            set => SetProperty(ref image, value); 
        }

        public ICommand LoadCommand { get; }

        public ICommand ContrastUpCommand { get; }
        public ICommand ContrastDownCommand { get; }
        
        public ICommand BrightUpCommand { get; }
        public ICommand BrightDownCommand { get; }

        public ICommand ApplyLUTCommand { get; }

        public ICommand InitCommand { get; }

        public MainViewModel()
        {
            LoadCommand = new RelayCommand(OnLoad);

            ContrastUpCommand = new RelayCommand(ContrastUp);
            ContrastDownCommand = new RelayCommand(ContrastDown);

            BrightUpCommand = new RelayCommand(BrightUp);
            BrightDownCommand = new RelayCommand(BrightDown);

            ApplyLUTCommand = new RelayCommand(ApplyLUT);
            InitCommand = new RelayCommand(Init);

        }

        private void OnLoad()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Files|*.*|RAW Files|*.raw|DICOM Files|*.dcm|Image Files|*.png;*.jpg;",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                string selectedFileExt = Path.GetExtension(selectedFilePath);

                FileStream fs = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);

                switch (selectedFileExt)
                {
                    case ".dcm":
                        _loader.LoadDicom(reader);
                        SetImage(_loader.Buffer8, _loader.Width, _loader.Height);
                        break;
                    case ".raw":
                        var raw = selectedFileExt;
                        break;
                }
            }
        }

        private void SetImage(byte[] buffer, int width, int height)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
            wb.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), buffer, width, 0);
            Image = wb;
        }

        private void ContrastUp()
        {
            if (_loader.Buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])_loader.Buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for(int i = 0; i < iBuffer.Length; i++)
            {
                double val = iBuffer[i] * alpha;

                if (val > 255) val = 255;
                if (val < 0) val = 0;

                oBuffer[i] = (byte)val;
            }

            iBuffer = (byte[])oBuffer.Clone();
            SetImage(iBuffer, _loader.Width, _loader.Height);

            alpha = alpha + 0.1;

            if (alpha > 3.0)
            {
                alpha = 3.0;
            }
        }

        private void ContrastDown()
        {
            if (_loader.Buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])_loader.Buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for (int i = 0; i < iBuffer.Length; i++)
            {
                double val = iBuffer[i] / alpha;

                if (val > 255) val = 255;
                if (val < 0) val = 0;

                oBuffer[i] = (byte)val;
            }

            iBuffer = (byte[])oBuffer.Clone();
            SetImage(iBuffer, _loader.Width, _loader.Height);

            alpha = alpha - 0.1;

            if (alpha < 0.0)
            {
                alpha = 0.0;
            }
        }

        private void BrightUp()
        {
            if (_loader.Buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])_loader.Buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for(int i = 0; i < iBuffer.Length; i++)
            {
                int val = iBuffer[i] + beta;
                if (val > 255) val = 255;
                if (val < 0) val = 0;
                oBuffer[i] = (byte)val;
            }
            iBuffer = (byte[])oBuffer.Clone();
            SetImage(iBuffer, _loader.Width, _loader.Height);

            beta = beta + 10;

            if (beta > 100)
            {
                beta = 100;
            }
        }

        private void BrightDown()
        {
            if (_loader.Buffer8 == null) return;

            if (iBuffer == null)
                iBuffer = (byte[])_loader.Buffer8.Clone();

            oBuffer = new byte[iBuffer.Length];

            for(int i = 0; i < iBuffer.Length; i++)
            {
                int val = iBuffer[i] + beta;
                if (val > 255) val = 255;
                if (val < 0) val = 0;
                oBuffer[i] = (byte)val;
            }
            iBuffer = (byte[])oBuffer.Clone();
            SetImage(iBuffer, _loader.Width, _loader.Height);

            beta = beta - 10;

            if (beta < 0)
            {
                beta = 0;
            }
        }

        private void ApplyLUT()
        {
            if (_loader.Buffer8 == null) return;

            ushort[] sourceBuffer16;
            sourceBuffer16 = new ushort[_loader.Buffer8.Length];
            for (int i = 0; i < _loader.Buffer8.Length; i++)
            {
                sourceBuffer16[i] = (ushort)(_loader.Buffer8[i] << 8);
            }

            if (iBuffer == null)
                iBuffer = (byte[])_loader.Buffer8.Clone();

            byte[] resultBuffer = new byte[iBuffer.Length];
            byte[] lutArr = new byte[65535];

            double windowC = _loader.WindowC;
            double windowW = _loader.WindowW;

            for(int i = 0; i < 65535; i++)
            {
                double lutVal;

                if (i <= windowC - 0.5 - (windowW - 1) / 2.0) lutVal = 0;
                else if (i > windowC - 0.5 + (windowW - 1) / 2.0) lutVal = 255;
                else lutVal = ((i - (windowC - 0.5)) / (windowW - 1) + 0.5) * 255.0;

                lutVal = lutVal * alpha + beta;

                if (lutVal > 255) lutVal = 255;
                if (lutVal < 0) lutVal = 0;

                lutArr[i] = (byte)lutVal;
            }

            for (int i = 0; i < sourceBuffer16.Length; i++)
                resultBuffer[i] = lutArr[sourceBuffer16[i]];

            var wbLut = new WriteableBitmap(_loader.Width, _loader.Height, 96, 96, PixelFormats.Gray8, null);
            wbLut.WritePixels(new System.Windows.Int32Rect(0, 0, _loader.Width, _loader.Height), resultBuffer, _loader.Width, 0);
            SetImage(resultBuffer, _loader.Width, _loader.Height);
        }

        private void Init()
        {
            if (_loader.Buffer8 == null) return;

            iBuffer = (byte[])_loader.Buffer8.Clone();
            oBuffer = new byte[iBuffer.Length];

            alpha = 1.0;
            beta = 0;

            SetImage(_loader.Buffer8, _loader.Width, _loader.Height);
        }
    }
}
