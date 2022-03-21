using ClickFree.Helpers;
using ClickFree.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using forms = System.Windows.Forms;

namespace ClickFree.ViewModel
{
    public class TransferVM : NavigationVM
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
                        if (DriveManager.CheckAccess())
                        {
                            var ownerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
                            TransferToPCWindow window = new TransferToPCWindow(CurrentDir)
                            {
                                Owner = ownerWindow
                            };
                            window.ShowDialog();
                        }
                    }, () =>
                    {
                        return !string.IsNullOrWhiteSpace(CurrentDir);
                    });
                }

                return mTransferCommand;
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

        public TransferVM(INavigation navigation)
            : base(navigation)
        {
            CurrentDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Constants.ClickFreeFolderName);
        }

        #endregion
    }
}
