using ClickFree.Helpers;
using ClickFree.Instagram;
using ClickFree.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClickFree.Windows
{
    /// <summary>
    /// Interaction logic for InstagramLoginWindow.xaml
    /// </summary>
    public partial class InstagramLoginWindow : Window
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

        private InstagramLoginDialogVM mInstagramLoginDialogVM;
        private bool mbLoggout = false;

        #endregion
        public InstagramLoginWindow(bool showLogin = true)
        {
            //disable cookies.
            SuppressWininetBehavior();

            InitializeComponent();

            this.DataContext = mInstagramLoginDialogVM = new InstagramLoginDialogVM()
            {
                BrowserIsVisible = showLogin
            };

            this.mInstagramLoginDialogVM.Continue += MInstagramLoginDialogVM_Continue;
            this.mInstagramLoginDialogVM.Logout += MInstagramLoginDialogVM_Logout;

            if (showLogin)
            {
                IntPtr windowHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
                if (windowHandle != IntPtr.Zero)
                {
                    WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & ~WinAPI.WS_MINIMIZEBOX);
                    WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & ~WinAPI.WS_MAXIMIZEBOX);
                }

                this.Height = 580;
                this.Width = 660;
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

        public static LoginState Show(bool login)
        {
            LoginState state = LoginState.Failed;

            if (App.Current.CheckAccess())
            {
                Window owner = Application.Current.Windows[Application.Current.Windows.Count - 1];

                InstagramLoginWindow window = new InstagramLoginWindow(login)
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
            wbInstagramLogin.Navigate(InstagramManager.OAuthURL, null, null, "User-Agent: ; MSIE xyz;");
        }


        #region Event handlers

        private async void WbInstagramLogin_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsolutePath == "blank")
            {
                wbInstagramLogin.Navigate(InstagramManager.OAuthURL, null, null, "User-Agent: ; MSIE xyz;");
            }
            else
            {
                var code = HttpUtility.ParseQueryString(e.Uri.Query)["code"];
                if (!string.IsNullOrWhiteSpace(code))
                {
                    DialogResult = await InstagramManager.GetAccessCode(code);

                    if (!DialogResult.GetValueOrDefault(false) && InstagramManager.LastRequestException != null)
                    {
                        MessageBoxWindow.ShowMessageBox("Error", InstagramManager.LastRequestException.Message, MessageBoxWindow.MessageBoxType.Error);
                    }

                    this.Close();
                }
            }
            SetSilent(wbInstagramLogin, true); // make it silent
        }

        private void WbInstagramLogin_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //hide scrollbar
            wbInstagramLogin.InvokeScript("execScript", new Object[] { "document.documentElement.style.overflow='hidden'", "JavaScript" });
        }
        #endregion

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

        private void MInstagramLoginDialogVM_Logout()
        {
            InstagramManager.Logout();

            mbLoggout = true;
            DialogResult = false;
            this.Close();
        }

        private void MInstagramLoginDialogVM_Continue()
        {
            DialogResult = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
