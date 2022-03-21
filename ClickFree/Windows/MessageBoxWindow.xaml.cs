using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : BaseWindow
    {
        #region Nested types

        public enum MessageBoxType
        {
            Success,
            Information,
            Warning,
            Error
        }

        #endregion

        #region Ctor
        private MessageBoxWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        #endregion

        #region Static

        public static bool? ShowMessageBox(string title, string message, MessageBoxType type)
        {
            return ShowMessageBox(title, message, type, null);
        }

        public static bool? ShowMessageBox(string title, string message, MessageBoxType type, Action viewFolder)
        {
            if (App.Current.CheckAccess())
            {
                Window owner = Application.Current.Windows[Application.Current.Windows.Count - 1];

                MessageBoxWindow window = new MessageBoxWindow()
                {
                    Title = title,
                    Owner = owner
                };

                window.txtTitle.Text = title;
                window.tbMessage.Text = message;

                if (viewFolder == null)
                    window.btnViewFolder.Visibility = Visibility.Hidden;
                else
                    window.btnViewFolder.Tag = viewFolder;

                switch (type)
                {
                    case MessageBoxType.Information:
                        window.imgInfo.Source = BitmapFrame.Create(new System.Uri("pack://application:,,,/ClickFree;component/Resources/Info.png", uriKind: System.UriKind.RelativeOrAbsolute));
                        break;
                    case MessageBoxType.Warning:
                        window.imgInfo.Source = BitmapFrame.Create(new System.Uri("pack://application:,,,/ClickFree;component/Resources/Warning.png", uriKind: System.UriKind.RelativeOrAbsolute));
                        break;
                    case MessageBoxType.Error:
                        window.imgInfo.Source = BitmapFrame.Create(new System.Uri("pack://application:,,,/ClickFree;component/Resources/Error.png", uriKind: System.UriKind.RelativeOrAbsolute));
                        break;
                    case MessageBoxType.Success:
                        window.imgInfo.Source = BitmapFrame.Create(new System.Uri("pack://application:,,,/ClickFree;component/Resources/Success.png", uriKind: System.UriKind.RelativeOrAbsolute));
                        break;
                }

                return window.ShowDialog();
            }
            else
            {
                return App.Current.Dispatcher.Invoke(() => ShowMessageBox(title, message, type, viewFolder));
            }
        }

        #endregion

        #region Event handlers
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void BtnViewFolder_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            if (btnViewFolder.Tag is Action action)
                action.BeginInvoke(null, this);

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        #endregion
    }
}
