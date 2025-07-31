using System;
using System.Globalization;
using System.Windows.Data;

namespace CapaPresentacionWPF.Converters
{
    // Un conversor para convertir un valor entero (int) a un booleano.
    // Se utiliza para enlazar una propiedad de un radio button (IsChecked)
    // a un campo entero en la base de datos (por ejemplo, 1 para true, 0 para false).
    public class IntToBoolConverter : IValueConverter
    {
        // Convierte un valor entero a un booleano.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 1;
            }
            return false;
        }

        // Convierte un valor booleano a un entero (de vuelta).
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
