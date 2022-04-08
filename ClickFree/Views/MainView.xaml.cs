using ClickFree.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        }

        public void MainBtn(object sender, System.EventArgs e)
        {
            bool ifDrive = DriveManager.HasUsbDrives;
            if (ifDrive == true)
            {
                MainPanel.Visibility = Visibility.Visible;
            }
            else
            {
                MainPanel.Visibility = Visibility.Hidden;
            }
            
            SettingsPanel.Visibility = Visibility.Hidden;
            AboutPanel.Visibility = Visibility.Hidden;
        }

        public void SettingsBtn(object sender, System.EventArgs e)
        {
            bool ifDrive = DriveManager.HasUsbDrives;
            if (ifDrive == true)
            {
                SettingsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsPanel.Visibility = Visibility.Hidden;
            }
            MainPanel.Visibility = Visibility.Hidden;
            AboutPanel.Visibility = Visibility.Hidden;

            firstBorder.Background = Brushes.Transparent;
        }

        private void AboutBtnClick(object sender, RoutedEventArgs e)
        {
            bool ifDrive = DriveManager.HasUsbDrives;
            if (ifDrive == true)
            {
                AboutPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AboutPanel.Visibility = Visibility.Hidden;
            }
            MainPanel.Visibility = Visibility.Hidden;
            SettingsPanel.Visibility = Visibility.Hidden;
           
            firstBorder.Background = Brushes.Transparent;
            
            disks = DriveManager.GetAvailableDisks();
            var disk = disks.FirstOrDefault();
            FirmwareVersionlbl.Content = disk.FirmwareRevision;

            Yearlbl.Content = "© " + DateTime.Now.Year + " Me Too Software, Inc. All rights reserved.";
        }

        public void EmailBtn(object sender, System.EventArgs e)
        {
            firstBorder.Background = Brushes.Transparent;
        }

        public void ChatBtn(object sender, System.EventArgs e)
        {
            firstBorder.Background = Brushes.Transparent;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            bool ifDrive = DriveManager.HasUsbDrives;
            if (ifDrive == true)
            {
                if (DriveManager.GetAvailableDisks().FirstOrDefault() != null)
                {
                    double bytesFs = DriveManager.GetAvailableDisks().FirstOrDefault().FreeSpace;
                    double kilobyteFs = bytesFs / 1024;
                    double megabyteFs = kilobyteFs / 1024;
                    double gigabyteFs = megabyteFs / 1024;

                    double bytesS = DriveManager.GetAvailableDisks().FirstOrDefault().Size;
                    double kilobyteS = bytesS / 1024;
                    double megabyteS = kilobyteS / 1024;
                    double gigabyteS = megabyteS / 1024;

                    usbButton.Background = Brushes.Green;
                    connection.Content = "Connected";
                    space.Content = (float)Math.Round(gigabyteFs, 1) + " GB available out of " + (float)Math.Round(gigabyteS, 1) + " GB";

                    if (mainButton.IsFocused)
                    {
                        MainPanel.Visibility = Visibility.Visible;
                        SettingsPanel.Visibility = Visibility.Hidden;
                        AboutPanel.Visibility = Visibility.Hidden;
                    }
                    else if (settingsButton.IsFocused)
                    {
                        MainPanel.Visibility = Visibility.Hidden;
                        SettingsPanel.Visibility = Visibility.Visible;
                        AboutPanel.Visibility = Visibility.Hidden;
                    }
                    else if (aboutButton.IsFocused)
                    {
                        MainPanel.Visibility = Visibility.Hidden;
                        SettingsPanel.Visibility = Visibility.Hidden;
                        AboutPanel.Visibility = Visibility.Visible;
                    }
                    
                }

            }
            else
            {
                usbButton.Background = Brushes.Red;
                connection.Content = "Disconnected";
                space.Content = "";
                MainPanel.Visibility = Visibility.Hidden;
                SettingsPanel.Visibility = Visibility.Hidden;
                AboutPanel.Visibility = Visibility.Hidden;
            }

        }

        private void FormatClickFreeUSBBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
