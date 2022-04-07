﻿using ClickFree.Facebook;
using ClickFree.Helpers;
using ClickFree.ViewModel;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for FacebookLoginWindow.xaml
    /// </summary>
    public partial class FacebookLoginWindow : Window
    {
        #region WINAPI

        [DllImport("wininet.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetOption(int hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private static unsafe void SuppressWininetBehavior()
        {

            int option = (int)3/* INTERNET_SUPPRESS_COOKIE_PERSIST*/;
            int* optionPtr = &option;

            bool success = InternetSetOption(0, 81/*INTERNET_OPTION_SUPPRESS_BEHAVIOR*/, new IntPtr(optionPtr), sizeof(int));
        }

        #endregion

        #region Nested types

        public enum LoginState
        {
            Success,
            Failed,
            LoggedOut
        }

        #endregion

        #region Fields

        private FacebookLoginDialogVM mFacebookLoginDialogVM;
        private bool mbLoggout = false;

        #endregion

        #region Ctor
        public FacebookLoginWindow(bool showLogin = true)
        {
            //disable cookies.
            SuppressWininetBehavior();

            InitializeComponent();

            this.DataContext = mFacebookLoginDialogVM = new FacebookLoginDialogVM()
            {
                BrowserIsVisible = showLogin
            };

            this.mFacebookLoginDialogVM.Continue += MFacebookLoginDialogVM_Continue;
            this.mFacebookLoginDialogVM.Logout += MFacebookLoginDialogVM_Logout;

            if (showLogin)
            {
                IntPtr windowHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
                if (windowHandle != IntPtr.Zero)
                {
                    WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & ~WinAPI.WS_MINIMIZEBOX);
                    WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & ~WinAPI.WS_MAXIMIZEBOX);
                }

                this.Height = 380;
                this.Width = 360;
                this.NavigateToLogin();
                this.AllowsTransparency = false;
                this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                this.BorderThickness = new Thickness(0);
            }
            else
            {
                this.BorderThickness = new Thickness(1);
                this.AllowsTransparency = true;
                this.WindowStyle = WindowStyle.None;
                this.Height = 280;
            }

            WinAPI.HideSysMENU(new WindowInteropHelper(this).Handle);
        }

        #endregion

        #region Methods

        public static LoginState Show(bool login)
        {
            LoginState state = LoginState.Failed;

            if (App.Current.CheckAccess())
            {
                Window owner = Application.Current.Windows[Application.Current.Windows.Count - 1];

                FacebookLoginWindow window = new FacebookLoginWindow(login)
                {
                    Owner = owner
                };

                if (window.ShowDialog().GetValueOrDefault(false))
                {
                    state = LoginState.Success;
                }
                else if (window.mbLoggout)
                {
                    state = LoginState.LoggedOut;
                }

                return state;
            }
            else
            {
                return App.Current.Dispatcher.Invoke(() => Show(login));
            }
        }

        private void NavigateToLogin()
        {
            wbFacebookLogin.Navigate(FacebookManager.OAuthURL, null, null, "User-Agent: ; MSIE xyz;");
        }

        #endregion

        #region Event handlers

        private async void WbFacebookLogin_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsolutePath == "blank")
            {
                wbFacebookLogin.Navigate(FacebookManager.OAuthURL, null, null, "User-Agent: ; MSIE xyz;");
            }
            else
            {
                var code = HttpUtility.ParseQueryString(e.Uri.Query)["code"];
                if (!string.IsNullOrWhiteSpace(code))
                {
                    DialogResult = await FacebookManager.GetAccessCode(code);

                    if (!DialogResult.GetValueOrDefault(false) && FacebookManager.LastRequestException != null)
                    {
                        MessageBoxWindow.ShowMessageBox("Error", FacebookManager.LastRequestException.Message, MessageBoxWindow.MessageBoxType.Error);
                    }

                    this.Close();
                }
            }
            SetSilent(wbFacebookLogin, true); // make it silent
        }

        #region SetSilent script method
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            //get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        #endregion

        private void MFacebookLoginDialogVM_Logout()
        {
            FacebookManager.Logout();

            mbLoggout = true;
            DialogResult = false;
            this.Close();
        }

        private void MFacebookLoginDialogVM_Continue()
        {
            DialogResult = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void WbFacebookLogin_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //hide scrollbar
            wbFacebookLogin.InvokeScript("execScript", new Object[] { "document.documentElement.style.overflow='hidden'", "JavaScript" });
        }

        #endregion
    }
}
