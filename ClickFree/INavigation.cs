using System.Windows.Input;

namespace ClickFree
{
    public enum NavigateEnum
    {
        OnBoarding = -1,
        Main = 0,
        TransferToPC = 1,
        BackupToUSBMain = 2,
        BackupToUSBSelect = 3,
        BackupFacebookMain = 4,
        BackupFacebookDest = 5,
        BackupFacebookSelectImages = 6
    }

    public interface INavigation
    {
        ICommand BackCommand { get; }
        ICommand FAQCommand { get; }
        ICommand ContactUSCommand { get; }
        ICommand GetHelpCommand { get; }
        ICommand EULACommand { get; }
        ICommand EraseCommand { get; }
        ICommand ChatSupportCommand { get; }
        void NavigateTo(NavigateEnum navigateTo);
        void NavigateTo(NavigateEnum navigateTo, object parameter);
    }
}
