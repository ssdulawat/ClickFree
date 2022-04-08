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
using ClickFree.Views;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for FormatClickFreeWindow.xaml
    /// </summary>
    public partial class FormatClickFreeWindow : Window
    {
        public FormatClickFreeWindow()
        {
            InitializeComponent();
        }

        public void CloseWindow(object sender, System.EventArgs e)
        {
            Hide();
       }

        public void Format(object sender, System.EventArgs e)
        {
            ConfirmationWindow win = new ConfirmationWindow();
            win.Show();
        }
    }
}
