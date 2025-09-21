using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacionWPF.Model
{
    /// <summary>
    /// Clase para representar las formas de pago de manera simplificada en la UI.
    /// </summary>
    public class FormaPagoSimple
    {
        public int IdFormaPago { get; set; }
        public string Plataforma { get; set; }
        public string MedioPago { get; set; }
        public string NombreDisplay => $"{Plataforma} - {MedioPago}";
    }
}
