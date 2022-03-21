using ClickFree.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ClickFree.Controls
{
    /// <summary>
    /// Interaction logic for WindowsTitle.xaml
    /// </summary>
    public partial class WindowsTitle : UserControl
    {
        #region Properties

        #region Title

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(WindowsTitle), new PropertyMetadata("Click Free"));

        #endregion

        #region TitleHeight

        public double TitleHeight
        {
            get { return (double)GetValue(TitleHeightProperty); }
            set { SetValue(TitleHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleHeightProperty =
            DependencyProperty.Register("TitleHeight", typeof(double), typeof(WindowsTitle), new PropertyMetadata(32.0d));

        #endregion

        #region TitleForeground

        public Color TitleForeground
        {
            get { return (Color)GetValue(TitleForegroundProperty); }
            set { SetValue(TitleForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleForegroundProperty =
            DependencyProperty.Register("TitleForeground", typeof(Color), typeof(WindowsTitle), new PropertyMetadata(Colors.White));

        #endregion

        #region TitleBackground

        public Color TitleBackground
        {
            get { return (Color)GetValue(TitleBackgroundProperty); }
            set { SetValue(TitleBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleBackgroundProperty =
            DependencyProperty.Register("TitleBackground", typeof(Color), typeof(WindowsTitle), new PropertyMetadata(Color.FromArgb(255, 23, 24, 28)));


        #endregion

        #region CanMinimize

        public bool CanMinimize
        {
            get { return (bool)GetValue(CanMinimizeProperty); }
            set { SetValue(CanMinimizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanMinimize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanMinimizeProperty =
            DependencyProperty.Register("CanMinimize", typeof(bool), typeof(WindowsTitle), new PropertyMetadata(true, OnCanMinimize));

        public static void OnCanMinimize(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WindowsTitle window)
            {
                window.UpdateMinimizeButton();
            }
        }

        #endregion


        #region CanMaximize

        public bool CanMaximize
        {
            get { return (bool)GetValue(CanMaximizeProperty); }
            set { SetValue(CanMaximizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanMaximize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanMaximizeProperty =
            DependencyProperty.Register("CanMaximize", typeof(bool), typeof(WindowsTitle), new PropertyMetadata(true, OnCanMaximize));

        public static void OnCanMaximize(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is WindowsTitle window)
            {
                window.UpdateMaximizeButton();
            }
        }

        #endregion

        #endregion

        #region Ctor
        public WindowsTitle()
        {
            InitializeComponent();
            
            DataContext = this;
        }
        #endregion

        #region Implementation

        protected void UpdateMinimizeButton()
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                IntPtr windowHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
                if (windowHandle != IntPtr.Zero)
                {
                    if (CanMinimize)
                        WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & WinAPI.WS_MINIMIZEBOX);
                    else
                        WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & ~WinAPI.WS_MINIMIZEBOX);
                }
            }
        }

        protected void UpdateMaximizeButton()
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                IntPtr windowHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
                if (windowHandle != IntPtr.Zero)
                {
                    if (CanMaximize)
                        WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & WinAPI.WS_MAXIMIZEBOX);
                    else
                        WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE) & ~WinAPI.WS_MAXIMIZEBOX);
                }
            }
        }


        #endregion

        #region Event handlers

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(Window.GetWindow(this));
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(Window.GetWindow(this));
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(Window.GetWindow(this));
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(Window.GetWindow(this));
        }

        private void WindowsTitle_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMinimizeButton();
            UpdateMaximizeButton();
            //WindowChrome.SetWindowChrome(Window.GetWindow(this), new WindowChrome() { CaptionHeight = 40 , CornerRadius= new CornerRadius(10)});
        } 

        #endregion
    }
}
