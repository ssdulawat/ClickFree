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
using ClickFree.Helpers;
using ClickFree.Views;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for FormatClickFreeWindow.xaml
    /// </summary>
    public partial class FormatClickFreeWindow : Window
    {
        List<UsbDisk> disks = new List<UsbDisk>();
        List<string> USBNameList = new List<string>();

        public FormatClickFreeWindow()
        {
            InitializeComponent();
        }

        public void CloseWindow(object sender, System.EventArgs e)
        {
            Close();
        }

        public void Format(object sender, System.EventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.FormatBtn.IsHitTestVisible = false;
                });

                ConfirmationWindow win = new ConfirmationWindow();
                win.Show();

                string selectedDrive = Convert.ToString(UsbListComboBox.SelectedValue.ToString().Split('-')[0]);
                ConfirmationWindow confirmation = new ConfirmationWindow(selectedDrive, this);
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            disks = DriveManager.GetAvailableDisks();
            foreach (var disk in disks)
            {
                var name = disk.Name.Split('\\')[0]+ "-Clickfree";
                USBNameList.Add(name);
            }
            UsbListComboBox.ItemsSource = USBNameList;
            FormatBtn.IsEnabled = false;
            UsbListComboBox.SelectedIndex = 0;
        }

        private void UsbListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsbListComboBox.SelectedValue == null)
            {
                FormatBtn.IsEnabled = false;
            }
            else
            {
                FormatBtn.IsEnabled = true;
            }
            
        }
    }
}
