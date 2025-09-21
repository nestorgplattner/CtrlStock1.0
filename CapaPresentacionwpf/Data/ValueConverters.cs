using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;


namespace CapaPresentacionWPF.Converters
{
    public class StockToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                if (stock == 0)
                    return new SolidColorBrush(Colors.Red);
                if (stock <= 5)
                    return new SolidColorBrush(Colors.Orange);
                return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue != 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? 1 : 0;
            return 0;
        }
    }

    

}