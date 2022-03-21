using ClickFree.ViewModel;
using System.Windows;

namespace ClickFree
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Properties

        public static ViewModelLocator Locator { get; private set; }

        #endregion

        #region Overrides

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Locator = this.FindResource("Locator") as ViewModelLocator;
        }

        #endregion
    }
}
