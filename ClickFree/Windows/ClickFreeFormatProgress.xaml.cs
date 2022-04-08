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
    /// Interaction logic for ClickFreeFormatProgress.xaml
    /// </summary>
    public partial class ClickFreeFormatProgress : Window
    {
        public ClickFreeFormatProgress()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
