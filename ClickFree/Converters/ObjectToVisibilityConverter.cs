namespace ClickFree.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    public class ObjectToVisibilityConverter : IValueConverter
    {
        #region Static

        public static readonly ObjectToVisibilityConverter Instance = new ObjectToVisibilityConverter();

        #endregion

        #region Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = (bool)ObjectToBoolConverter.Instance.Convert(value, targetType, parameter, culture);
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
