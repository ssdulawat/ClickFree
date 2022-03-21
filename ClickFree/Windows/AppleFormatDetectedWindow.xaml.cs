using System;
using System.ComponentModel;
using System.Windows;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for AppleFormatDetectedWindow.xaml
    /// </summary>
    public partial class AppleFormatDetectedWindow : BaseWindow
    {
        #region Ctor

        public AppleFormatDetectedWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public bool? Result { get; private set; }

        #endregion

        #region Event handlers

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;

            this.Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = null;

            this.Close();
        }

        #endregion
    }
}
