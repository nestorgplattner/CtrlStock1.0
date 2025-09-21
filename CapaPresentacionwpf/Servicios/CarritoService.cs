using System.Collections.Generic;
using System.Linq;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Servicios
{
    public static class CarritoService
    {
        public static void AgregarProductoAlCarrito(List<ItemCarrito> carrito, Producto producto)
        {
            var item = carrito.FirstOrDefault(i => i.IdProducto == producto.IdProducto);
            if (item != null)
            {
                carrito.Remove(item);
                item.Cantidad++;
                carrito.Add(item);
            }
            else
            {
                carrito.Add(new ItemCarrito
                {
                    IdProducto = producto.IdProducto,
                    Codigo = producto.Codigo,
                    Nombre = producto.Nombre,
                    PrecioUnitario = producto.PrecioFinal,
                    Cantidad = 1
                });
            }
            producto.Stock--;
        }

        public static void EliminarItemDelCarrito(List<ItemCarrito> carrito, List<Producto> productosDisponibles, ItemCarrito item)
        {
            if (item.IdProducto != -1)
            {
                var producto = productosDisponibles.FirstOrDefault(p => p.IdProducto == item.IdProducto);
                if (producto != null)
                {
                    producto.Stock += item.Cantidad;
                }
            }
            carrito.Remove(item);
        }
    }
}