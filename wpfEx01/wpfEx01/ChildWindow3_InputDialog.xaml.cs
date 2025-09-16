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
    /// ChildWindow3_InputDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChildWindow3_InputDialog : Window
    {
        public double contrastValue { get; private set; }

        public ChildWindow3_InputDialog()
        {
            InitializeComponent();
            txtInputValue.Focus();
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {

            if (double.TryParse(txtInputValue.Text, out double val))
            {
                contrastValue = val;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("숫자를 입력 해 주세요.");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
