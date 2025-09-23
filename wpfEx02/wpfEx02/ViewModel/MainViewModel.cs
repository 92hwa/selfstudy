using System.IO;
using wpfEx02.Model;
using Microsoft.Win32;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace wpfEx02.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Load _loader = new Load();

        [ObservableProperty]
        private BitmapSource image = null;

        public ICommand LoadCommand { get; }

        public MainViewModel()
        {
            LoadCommand = new RelayCommand(OnLoad);
        }

        private void OnLoad()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files|*.*|RAW Files|*.raw|DICOM Files|*.dcm";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                string selectedFileExt = Path.GetExtension(selectedFilePath);

                switch (selectedFileExt)
                {
                    case "dcm":
                        {
                            _loader.LoadDicom(openFileDialog.FileName);
                            image = BitmapSource.Create(_loader.Width, _loader.Height, 96, 96, pixelFormats.Gray8, null, _loader.Buffer8, _loader.Width);
                        }
                        break;
                    case "raw":
                        {

                        }
                        break;
                }
            }
        }
    }
}
