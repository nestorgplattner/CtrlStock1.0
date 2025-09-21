using System;
using System.Collections.Generic;

namespace CapaPresentacionWPF.Model
{
    /// <summary>
    /// Modelo de datos para representar un pago de Mercado Pago.
    /// Las propiedades coinciden con la respuesta de la API de Mercado Pago.
    /// </summary>
    public class MercadoPagoPayment
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public decimal TransactionAmount { get; set; }
        public string CurrencyId { get; set; }
        public DateTime DateCreated { get; set; }
        public string Description { get; set; }
        public string PaymentTypeId { get; set; }
        public string POSId { get; set; }
        public string CollectorId { get; set; }
    }
}
