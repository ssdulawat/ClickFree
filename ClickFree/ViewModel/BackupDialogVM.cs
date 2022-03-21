using ClickFree.Helpers;
using ClickFree.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class BackupDialogVM : VMBase
    {
        #region Fields

        #region Commands

        private ICommand mCancelCommand = null;

        #endregion

        private TransferManager mTransferManager = null;
        private List<string> mFrom;
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
                    mCancelCommand = new RelayCommand(async () =>
                    {
                        if (mTransferManager == null)
                            Finished?.Invoke();
                        else
                        {
                            Status = "Cancelling";
                            await mTransferManager.CancelAsync();
                        }
                    }, () =>
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

        public bool SuccessfullyBackuped { get; private set; }

        #endregion

        #region Events

        public event Action Finished;

        #endregion

        #region Ctor

        public BackupDialogVM(List<string> from, string toDir)
        {
            mFrom = from;
            mToDir = toDir;
        }

        #endregion

        #region Methods

        public async Task<bool> StartBackup()
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
                    mTransferManager.SearchStart -= MTransferManager_SearchStart;
                    mTransferManager.SearchFinished -= MTransferManager_SearchFinished;
                }

                mTransferManager = new TransferManager();
                mTransferManager.Start += MTransferManager_Start;
                mTransferManager.Progress += MTransferManager_Progress;
                mTransferManager.Finished += MTransferManager_Finished;
                mTransferManager.SearchStart += MTransferManager_SearchStart;
                mTransferManager.SearchFinished += MTransferManager_SearchFinished;

                await mTransferManager.ScanAndBackup(mFrom, mToDir);
            }

            return result;
        }

        public async Task CancelTransfer()
        {
            if (mTransferManager != null)
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

        private void MTransferManager_Finished(TransferManager.TransferFinishedInfo obj)
        {
            CurrentSize = obj.CurrentSize;
            TotalSize = obj.TotalSize;

            switch (obj.FailedReson)
            {
                case TransferManager.FailedReason.AccessDenied:
                    MessageBoxWindow.ShowMessageBox("Backup your photos and videos to PC",
                                                    "Could not backup files. You dont have enought permissions for destination folder. Please restart the app as administrator and try again.",
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
                    MessageBoxWindow.ShowMessageBox("Backup your photos and videos to USB",
                                                    $"{obj.CurrentPosition} files were successfully backuped to your USB. Now you can view them in Click Free folder.",
                                                    MessageBoxWindow.MessageBoxType.Success,
                                                    () =>
                                                    {
                                                        Process.Start(mToDir);
                                                    });

                    SuccessfullyBackuped = true;
                    break;
            }

            Finished?.Invoke();
        }

        private void MTransferManager_Progress(TransferManager.TransferProgressInfo obj)
        {
            CurrentSize = obj.CurrentSize;
            TotalSize = obj.TotalSize;

            Status = $"{obj.CurrentPosition} files out of {obj.TotalFiles} were backuped. Please wait.";
        }

        private void MTransferManager_Start(TransferManager.TransferStartInfo obj)
        {
            CurrentSize = obj.CurrentSize;
            TotalSize = obj.TotalSize;

            Status = $"{obj.CurrentPosition} files out of {obj.TotalFiles} were backuped. Please wait.";
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
                mTransferManager.SearchStart -= MTransferManager_SearchStart;
                mTransferManager.SearchFinished -= MTransferManager_SearchFinished;
                mTransferManager = null;
            }
        }

        #endregion
    }
}
