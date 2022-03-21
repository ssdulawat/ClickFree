using ClickFree.Helpers;
using System.Windows;
using System.Windows.Interop;

namespace ClickFree.Windows
{
    public class BaseWindow: Window
    {
        #region Properties

        public virtual bool HideSysMenu { get { return true; } }

        #endregion

        #region Ctor

        public BaseWindow()
        {
            Loaded += BaseWindow_Loaded;
        }

        #endregion

        #region Event handlers

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (HideSysMenu)
            {
                WinAPI.HideSysMENU(new WindowInteropHelper(this).Handle);
            }

            OnLoaded();
        }

        #endregion

        #region Protected virtual

        protected virtual void OnLoaded()
        {

        }

        #endregion
    }
}
