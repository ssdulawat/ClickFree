/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ClickFree"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using ClickFree.Helpers;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        #region Properties

        public RootViewModel Root
        {
            get
            {
                return ServiceLocator.Current.GetInstance<RootViewModel>();
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<RootViewModel>();
        }

        static ViewModelLocator()
        {
            DriveManager.DriveStateChanged += DriveManager_DriveStateChanged;
        }

        #endregion

        #region Event handlers

        private static void DriveManager_DriveStateChanged(DriveState state, UsbDisk disk)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Methods

        #region Static 

        public static void Cleanup()
        {
            SimpleIoc.Default.Unregister<RootViewModel>();
        }

        #endregion

        #endregion
    }
}