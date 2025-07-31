using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

// El namespace debe coincidir con la ruta de la carpeta
namespace CapaPresentacionWPF.Converters
{
    // Convierte un valor de stock (int) a un color (Brush)
    public class StockToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                if (stock == 0)
                    return new SolidColorBrush(Colors.Red); // Stock 0
                if (stock <= 5)
                    return new SolidColorBrush(Colors.Orange); // Stock bajo (1-5)

                return new SolidColorBrush(Colors.Green); // Stock suficiente
            }
            return new SolidColorBrush(Colors.Gray); // Valor por defecto
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Convierte un entero a un booleano (útil para CheckBox)
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 1; // True si el valor es 1
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0;
            }
            return 0;
        }
    }
}
