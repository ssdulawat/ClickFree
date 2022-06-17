﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClickFree.Views
{
    /// <summary>
    /// Interaction logic for EraseDeviceView.xaml
    /// </summary>
    public partial class EraseDeviceView : UserControl
    {
        #region Ctor
        public EraseDeviceView()
        {
            InitializeComponent();
        }

        #endregion

        #region Fields

        private Point mScrollMousePoint = new Point();
        private double mOffset = 1;

        #endregion

        #region Event handlers

        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //mScrollMousePoint = e.GetPosition(scrollViewer);
            //mOffset = scrollViewer.HorizontalOffset;
            //scrollViewer.CaptureMouse();
        }

        private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //scrollViewer.ReleaseMouseCapture();
        }

        private void ScrollViewer_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //if (scrollViewer.IsMouseCaptured)
            //{
            //    scrollViewer.ScrollToHorizontalOffset(mOffset + (mScrollMousePoint.X - e.GetPosition(scrollViewer).X));
            //}
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + e.Delta);
        }


        /// <summary>
        /// Workaround: scrollviewer blocks all events
        /// </summary>
        private void ScrollViewer_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var source = scrollViewer.InputHitTest(e.GetPosition(scrollViewer as System.Windows.IInputElement));
            object tag = null;

            FrameworkElement fe = null;
            FrameworkContentElement fce = null;

            do
            {
                if (source is FrameworkElement feArg)
                {
                    fe = feArg;

                    if (fe.Tag != null)
                    {
                        tag = fe.Tag;
                        break;
                    }
                }
                else if (source is FrameworkContentElement fceArg)
                {
                    fce = fceArg;

                    if (fce.Tag != null)
                    {
                        tag = fce.Tag;
                        break;
                    }
                }

                source = (fe?.Parent ?? fce?.Parent) as FrameworkElement;

            }
            while (source != null);

            if (tag != null)
            {
                (e.OriginalSource as FrameworkElement).Tag = tag;

                icHeaders.RaiseEvent(e);
            }
        }


        #endregion
    }
}
