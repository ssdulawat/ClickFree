using ClickFree.Properties;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    class InstagramLoginDialogVM : VMBase
    {
        #region Fields

        #region Commands

        private ICommand mContinueCommand;
        private ICommand mLogoutCommand;

        #endregion

        private bool mbBrowserIsVisible = true;

        #endregion

        #region Events

        public event Action Continue;
        public event Action Logout;

        #endregion

        #region Properties

        public ICommand ContinueCommand
        {
            get
            {
                if (mContinueCommand == null)
                {
                    mContinueCommand = new RelayCommand(() =>
                    {
                        Continue?.Invoke();
                    });
                }

                return mContinueCommand;
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                if (mLogoutCommand == null)
                {
                    mLogoutCommand = new RelayCommand(() =>
                    {
                        Logout?.Invoke();
                    });
                }

                return mLogoutCommand;
            }
        }

        public bool BrowserIsVisible
        {
            get
            {
                return mbBrowserIsVisible;
            }
            set
            {

                Set(ref mbBrowserIsVisible, value);
            }
        }

        public string UserName
        {
            get
            {
                return Settings.Default.FacebookUserName;
            }
        }

        #endregion

        #region ctor
        public InstagramLoginDialogVM()
        {

        }
        #endregion
    }
}
