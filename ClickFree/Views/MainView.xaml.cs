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
        List<UsbDisk> disks = new List<UsbDisk>();
        string USBName;
        string formatResult;

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

        //private void ShowQuestion()
        //{
        //    string messageBoxText = "Are you sure format USB drive - " + USBName;
        //    string caption = "Format USB";
        //    MessageBoxButton button = MessageBoxButton.YesNo;
        //    MessageBoxImage icon = MessageBoxImage.Warning;
        //    MessageBoxOptions options = MessageBoxOptions.DefaultDesktopOnly;
        //    var result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.No, options);

        //    formatResult = Convert.ToString(result);
        //}

        //private void FormatClickFreeUSBBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    disks = DriveManager.GetAvailableDisks();
        //    var disk = disks.FirstOrDefault();
        //    USBName = disk.Name.Split('\\')[0];
        //    ShowQuestion();

        //    if (formatResult == "Yes")
        //    {
        //        var isFormatted = FormatUSB(USBName);
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}

        //public bool FormatUSB(string driveLetter, string fileSystem = "FAT32", bool quickFormat = true, int clusterSize = 4096,
        //    string label = "USB_0000", bool enableCompression = false)
        //{
        //    try
        //    {
        //        //add logic to format Usb drive
        //        //verify conditions for the letter format: driveLetter[0] must be letter. driveLetter[1] must be ":" and all the characters mustn't be more than 2
        //        if (driveLetter.Length != 2 || driveLetter[1] != ':' || !char.IsLetter(driveLetter[0]))
        //            return false;

        //        //query and format given drive 
        //        //best option is to use ManagementObjectSearcher

        //        DirectoryInfo di = new DirectoryInfo(driveLetter);

        //        foreach (FileInfo file in di.GetFiles())
        //        {
        //            try
        //            {
        //                file.Delete();
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        foreach (DirectoryInfo dir in di.GetDirectories())
        //        {
        //            try
        //            {
        //                dir.Delete(true);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        //        ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"select * from Win32_Volume WHERE DriveLetter = '" + driveLetter + "'");
        //        foreach (ManagementObject vi in searcher.Get())
        //        {
        //            try
        //            {
        //                var completed = false;
        //                var watcher = new ManagementOperationObserver();

        //                watcher.Completed += (sender, args) =>
        //                {
        //                    Console.WriteLine("USB format completed " + args.Status);
        //                    completed = true;
        //                };
        //                watcher.Progress += (sender, args) =>
        //                {
        //                    Console.WriteLine("USB format in progress " + args.Current);
        //                };

        //                vi.InvokeMethod(watcher, "Format", new object[] { fileSystem, quickFormat, clusterSize, label, enableCompression });

        //                while (!completed) { System.Threading.Thread.Sleep(1000); }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}
