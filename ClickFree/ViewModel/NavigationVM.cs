namespace ClickFree.ViewModel
{
    public abstract class NavigationVM: VMBase
    {
        #region Properties

        public INavigation Navigation
        {
            get;
            private set;
        }

        #endregion

        #region Ctor

        public NavigationVM(INavigation navigation)
        {
            this.Navigation = navigation;
        }

        #endregion

        #region Implementation

        public void NavigateTo(NavigateEnum navigateTo)
        {
            Navigation.NavigateTo(navigateTo);
        }

        public void NavigateTo(NavigateEnum navigateTo, object parameter)
        {
            Navigation.NavigateTo(navigateTo, parameter);
        }

        internal protected virtual void Activated(object parameter)
        {

        }

        internal protected virtual void Deactivated()
        {

        }

        #endregion
    }
}
