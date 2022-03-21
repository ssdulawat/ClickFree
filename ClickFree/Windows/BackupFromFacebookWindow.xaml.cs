using ClickFree.ViewModel;
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
using static ClickFree.Facebook.FacebookManager;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for BackupFromFacebookWindow.xaml
    /// </summary>
    public partial class BackupFromFacebookWindow : BaseWindow
    {
        #region Fields

        private BackupFromFacebookDialogVM mBackupFromFacebookDialogVM;

        #endregion

        #region Ctor

        public BackupFromFacebookWindow()
        {
            InitializeComponent();
        }


        public BackupFromFacebookWindow(List<MediaResult> from, string toDir)
        {
            InitializeComponent();

            this.DataContext = this.mBackupFromFacebookDialogVM = new BackupFromFacebookDialogVM(from, toDir);
            this.mBackupFromFacebookDialogVM.Finished += this.MTransferDialogVM_Finished;
        }
        #endregion

        #region Event handlers

        private void MTransferDialogVM_Finished()
        {
            if (CheckAccess())
                this.Close();
            else
                Dispatcher.Invoke(this.Close);
        }

        #endregion

        #region Overrides

        protected async override void OnLoaded()
        {
            base.OnLoaded();

            await this.mBackupFromFacebookDialogVM.StartBackup();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this.mBackupFromFacebookDialogVM != null)
            {
                this.mBackupFromFacebookDialogVM.Finished -= MTransferDialogVM_Finished;
                this.mBackupFromFacebookDialogVM.Dispose();
            }
        }

        #endregion
    }
}
