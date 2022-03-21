using ClickFree.Helpers;
using ClickFree.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class BackupToUSBMainVM : NavigationVM
    {
        #region Fields

        public ICommand mTransferDefaultFoldersCommand;
        public ICommand mTransferSelectedFilesCommand;

        #endregion

        #region Properties

        #region Commands

        public ICommand TransferDefaultCommand
        {
            get
            {
                if (mTransferDefaultFoldersCommand == null)
                {
                    mTransferDefaultFoldersCommand = new RelayCommand(() =>
                    {
                        if (DriveManager.CheckAccess())
                        {
                            string toFolder = Path.Combine(DriveManager.SelectedUSBDrive.Name, Constants.WindowsBackupFolderName);

                            var ownerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
                            BackupToClickFreeWindow window = new BackupToClickFreeWindow(Constants.DefaultBackUpFolders, toFolder)
                            {
                                Owner = ownerWindow
                            };
                            window.ShowDialog();
                        }
                    });
                }

                return mTransferDefaultFoldersCommand;
            }
        }

        public ICommand TransferSelectedFilesCommand
        {
            get
            {
                if (mTransferSelectedFilesCommand == null)
                {
                    mTransferSelectedFilesCommand = new RelayCommand(() =>
                    {
                        if (DriveManager.CheckAccess())
                        {
                            NavigateTo(NavigateEnum.BackupToUSBSelect);
                        }
                    });
                }

                return mTransferSelectedFilesCommand;
            }
        } 
        #endregion

        #endregion

        #region Ctor
        public BackupToUSBMainVM(INavigation navigation) : base(navigation)
        {
        }
        #endregion
    }
}
