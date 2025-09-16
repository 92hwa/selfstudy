using System.Windows;
using System.Windows.Media.Imaging;

namespace wpfEx01
{
    /// <summary>
    /// ChildWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChildWindow1_Histogram : Window
    {
        public ChildWindow1_Histogram()
        {
            InitializeComponent();
        }

        public void SetImage(WriteableBitmap bitmap)
        {
            imgBox2.Source = bitmap;
        }
    }
}
