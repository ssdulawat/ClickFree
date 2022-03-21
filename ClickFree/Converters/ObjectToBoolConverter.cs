namespace ClickFree.Converters
{
    using System;
    using System.Windows.Data;

    public class ObjectToBoolConverter : IValueConverter
    {
        #region Static

        public static readonly ObjectToBoolConverter Instance = new ObjectToBoolConverter();

        #endregion

        #region Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result;

            if (value == null) result = false;
            else if (value is bool) result = (bool)value;
            else if (value is string) result = !string.IsNullOrWhiteSpace((string)value);
            else if (value is int) result = ((int)value) > 0;
            else if (value is double) result = ((double)value) > 0;
            else throw new NotSupportedException("Type is not supported");

            return (parameter is string param && param.Contains("invert")) ? !result : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
