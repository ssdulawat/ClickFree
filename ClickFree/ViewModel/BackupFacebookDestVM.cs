using ClickFree.Facebook;
using ClickFree.Helpers;
using ClickFree.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class BackupFacebookDestVM : NavigationVM
    {
        #region Fields

        private ICommand mTransferToUSBCommand;
        private ICommand mTransferClickFreePCFolderCommand;
        private ICommand mTransferToSelectedFolderCommand;

        private FacebookManager.MediaResult[] mSelectedImages = null;

        #endregion

        #region Properties

        public ICommand TransferToUSBCommand
        {
            get
            {
                if (mTransferToUSBCommand == null)
                {
                    mTransferToUSBCommand = new RelayCommand(() =>
                    {
                        if (FacebookManager.CheckNetworkConnection() && DriveManager.CheckAccess())
                        {
                            string toFolder = Path.Combine(DriveManager.SelectedUSBDrive.Name, Constants.WindowsBackupFolderName, Constants.FacebookFolderName);

                            var ownerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
                            BackupFromFacebookWindow window = new BackupFromFacebookWindow(mSelectedImages?.ToList(), toFolder)
                            {
                                Owner = ownerWindow
                            };
                            window.ShowDialog(); 
                        }
                    });
                }

                return mTransferToUSBCommand;
            }
        }

        public ICommand TransferClickFreePCFolderCommand
        {
            get
            {
                if (mTransferClickFreePCFolderCommand == null)
                {
                    mTransferClickFreePCFolderCommand = new RelayCommand(() =>
                    {
                        if (FacebookManager.CheckNetworkConnection())
                        {
                            string toFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Constants.ClickFreeFolderName, Constants.FacebookFolderName);

                            var ownerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
                            BackupFromFacebookWindow window = new BackupFromFacebookWindow(mSelectedImages?.ToList(), toFolder)
                            {
                                Owner = ownerWindow
                            };
                            window.ShowDialog(); 
                        }
                    });
                }

                return mTransferClickFreePCFolderCommand;
            }
        }

        public ICommand TransferToSelectedFolderCommand
        {
            get
            {
                if (mTransferToSelectedFolderCommand == null)
                {
                    mTransferToSelectedFolderCommand = new RelayCommand(() =>
                    {
                        if (FacebookManager.CheckNetworkConnection())
                        {
                            using (var dbd = new System.Windows.Forms.FolderBrowserDialog())
                            {
                                if (dbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    var ownerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
                                    BackupFromFacebookWindow window = new BackupFromFacebookWindow(mSelectedImages?.ToList(), dbd.SelectedPath)
                                    {
                                        Owner = ownerWindow
                                    };
                                    window.ShowDialog();
                                }
                            } 
                        }
                    });
                }

                return mTransferToSelectedFolderCommand;
            }
        }

        #endregion

        #region Ctor

        public BackupFacebookDestVM(INavigation navigation) : base(navigation)
        {
        }

        #endregion

        #region Overrides

        protected internal override void Activated(object parameter)
        {
            if (parameter is FacebookManager.MediaResult[] selectedImages && selectedImages.Length > 0)
            {
                mSelectedImages = selectedImages;
            }
        }

        protected internal override void Deactivated()
        {
            mSelectedImages = null;
        }

        #endregion
    }
}
