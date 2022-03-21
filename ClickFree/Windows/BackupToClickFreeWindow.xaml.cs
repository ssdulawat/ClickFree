using ClickFree.ViewModel;
using System;
using System.Collections.Generic;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for BackupToClickFreeWindow.xaml
    /// </summary>
    public partial class BackupToClickFreeWindow : BaseWindow
    {
        #region Fields

        private BackupDialogVM mTransferDialogVM;

        #endregion

        #region Properties

        public bool SuccessfullyBackuped
        {
            get
            {
                return (mTransferDialogVM?.SuccessfullyBackuped).GetValueOrDefault(false);
            }
        }

        #endregion

        #region Constructor

        public BackupToClickFreeWindow()
        {
        }

        public BackupToClickFreeWindow(List<string> from, string toDir)
        {
            InitializeComponent();
            this.DataContext = mTransferDialogVM = new BackupDialogVM(from, toDir);
            mTransferDialogVM.Finished += MTransferDialogVM_Finished;
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

            if (mTransferDialogVM != null)
            {
                await mTransferDialogVM.StartBackup();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (mTransferDialogVM != null)
            {
                mTransferDialogVM.Finished -= MTransferDialogVM_Finished;
                mTransferDialogVM.Dispose();
            }
        }

        #endregion
    }
}
