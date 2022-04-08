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

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for ConfirmationWindow.xaml
    /// </summary>
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationWindow()
        {
            InitializeComponent();
        }


        public void YesButton_Click(object sender, System.EventArgs e)
        {
            ClickFreeFormatProgress win = new ClickFreeFormatProgress();
            win.Show();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
