using System;
using System.Collections.Generic;
using System.Linq;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Servicios
{
    public static class DescuentoService
    {
        public static (decimal subtotal, decimal montoDescuento, decimal total) CalcularTotales(
            List<ItemCarrito> carrito, decimal porcentajeDescuento)
        {
            decimal subtotal = carrito.Sum(i => i.PrecioUnitario * i.Cantidad);
            decimal montoDescuento = subtotal * (porcentajeDescuento / 100m);
            decimal total = subtotal - montoDescuento;
            return (subtotal, montoDescuento, total);
        }
    }
}