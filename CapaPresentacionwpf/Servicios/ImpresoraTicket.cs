using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Servicios
{
    public static class ImpresoraTicket
    {
        private static List<ItemCarrito> _carrito;
        private static List<PagoTemporal> _pagos;
        private static decimal _subtotal;
        private static decimal _descuento;
        private static decimal _total;
        private static decimal _totalPagado;
        private static string _nombreNegocio = "Mi Negocio";
        private static string _direccion = "Dirección Ejemplo 123";
        private static string _telefono = "Tel: 1234-5678";
        private static DateTime _fechaVenta;

        public static void ImprimirTicket(
            List<ItemCarrito> carrito,
            List<PagoTemporal> pagos,
            decimal subtotal,
            decimal descuento,
            decimal total,
            decimal totalPagado,
            DateTime fechaVenta,
            string nombreNegocio = null,
            string direccion = null,
            string telefono = null)
        {
            _carrito = carrito;
            _pagos = pagos;
            _subtotal = subtotal;
            _descuento = descuento;
            _total = total;
            _totalPagado = totalPagado;
            _fechaVenta = fechaVenta;
            if (!string.IsNullOrEmpty(nombreNegocio)) _nombreNegocio = nombreNegocio;
            if (!string.IsNullOrEmpty(direccion)) _direccion = direccion;
            if (!string.IsNullOrEmpty(telefono)) _telefono = telefono;

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += PrintPage;
            // Puedes especificar la impresora aquí si lo deseas:
            // pd.PrinterSettings.PrinterName = "NombreDeTuImpresora";
            pd.Print();
        }

        private static void PrintPage(object sender, PrintPageEventArgs e)
        {
            float y = 10;
            float leftMargin = 10;
            Font font = new Font("Consolas", 9);
            Font fontBold = new Font("Consolas", 9, FontStyle.Bold);

            e.Graphics.DrawString(_nombreNegocio, fontBold, Brushes.Black, leftMargin, y);
            y += 18;
            e.Graphics.DrawString(_direccion, font, Brushes.Black, leftMargin, y);
            y += 15;
            e.Graphics.DrawString(_telefono, font, Brushes.Black, leftMargin, y);
            y += 20;
            e.Graphics.DrawString($"Fecha: {_fechaVenta:dd/MM/yyyy HH:mm}", font, Brushes.Black, leftMargin, y);
            y += 20;
            e.Graphics.DrawString("----------------------------------------", font, Brushes.Black, leftMargin, y);
            y += 15;
            e.Graphics.DrawString("Producto        Cant  P.Unit   Subtotal", fontBold, Brushes.Black, leftMargin, y);
            y += 15;
            e.Graphics.DrawString("----------------------------------------", font, Brushes.Black, leftMargin, y);
            y += 15;

            foreach (var item in _carrito)
            {
                string nombre = item.Nombre.Length > 14 ? item.Nombre.Substring(0, 14) : item.Nombre;
                string linea = $"{nombre.PadRight(14)} {item.Cantidad,3} {item.PrecioUnitario,7:C0} {item.Cantidad * item.PrecioUnitario,9:C0}";
                e.Graphics.DrawString(linea, font, Brushes.Black, leftMargin, y);
                y += 15;
            }

            y += 10;
            e.Graphics.DrawString("----------------------------------------", font, Brushes.Black, leftMargin, y);
            y += 15;
            e.Graphics.DrawString($"Subtotal: {_subtotal.ToString("C", CultureInfo.CurrentCulture)}", font, Brushes.Black, leftMargin, y);
            y += 15;
            e.Graphics.DrawString($"Descuento: {_descuento.ToString("C", CultureInfo.CurrentCulture)}", font, Brushes.Black, leftMargin, y);
            y += 15;
            e.Graphics.DrawString($"TOTAL: {_total.ToString("C", CultureInfo.CurrentCulture)}", fontBold, Brushes.Black, leftMargin, y);
            y += 20;
            e.Graphics.DrawString("Pagos:", fontBold, Brushes.Black, leftMargin, y);
            y += 15;
            foreach (var pago in _pagos)
            {
                e.Graphics.DrawString($"{pago.FormaPagoDisplay}: {pago.Monto.ToString("C", CultureInfo.CurrentCulture)}", font, Brushes.Black, leftMargin, y);
                y += 15;
            }
            y += 10;
            e.Graphics.DrawString($"Total Pagado: {_totalPagado.ToString("C", CultureInfo.CurrentCulture)}", font, Brushes.Black, leftMargin, y);
            y += 15;
            e.Graphics.DrawString("----------------------------------------", font, Brushes.Black, leftMargin, y);
            y += 20;
            e.Graphics.DrawString("¡Gracias por su compra!", fontBold, Brushes.Black, leftMargin, y);
        }
    }
}