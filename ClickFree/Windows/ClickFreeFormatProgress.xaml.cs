using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
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
        string selectedDriveFormat;
        public int TotalCount = 0;
        public int CurrentCount = 0;

        public ClickFreeFormatProgress()
        {
            InitializeComponent();
        }

        public ClickFreeFormatProgress(string selecteddrive, int totalCount)
        {
            InitializeComponent();
            selectedDriveFormat = selecteddrive;
            TotalCount = totalCount;
            progress.Minimum = 0;
            progress.Maximum = TotalCount;
            progress.Value = CurrentCount;
            var isFormatted = FormatUSB(selectedDriveFormat);
            if (isFormatted)
            {
                string messageBoxText = "Format Complete.";
                string caption = "Formatting USB Drive";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.None;
                MessageBoxOptions options = MessageBoxOptions.DefaultDesktopOnly;
                var result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK, options);
            }
            else
            {
                string messageBoxText = "Format not Completed.";
                string caption = "Formatting USB Drive failed";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBoxOptions options = MessageBoxOptions.DefaultDesktopOnly;
                var result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK, options);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        public bool FormatUSB(string driveLetter, string fileSystem = "FAT32", bool quickFormat = true, int clusterSize = 4096,
            string label = "USB_0000", bool enableCompression = false)
        {
            try
            {
                //add logic to format Usb drive
                //verify conditions for the letter format: driveLetter[0] must be letter. driveLetter[1] must be ":" and all the characters mustn't be more than 2
                if (driveLetter.Length != 2 || driveLetter[1] != ':' || !char.IsLetter(driveLetter[0]))
                    return false;

                //query and format given drive 
                //best option is to use ManagementObjectSearcher

                DirectoryInfo di = new DirectoryInfo(driveLetter);
                
                foreach (FileInfo file in di.GetFiles())
                {
                    try
                    {
                        file.Delete();
                        TotalCount--;
                        progress.Value = TotalCount;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                        TotalCount--;
                        progress.Value = TotalCount;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"select * from Win32_Volume WHERE DriveLetter = '" + driveLetter + "'");
                foreach (ManagementObject vi in searcher.Get())
                {
                    try
                    {
                        var completed = false;
                        var watcher = new ManagementOperationObserver();

                        watcher.Completed += (sender, args) =>
                        {
                            Console.WriteLine("USB format completed " + args.Status);
                            completed = true;
                        };
                        watcher.Progress += (sender, args) =>
                        {
                            Console.WriteLine("USB format in progress " + args.Current);
                        };

                        vi.InvokeMethod(watcher, "Format", new object[] { fileSystem, quickFormat, clusterSize, label, enableCompression });

                        while (!completed) { System.Threading.Thread.Sleep(1000); }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
