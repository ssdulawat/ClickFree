using ClickFree.Helpers;
using ClickFree.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

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
                            string toFolder = Path.Combine(DriveManager.SelectedUSBDrive.Name, Constants.WindowsBackupFolderName, Environment.MachineName);
                            string defaultdirectory = string.Empty;
                            var ownerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
                            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                            var settings = configFile.AppSettings.Settings;
                            List<string> objList = new List<string>();
                            if (settings["DefaultFolders"] != null)
                                objList.Add(settings["DefaultFolders"].Value.ToString());
                            else
                                objList = Constants.DefaultBackUpFolders;

                            //to foldet is not exists then we need to make this directory
                            if (!Directory.Exists(toFolder))
                            {
                                Directory.CreateDirectory(toFolder);
                            }
                            BackupToClickFreeWindow window = new BackupToClickFreeWindow(objList, toFolder)
                            {
                                Owner = ownerWindow
                            };
                            //BackupToClickFreeWindow window = new BackupToClickFreeWindow(Constants.DefaultBackUpFolders, toFolder)
                            //{
                            //    Owner = ownerWindow
                            //};
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
