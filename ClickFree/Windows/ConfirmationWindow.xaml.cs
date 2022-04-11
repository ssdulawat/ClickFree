using System;
using System.Collections.Generic;
using System.IO;
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
        public static string selectedDriveFormat { get; set; }
        public static FormatClickFreeWindow _formatClickFreeWindow { get; set; }
        int fileCount;
        int directoriesCount;
        int TotalCount;

        public ConfirmationWindow()
        {
            InitializeComponent();
        }

        public ConfirmationWindow(string selectedDrive, FormatClickFreeWindow formatClickFreeWindow)
        {
            InitializeComponent();
            selectedDriveFormat = selectedDrive;
            _formatClickFreeWindow = formatClickFreeWindow;
        }

        public void YesButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.yesBtn.IsHitTestVisible = false;
                });
                
                ClickFreeFormatProgress win = new ClickFreeFormatProgress();
                win.Show();

                DirectoryInfo di = new DirectoryInfo(selectedDriveFormat);
                fileCount = di.GetFiles().Count();
                directoriesCount = di.GetDirectories().Count();
                TotalCount = fileCount + directoriesCount;

                ClickFreeFormatProgress clickFreeFormat = new ClickFreeFormatProgress(selectedDriveFormat, TotalCount);

                win.Close();
                Close();
                this.Dispatcher.Invoke(() =>
                {
                    this.yesBtn.IsHitTestVisible = true;
                    _formatClickFreeWindow.FormatBtn.IsHitTestVisible = true;
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            _formatClickFreeWindow.FormatBtn.IsHitTestVisible = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
