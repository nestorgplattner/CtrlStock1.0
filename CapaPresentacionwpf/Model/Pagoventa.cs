using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacionwpf.Model
{
    

    public class Producto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int CantidadVendida { get; set; }
        public decimal PrecioFinal { get; set; }
    }

    public class PagoVenta
    {
        public string Plataforma { get; set; }
        public string MedioPago { get; set; }
        public decimal Monto { get; set; }
        public int Cuotas { get; set; }
    }

    public class PagoParcial
    {
        public string Plataforma { get; set; }
        public string MedioPago { get; set; }
        public decimal Monto { get; set; }
        public int Cuotas { get; set; }
    }

}
