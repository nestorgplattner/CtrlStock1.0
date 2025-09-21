using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Servicios
{
    public static class PagoService
    {
        public static bool ValidarMontoPago(string textToParse, out decimal monto, out string mensajeError)
        {
            mensajeError = null;
            monto = 0m;

            // Normalizar el separador decimal (reemplazar coma con punto)
            textToParse = textToParse.Replace(",", ".");

            // Eliminar separadores de miles (todos los puntos excepto el último, si existe)
            int lastDotIndex = textToParse.LastIndexOf('.');
            if (lastDotIndex != -1)
            {
                string integerPart = textToParse.Substring(0, lastDotIndex).Replace(".", "");
                string decimalPart = textToParse.Substring(lastDotIndex + 1);
                textToParse = integerPart + "." + decimalPart;
            }

            if (!decimal.TryParse(textToParse, NumberStyles.Number, CultureInfo.InvariantCulture, out monto))
            {
                mensajeError = "Formato de monto inválido. Por favor, use solo números, comas o puntos como separadores decimales (ej: 1234.56 o 1.234,56).";
                return false;
            }
            if (monto < 0)
            {
                mensajeError = "El monto a pagar debe ser mayor o igual a cero.";
                return false;
            }
            return true;
        }
    }
}