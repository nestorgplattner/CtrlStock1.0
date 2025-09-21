using System.Collections.Generic;
using System.Linq;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Servicios
{
    public static class VentaRapidaService
    {
        public static void AgregarVentaRapida(List<ItemCarrito> carrito, string tipoRapidoSeleccionado, decimal importe)
        {
            var existingRapidItem = carrito.FirstOrDefault(item =>
                item.IdProducto == -1 &&
                item.Codigo == tipoRapidoSeleccionado.ToUpper());

            if (existingRapidItem != null)
            {
                carrito.Remove(existingRapidItem);
                existingRapidItem.Cantidad++;
                existingRapidItem.PrecioUnitario += importe;
                existingRapidItem.Nombre = $"{tipoRapidoSeleccionado} (x{existingRapidItem.Cantidad})";
                carrito.Add(existingRapidItem);
            }
            else
            {
                carrito.Add(new ItemCarrito
                {
                    IdProducto = -1,
                    Codigo = tipoRapidoSeleccionado.ToUpper(),
                    Nombre = $"{tipoRapidoSeleccionado} (x1)",
                    PrecioUnitario = importe,
                    Cantidad = 1
                });
            }
        }
    }
}