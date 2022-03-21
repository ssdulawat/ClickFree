using ClickFree.Helpers;
using ClickFree.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class TransferDialogVM : VMBase
    {
        #region Fields

        #region Commands

        private ICommand mCancelCommand = null;

        #endregion

        private TransferManager mTransferManager = null;
        private string mToDir;
        private long mCurrentSize;
        private long mTotalSize;
        private string mStatus;

        #endregion

        #region Properties

        #region Commands

        public ICommand CancelCommand
        {
            get
            {
                if (mCancelCommand == null)
                {
                    mCancelCommand = new RelayCommand(async() =>
                    {
                        if (mTransferManager == null)
                            Finished?.Invoke();
                        else
                        {
                            Status = "Cancelling";
                            await mTransferManager.CancelAsync();
                        }
                    },()=>
                    {
                        return mTransferManager != null && mTransferManager.IsInProgress;
                    });
                }

                return mCancelCommand;
            }
        }

        #endregion

        public long CurrentSize
        {
            get
            {
                return mCurrentSize;
            }
            set
            {
                Set(ref mCurrentSize, value);
            }
        }

        public long TotalSize
        {
            get
            {
                return mTotalSize;
            }
            set
            {
                Set(ref mTotalSize, value);
            }
        }

        public string Status
        {
            get
            {
                return mStatus;
            }
            set
            {
                Set(ref mStatus, value);
            }
        }

        #endregion

        #region Events

        public event Action Finished;
        public event Func<bool?> AppleFormatDetected;

        #endregion


        #region Ctor

        public TransferDialogVM(string toDir)
        {
            mToDir = toDir;
        }

        #endregion

        #region Methods

        public async Task<bool> StartTransfer()
        {
            bool result = DriveManager.CheckAccess();

            if (result)
            {
                if (mTransferManager != null)
                {
                    await mTransferManager.CancelAsync();
                    mTransferManager.Start -= MTransferManager_Start;
                    mTransferManager.Progress -= MTransferManager_Progress;
                    mTransferManager.Finished -= MTransferManager_Finished;
                    mTransferManager.AppleFormatDetected -= MTransferManager_AppleFormatDetected;
                    mTransferManager.SearchStart -= MTransferManager_SearchStart;
                    mTransferManager.SearchFinished -= MTransferManager_SearchFinished;
                }

                mTransferManager = new TransferManager();
                mTransferManager.Start += MTransferManager_Start;
                mTransferManager.Progress += MTransferManager_Progress;
                mTransferManager.Finished += MTransferManager_Finished;
                mTransferManager.AppleFormatDetected += MTransferManager_AppleFormatDetected;
                mTransferManager.SearchStart += MTransferManager_SearchStart;
                mTransferManager.SearchFinished += MTransferManager_SearchFinished;

                await mTransferManager.ScanAndTransfer(DriveManager.SelectedUSBDrive.Name, mToDir);
            }

            return result;
        }

        public async Task CancelTransfer()
        {
            if(mTransferManager != null)
            {
                await mTransferManager.CancelAsync();
            }
        }

        #endregion

        #region Event handlers

        private void MTransferManager_SearchFinished(FileManager.SearchResult obj)
        {
            Status = $"Scanning is finished";
        }

        private void MTransferManager_SearchStart()
        {
            Status = $"Scanning '{DriveManager.SelectedUSBDrive?.Name}'...";
        }

        private bool? MTransferManager_AppleFormatDetected(FileManager.SearchResult arg)
        {
            if (AppleFormatDetected == null)
                return false;
            else return AppleFormatDetected.Invoke();
        }

        private void MTransferManager_Finished(TransferManager.TransferFinishedInfo obj)
        {
            CurrentSize = obj.CurrentSize;
            TotalSize = obj.TotalSize;

            switch (obj.FailedReson)
            {
                case TransferManager.FailedReason.AccessDenied:
                    MessageBoxWindow.ShowMessageBox("Transfer your photos and videos to PC",
                                                    "Could not transfer files. You dont have enought permissions for destination folder. Please restart the app as administrator and try again.",
                                                    MessageBoxWindow.MessageBoxType.Error);
                    break;
                case TransferManager.FailedReason.SearchFailed:
                case TransferManager.FailedReason.Other:
                case TransferManager.FailedReason.UsbNotFound:
                case TransferManager.FailedReason.FolderNotFound:
                    MessageBoxWindow.ShowMessageBox("Could not establish connection with ClickFree.", "Please connect/ re - connect ClickFree to your computer USB port.",
                        MessageBoxWindow.MessageBoxType.Error);
                    break;
                case TransferManager.FailedReason.Cancelled:
                    Status = "Transfer is cancelled";
                    break;
                case TransferManager.FailedReason.None:
                default:
                    MessageBoxWindow.ShowMessageBox("Transfer your photos and videos to PC",
                                                    $"{obj.CurrentPosition} files were successfully transferred to your PC. Now you can view them in '{Path.GetFileName(mToDir)}' folder.",
                                                    MessageBoxWindow.MessageBoxType.Success,
                                                    () =>
                                                    {
                                                        Process.Start(mToDir);
                                                    });
                    break;
            }

            Finished?.Invoke();
        }

        private void MTransferManager_Progress(TransferManager.TransferProgressInfo obj)
        {
            CurrentSize = obj.CurrentSize;
            TotalSize = obj.TotalSize;

            Status = $"{obj.CurrentPosition} files out of {obj.TotalFiles} were transferred. Please wait.";
        }

        private void MTransferManager_Start(TransferManager.TransferStartInfo obj)
        {
            CurrentSize = obj.CurrentSize;
            TotalSize = obj.TotalSize;

            Status = $"{obj.CurrentPosition} files out of {obj.TotalFiles} were transferred. Please wait.";
        }

        #endregion

        #region Overrides

        protected async override void OnDisposeInternal()
        {
            base.OnDisposeInternal();

            if (mTransferManager != null)
            {
                await mTransferManager.CancelAsync();
                mTransferManager.Start -= MTransferManager_Start;
                mTransferManager.Progress -= MTransferManager_Progress;
                mTransferManager.Finished -= MTransferManager_Finished;
                mTransferManager.AppleFormatDetected -= MTransferManager_AppleFormatDetected;
                mTransferManager.SearchStart -= MTransferManager_SearchStart;
                mTransferManager.SearchFinished -= MTransferManager_SearchFinished;
                mTransferManager = null;
            }
        }

        #endregion
    }
}
