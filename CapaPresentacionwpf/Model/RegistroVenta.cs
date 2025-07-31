using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacionWPF.Model
{
    public class RegistroVenta
    {
        public int IdVenta { get; set; }
        public string Fecha { get; set; }
        public string ClienteNombre { get; set; }
        public string CUIT_CUIL { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string ComprobanteTipo { get; set; }
        public string FormaPago { get; set; }
        public decimal Subtotal { get; set; }
        public int DescuentoPorcentaje { get; set; }
        public decimal IVA { get; set; }
        public decimal TotalComision { get; set; }
        public decimal Total { get; set; }
        public decimal TotalRecibido { get; set; }
    }
}
