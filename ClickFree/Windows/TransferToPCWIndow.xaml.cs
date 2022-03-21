using ClickFree.ViewModel;
using System;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for TransferToPCWIndow.xaml
    /// </summary>
    public partial class TransferToPCWindow : BaseWindow
    {
        #region Fields

        private TransferDialogVM mTransferDialogVM;

        #endregion

        #region Constructor

        public TransferToPCWindow()
        {
            InitializeComponent();
        }

        public TransferToPCWindow(string toDir)
            : this()
        {
            this.DataContext = mTransferDialogVM = new TransferDialogVM(toDir);
            mTransferDialogVM.Finished += MTransferDialogVM_Finished;
            mTransferDialogVM.AppleFormatDetected += MTransferDialogVM_AppleFormatDetected;
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

        private bool? MTransferDialogVM_AppleFormatDetected()
        {
            return Dispatcher.Invoke<bool?>(() =>
            {
                AppleFormatDetectedWindow window = new AppleFormatDetectedWindow()
                {
                    Owner = this
                };

                window.ShowDialog();

                return window.Result/*true - convert, false - leave as it is, null - cancel operation*/;
            });
        }

        #endregion

        #region Overrides

        protected async override void OnLoaded()
        {
            base.OnLoaded();

            if (mTransferDialogVM != null)
            {
                await mTransferDialogVM.StartTransfer(); 
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (mTransferDialogVM != null)
            {
                mTransferDialogVM.Finished -= MTransferDialogVM_Finished;
                mTransferDialogVM.AppleFormatDetected -= MTransferDialogVM_AppleFormatDetected;
                mTransferDialogVM.Dispose();
                mTransferDialogVM = null; 
            }
        }

        #endregion
    }
}
