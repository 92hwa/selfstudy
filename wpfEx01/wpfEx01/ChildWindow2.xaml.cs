using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpfEx01
{
    /// <summary>
    /// ChildWindow2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChildWindow2 : Window
    {
        public ChildWindow2()
        {
            InitializeComponent();
        }
        public void SetImage(WriteableBitmap bitmap)
        {
            imgBox3.Source = bitmap;
        }
    }
}
