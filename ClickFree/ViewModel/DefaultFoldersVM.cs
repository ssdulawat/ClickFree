using ClickFree.Helpers;
using ClickFree.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using forms = System.Windows.Forms;
using ClickFree.Properties;
using ClickFree.ViewModel;
using System.Configuration;

namespace ClickFree.ViewModel
{
    public class DefaultFoldersVM : NavigationVM
    {
        #region Fields

        #region Commands

        private ICommand mTransferCommand = null;
        private ICommand mSelectFolderCommand = null;

        #endregion

        private string mCurrentDir;

        #endregion

        #region Properties

        #region Commands

        public ICommand TransferCommand
        {
            get
            {
                if (mTransferCommand == null)
                {
                    mTransferCommand = new RelayCommand(() =>
                    {
                        AddUpdateAppSettings("DefaultFolders", CurrentDir);
                        NavigateTo(NavigateEnum.Main);
                    }, () =>
                    {
                        return !string.IsNullOrWhiteSpace(CurrentDir);
                    });
                }

                return mTransferCommand;
            }
        }

        static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        public ICommand SelectFolderCommand
        {
            get
            {
                if (mSelectFolderCommand == null)
                {
                    mSelectFolderCommand = new RelayCommand(() =>
                    {
                        using (var folder = new forms.FolderBrowserDialog())
                        {
                            if (folder.ShowDialog() == forms.DialogResult.OK)
                            {
                                CurrentDir = folder.SelectedPath;
                            }
                        }
                    });
                }

                return mSelectFolderCommand;
            }
        }

        #endregion

        public string CurrentDir
        {
            get { return mCurrentDir; }
            set
            {
                Set(ref mCurrentDir, value);
            }
        }

        #endregion

        #region Ctor

        public DefaultFoldersVM(INavigation navigation)
            : base(navigation)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if(settings["DefaultFolders"]!=null)
            CurrentDir = settings["DefaultFolders"].Value;
        }

        #endregion
    }
}
