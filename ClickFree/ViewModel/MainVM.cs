using ClickFree.Facebook;
using ClickFree.Helpers;
using GalaSoft.MvvmLight.CommandWpf;
using System.Diagnostics;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class MainVM : NavigationVM
    {
        #region Fields

        #region Commands

        private ICommand mTransferToPCCommand = null;
        private ICommand mBackupToUSBCommand = null;
        private ICommand mBackupFromFacebookCommand = null;
        private ICommand mViewClickFreeCommand = null;

        #endregion

        #endregion

        #region Properties

        #region Commands

        public ICommand BackupFromFacebookCommand
        {
            get
            {
                if (mBackupFromFacebookCommand == null)
                {
                    mBackupFromFacebookCommand = new RelayCommand(() =>
                    {
                        if (FacebookManager.CheckAuthorization())
                        {
                            NavigateTo(NavigateEnum.BackupFacebookMain);
                        }
                    });
                }

                return mBackupFromFacebookCommand;
            }
        }

        public ICommand TransferToPCCommand
        {
            get
            {
                if (mTransferToPCCommand == null)
                {
                    mTransferToPCCommand = new RelayCommand(() =>
                    {
                        NavigateTo(NavigateEnum.TransferToPC);
                    },
                    () =>
                    {
                        return true;
                    });
                }

                return mTransferToPCCommand;
            }
        }

        public ICommand BackupToUSBCommand
        {
            get
            {
                if (mBackupToUSBCommand == null)
                {
                    mBackupToUSBCommand = new RelayCommand(() =>
                    {
                        NavigateTo(NavigateEnum.BackupToUSBMain);
                    });
                }

                return mBackupToUSBCommand;
            }
        }

        public ICommand ViewClickFreeCommand
        {
            get
            {
                if (mViewClickFreeCommand == null)
                {
                    mViewClickFreeCommand = new RelayCommand(() =>
                    {
                        if(DriveManager.CheckAccess())
                        {
                            try
                            {
                                Process.Start(DriveManager.SelectedUSBDrive.Name);
                            }
                            catch{}
                        }
                    }/*,
                    ()=>
                    {
                        return DriveManager.SelectedUSBDrive != null;
                    }*/);
                }

                return mViewClickFreeCommand;
            }
        }

        #endregion

        #endregion

        #region Ctor

        public MainVM(INavigation navigation)
            :base(navigation)
        {
        }

        #endregion
    }
}
