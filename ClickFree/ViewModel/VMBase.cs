using ClickFree.Helpers;
using GalaSoft.MvvmLight;
using System;

namespace ClickFree.ViewModel
{
    public class VMBase : ViewModelBase, IDisposable
    {
        #region Fields

        private bool mbisDisposed = false;

        #endregion

        #region Contructor
        public VMBase()
        {
            DriveManager.DriveStateChanged += DriveManager_DriveStateChanged;
        }
        #endregion

        #region overrides

        public override void Cleanup()
        {
            base.Cleanup();

            Dispose();
        }

        #endregion

        #region Event handlers

        private void DriveManager_DriveStateChanged(DriveState state, UsbDisk disk)
        {
            RaisePropertyChanged("HasUsbDrives");

            OnDriveStateChanged(state, disk);
        }

        #endregion

        #region Protected virtual

        protected virtual void OnDisposeInternal() { }
        protected virtual void OnDriveStateChanged(DriveState state, UsbDisk disk) { }

        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (!mbisDisposed)
            {
                OnDisposeInternal();

                mbisDisposed = true;
            }
        }

        #endregion
    }
}
