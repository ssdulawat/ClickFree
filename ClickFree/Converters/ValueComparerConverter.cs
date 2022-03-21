using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClickFree.Converters
{
    public class ValueComparerConverter : IValueConverter
    {
        #region Static

        public static readonly ValueComparerConverter Instance = new ValueComparerConverter();

        #endregion

        #region Implementation of IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == parameter)
                return true;
            if (value == null || parameter == null)
                return false;
            else
            {
                foreach (var p in parameter.ToString().Split('|'))
                {
                    if (string.Compare(value.ToString(), p, true) == 0)
                        return true;
                }

                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        } 
        #endregion
    }

    public class VisibilityValueComparerConverter : IValueConverter
    {
        #region Implementation of IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)ValueComparerConverter.Instance.Convert(value, targetType, parameter, culture) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}
