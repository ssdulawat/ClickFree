using ClickFree.Helpers;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClickFree.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            BrushConverter bc = new BrushConverter();
            firstBorder.Background = (Brush)bc.ConvertFrom("#54BAF4");
            usbButton.Background = Brushes.Green;  //when connected  "Brushes.Red;" when disconnected
            connection.Content = "Connected";
            space.Content = "143 GB Available";
        }

        public void MainBtn(object sender, System.EventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            SettingsPanel.Visibility = Visibility.Hidden;
        }

        public void SettingsBtn(object sender, System.EventArgs e)
        {
            MainPanel.Visibility = Visibility.Hidden;
            SettingsPanel.Visibility = Visibility.Visible;
            firstBorder.Background = Brushes.Transparent;
        }

        public void EmailBtn(object sender, System.EventArgs e)
        {
            firstBorder.Background = Brushes.Transparent;
        }

        public void ChatBtn(object sender, System.EventArgs e)
        {
            firstBorder.Background = Brushes.Transparent;
        }

    }
}
