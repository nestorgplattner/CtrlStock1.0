
using CapaPresentacionWPF.Data;
using CapaPresentacionWPF.Model;
using CapaPresentacionWPF.Servicios;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;


using System.IO;                                     // para File.*
using System.Xml;                                    // para XmlDocument
using System.Security.Cryptography.Xml;              // para SignedXml, Reference, XmlDsigEnvelopedSignatureTransform
using System.Security.Cryptography.X509Certificates; // para X509Certificate2


namespace CapaPresentacionWPF
{
    public partial class VentanaFacturacion : Window
    {
        public ClienteAFIP ClienteSeleccionado { get; private set; }

        public VentanaFacturacion()
        {
            InitializeComponent();

            btnBuscarLocal.Click += BtnBuscarLocal_Click;
            btnBuscarOnline.Click += BtnBuscarOnline_Click;
            btnNuevo.Click += BtnNuevo_Click;
            btnConfirmar.Click += BtnConfirmar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = false;
        }

        private void BtnBuscarLocal_Click(object sender, RoutedEventArgs e)
        {
            string input = txtCUIT.Text.Trim();
            var cliente = BuscarClienteEnBD(input);
            if (cliente != null)
            {
                MostrarCliente(cliente);
            }
            else
            {
                MessageBox.Show("Cliente no encontrado en la base de datos.");
            }
        }

        private async void BtnBuscarOnline_Click(object sender, RoutedEventArgs e)
        {
            string cuit = txtCUIT.Text.Trim();
            if (!EsCUITValido(cuit))
            {
                MessageBox.Show("Formato de CUIT inválido.");
                return;
            }

            var cliente = await BuscarPorCUITOnline(cuit);
            if (cliente != null)
            {
                MostrarCliente(cliente);
            }
            else
            {
                MessageBox.Show("CUIT no encontrado online.");
            }
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            txtNombre.Text = "";
            txtDomicilio.Text = "";
            txtCondicion.Text = "";
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCUIT.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Debe ingresar CUIT/DNI y Nombre.");
                return;
            }

            ClienteSeleccionado = new ClienteAFIP
            {
                CUIT = txtCUIT.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Domicilio = txtDomicilio.Text.Trim(),
                CondicionIVA = txtCondicion.Text.Trim()
            };

            GuardarClienteEnBD(ClienteSeleccionado);
            this.DialogResult = true;
        }

        private void MostrarCliente(ClienteAFIP cliente)
        {
            txtCUIT.Text = cliente.CUIT;
            txtNombre.Text = cliente.Nombre;
            txtDomicilio.Text = cliente.Domicilio;
            txtCondicion.Text = cliente.CondicionIVA;
        }

        private bool EsCUITValido(string cuit)
        {
            return cuit.Length == 11 && long.TryParse(cuit, out _);
        }

        private ClienteAFIP BuscarClienteEnBD(string identificador)
        {
            return ClienteData.BuscarCliente(identificador);
        }

        private void GuardarClienteEnBD(ClienteAFIP cliente)
        {
            ClienteData.GuardarCliente(cliente);
        }

        private async Task<ClienteAFIP> BuscarPorCUITOnline(string cuit)
        {
            try
            {
                if (!File.Exists("Certificados/certificado.pem") || !File.Exists("Certificados/private.key"))
                {
                    MessageBox.Show("No se encontraron los certificados.");
                    return null;
                }

                var (token, sign) = AFIPHelper.ObtenerLoginTicket();

                using (var client = new HttpClient())
                {
                    var soap = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://ser.padron.ws.server.a4.afip.gov.ar/"">
   <soapenv:Header/>
   <soapenv:Body>
      <ser:getPersona_v2>
         <token>{token}</token>
         <sign>{sign}</sign>
         <cuitRepresentado>20304567891</cuitRepresentado>
         <idPersona>{cuit}</idPersona>
      </ser:getPersona_v2>
   </soapenv:Body>
</soapenv:Envelope>";

                    var content = new StringContent(soap, Encoding.UTF8, "text/xml");
                    var response = await client.PostAsync("https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA4", content);
                    var xml = await response.Content.ReadAsStringAsync();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    string nombre = doc.GetElementsByTagName("nombre")[0]?.InnerText ?? "";
                    string domicilio = doc.GetElementsByTagName("domicilioFiscal")[0]?.InnerText ?? "";
                    string condicion = doc.GetElementsByTagName("idIVA")[0]?.InnerText ?? "Sin datos";

                    return new ClienteAFIP
                    {
                        CUIT = cuit,
                        Nombre = nombre,
                        Domicilio = domicilio,
                        CondicionIVA = condicion
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar AFIP: " + ex.Message);
                return null;
            }
        }
    }
}