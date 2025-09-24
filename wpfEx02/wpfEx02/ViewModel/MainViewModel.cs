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
        private readonly FileLoadException _loader = new Load();

        [ObservableProperty]
        private BitmapSource image = null;
        
        public ICommand LoadCommand { get; }

        public MainViewModel()
        {
            LoadCommand = new RelayCommand(OnLoad);
        }

        private void OnLoad()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Files|*.*|RAW Files|*.raw|DICOM Files|*.dcm|Image Files|*.png;*.jpg;",
                Multiselect = false;
            }
        }
    }
}
