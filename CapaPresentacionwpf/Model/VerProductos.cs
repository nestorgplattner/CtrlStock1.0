using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacionWPF.Model
{
    public class VerProductos
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public double PrecioFinal { get; set; }
        public int Stock { get; set; }
        public decimal PrecioCompra { get; set; }
    }
   
}
