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
        private EraseDialogVM mEraseDialogVM;
        private BackupDialogVM mTransferDialogVM;
        private bool IsEraseDevice = false;
        #endregion

        #region Properties

        public bool SuccessfullyBackuped
        {

            get
            {
                if (!IsEraseDevice)
                    return (mTransferDialogVM?.SuccessfullyBackuped).GetValueOrDefault(false);
                else
                    return (mEraseDialogVM?.SuccessfullyBackuped).GetValueOrDefault(false);

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


        public BackupToClickFreeWindow(string toDir, List<string> from)
        {
            InitializeComponent();
            IsEraseDevice = true;
            this.DataContext = mEraseDialogVM = new EraseDialogVM(toDir, from);
            mEraseDialogVM.Finished += mEraseDialogVM_Finished;
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


        private void mEraseDialogVM_Finished()
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
            if (mEraseDialogVM != null)
            {
                await mEraseDialogVM.StartErase();
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
            if (mEraseDialogVM != null)
            {
                mEraseDialogVM.Finished -= mEraseDialogVM_Finished;
                mEraseDialogVM.Dispose();
            }
        }

        #endregion
    }
}
