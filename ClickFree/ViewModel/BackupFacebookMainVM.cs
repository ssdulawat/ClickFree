using ClickFree.Facebook;
using ClickFree.Helpers;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class BackupFacebookMainVM : NavigationVM
    {
        #region Fields

        private ICommand mBackupEverythingCommand;
        private ICommand mBackupPhotoAndVideoCommand;

        #endregion

        #region Properties

        public ICommand BackupEverythingCommand
        {
            get
            {
                if (mBackupEverythingCommand == null)
                {
                    mBackupEverythingCommand = new RelayCommand(() =>
                    {
                        if (FacebookManager.CheckAuthorization(false))
                            NavigateTo(NavigateEnum.BackupFacebookDest, 1);
                    });
                }

                return mBackupEverythingCommand;
            }
        }

        public ICommand BackupPhotoAndVideoCommand
        {
            get
            {
                if (mBackupPhotoAndVideoCommand == null)
                {
                    mBackupPhotoAndVideoCommand = new RelayCommand(() =>
                    {
                        if (FacebookManager.CheckAuthorization(false))
                            NavigateTo(NavigateEnum.BackupFacebookSelectImages, 1);
                    });
                }

                return mBackupPhotoAndVideoCommand;
            }
        }

        #endregion

        #region Ctor

        public BackupFacebookMainVM(INavigation navigation) : base(navigation)
        {
        } 

        #endregion
    }
}
