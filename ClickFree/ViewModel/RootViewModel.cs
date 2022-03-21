using ClickFree.Helpers;
using ClickFree.Properties;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class RootViewModel : VMBase, INavigation
    {
        #region Fields

        #region Commands
        private ICommand mBackCommand = null;
        private ICommand mFAQCommand = null;
        private ICommand mContactUSCommand = null;
        private ICommand mGetHelpCommand = null;
        private ICommand mEULACommand = null;
        private ICommand mEraseCommand = null;
        private ICommand mChatSupportCommand = null;
        #endregion

        private NavigationVM mCurrentView = null;
        private Dictionary<NavigateEnum, NavigationVM> mViews = new Dictionary<NavigateEnum, NavigationVM>();

        #endregion

        #region Properties

        #region Commands

        public ICommand BackCommand
        {
            get
            {
                if (mBackCommand == null)
                {
                    mBackCommand = new RelayCommand(() =>
                    {
                        NavigateTo(NavigateEnum.Main);
                    });
                }

                return mBackCommand;
            }
        }

        public ICommand ChatSupportCommand
        {
            get
            {
                if (mChatSupportCommand == null)
                {
                    mChatSupportCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            Process.Start("https://easycustomersupport.com/");
                        }
                        catch { }
                    });
                }

                return mChatSupportCommand;
            }
        }

        public ICommand FAQCommand
        {
            get
            {
                if (mFAQCommand == null)
                {
                    mFAQCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            Process.Start("https://www.datalogixxmemory.com/faq");
                        }
                        catch { }
                    });
                }

                return mFAQCommand;
            }
        }

        public ICommand ContactUSCommand
        {
            get
            {
                if (mContactUSCommand == null)
                {
                    mContactUSCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            Process.Start("mailto:wecare@clickfreebackup.com?subject=Click Free (Windows)");
                        }
                        catch { }
                    });
                }

                return mContactUSCommand;
            }
        }

        public ICommand GetHelpCommand
        {
            get
            {
                if (mGetHelpCommand == null)
                {
                    mGetHelpCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            Process.Start("https://bit.ly/3nDc4YK");
                        }
                        catch { }
                    });
                }

                return mGetHelpCommand;
            }
        }

        public ICommand EULACommand
        {
            get
            {
                if (mEULACommand == null)
                {
                    mEULACommand = new RelayCommand(() =>
                    {
                        try
                        {
                            Process.Start("https://clickfreebackup.com/pages/end-user-license-agreement");
                        }
                        catch { }
                    });
                }

                return mEULACommand;
            }
        }

        public ICommand EraseCommand
        {
            get
            {
                if (mEraseCommand == null)
                {
                    mEraseCommand = new RelayCommand(() =>
                    {
                    });
                }

                return mEraseCommand;
            }
        }

        #endregion

        public NavigationVM CurrentView
        {
            get
            {
                return mCurrentView;
            }
            set
            {
                Set(ref mCurrentView, value);
            }
        }

        #endregion

        #region Ctor
        public RootViewModel()
        {
            mViews.Add(NavigateEnum.OnBoarding, new OnboardingVM(this));
            mViews.Add(NavigateEnum.Main, new MainVM(this));
            mViews.Add(NavigateEnum.TransferToPC, new TransferVM(this));
            mViews.Add(NavigateEnum.BackupToUSBMain, new BackupToUSBMainVM(this));
            mViews.Add(NavigateEnum.BackupToUSBSelect, new BackupToUSBSelectVM(this));
            mViews.Add(NavigateEnum.BackupFacebookMain, new BackupFacebookMainVM(this));
            mViews.Add(NavigateEnum.BackupFacebookDest, new BackupFacebookDestVM(this));
            mViews.Add(NavigateEnum.BackupFacebookSelectImages, new BackupFacebookSelectImagesVM(this));

            if (Settings.Default.OnBoardingPassed == false)
                NavigateTo(NavigateEnum.OnBoarding);
            else
                NavigateTo(NavigateEnum.Main);
        }

        #endregion

        #region Implementation

        public void NavigateTo(NavigateEnum navigateTo)
        {
            NavigateTo(navigateTo, null);
        }

        public void NavigateTo(NavigateEnum navigateTo, object parameter)
        {
            var prevView = CurrentView;

            switch (navigateTo)
            {
                case NavigateEnum.TransferToPC:
                case NavigateEnum.BackupToUSBSelect:
                case NavigateEnum.BackupFacebookMain:
                case NavigateEnum.BackupFacebookDest:
                    if (DriveManager.CheckAccess())
                        CurrentView = mViews[navigateTo];
                    break;

                case NavigateEnum.BackupToUSBMain:
                case NavigateEnum.Main:
                case NavigateEnum.OnBoarding:
                default:
                    CurrentView = mViews[navigateTo];
                    break;
            }

            if (prevView != CurrentView)
            {
                prevView?.Deactivated();

                CurrentView?.Activated(parameter);
            }
        }

        #endregion
    }
}