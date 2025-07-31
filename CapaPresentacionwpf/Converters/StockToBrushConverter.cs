using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CapaPresentacionWPF.Converters
{
    // Un conversor para cambiar el color de un elemento según un valor de stock.
    // Retorna un color (Brush) basado en el nivel de stock.
    public class StockToBrushConverter : IValueConverter
    {
        // Convierte un valor de stock (int) a un color (Brush).
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                if (stock == 0)
                {
                    // Si el stock es 0, el color es rojo.
                    return new SolidColorBrush(Colors.Red);
                }
                else if (stock <= 10)
                {
                    // Si el stock es bajo (<= 10), el color es amarillo.
                    return new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    // Si el stock es suficiente (> 10), el color es verde.
                    return new SolidColorBrush(Colors.Green);
                }
            }
            // Retorna un color por defecto si el valor no es un entero.
            return new SolidColorBrush(Colors.White);
        }

        // No se necesita una conversión inversa en este caso.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
