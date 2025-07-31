using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace CapaPresentacionWPF.Servicios
{
    public class AFIPHelper
    {
        private const string SERVICE = "ws_sr_padron";
        private const string URL_WSAA = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
        private const string URL_PADRON = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA4";

        // Ruta y contraseña de tu certificado PFX
        private static string certPfxPath = @"C:\Users\platt\certificado.pfx";
        private static string certPfxPassword = "1234";

        public static (string Token, string Sign) ObtenerLoginTicket()
        {
            string traPath = GenerarTRA();
            byte[] traFirmado = FirmarTRA(traPath);
            string loginCmsBase64 = Convert.ToBase64String(traFirmado);

            return LlamarWSAA(loginCmsBase64);
        }

        private static string GenerarTRA()
        {
            string xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<loginTicketRequest version=""1.0"">
  <header>
    <uniqueId>{Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds)}</uniqueId>
    <generationTime>{DateTime.UtcNow.AddMinutes(-10):s}</generationTime>
    <expirationTime>{DateTime.UtcNow.AddMinutes(10):s}</expirationTime>
  </header>
  <service>{SERVICE}</service>
</loginTicketRequest>";

            string tempPath = Path.GetTempFileName();
            File.WriteAllText(tempPath, xml, Encoding.UTF8);
            return tempPath;
        }

        private static byte[] FirmarTRA(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(xmlPath);

            var cert = new X509Certificate2(certPfxPath, certPfxPassword, X509KeyStorageFlags.Exportable);
            var signedXml = new SignedXml(xmlDoc);
            signedXml.SigningKey = cert.GetRSAPrivateKey();

            var reference = new System.Security.Cryptography.Xml.Reference();
            reference.Uri = "";

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);
            signedXml.ComputeSignature();

            XmlElement xmlDigitalSignature = signedXml.GetXml();
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

            using (MemoryStream ms = new MemoryStream())
            {
                xmlDoc.Save(ms);
                return ms.ToArray();
            }
        }

        private static (string Token, string Sign) LlamarWSAA(string loginCms)
        {
            var soapEnvelope = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://wsaa.view.sua.dvadac.desein.afip.gov"">
  <soapenv:Header/>
  <soapenv:Body>
    <ws:loginCms>
      <ws:in0>{loginCms}</ws:in0>
    </ws:loginCms>
  </soapenv:Body>
</soapenv:Envelope>";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_WSAA);
            request.Method = "POST";
            request.ContentType = "text/xml;charset=utf-8";
            request.Headers.Add("SOAPAction", "loginCms");

            byte[] bytes = Encoding.UTF8.GetBytes(soapEnvelope);
            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (WebResponse response = request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string responseXml = reader.ReadToEnd();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseXml);

                string token = doc.GetElementsByTagName("token")[0]?.InnerText ?? "";
                string sign = doc.GetElementsByTagName("sign")[0]?.InnerText ?? "";

                return (token, sign);
            }
        }

        // Consulta al padrón AFIP para CUIT
        public static (string Nombre, string Domicilio, string CondicionIVA) ConsultarPadron(string token, string sign, string cuitRepresentado, string cuitConsultado)
        {
            var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://ser.padron.ws.server.a4.afip.gov.ar/"">
   <soapenv:Header/>
   <soapenv:Body>
      <ser:getPersona_v2>
         <token>{token}</token>
         <sign>{sign}</sign>
         <cuitRepresentado>{cuitRepresentado}</cuitRepresentado>
         <idPersona>{cuitConsultado}</idPersona>
      </ser:getPersona_v2>
   </soapenv:Body>
</soapenv:Envelope>";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_PADRON);
            request.Method = "POST";
            request.ContentType = "text/xml;charset=utf-8";
            request.Headers.Add("SOAPAction", "");

            byte[] bytes = Encoding.UTF8.GetBytes(soapEnvelope);
            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (WebResponse response = request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string responseXml = reader.ReadToEnd();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseXml);

                // Extraemos valores (validar que existan)
                string nombre = doc.GetElementsByTagName("nombre")[0]?.InnerText ?? "";
                string domicilio = doc.GetElementsByTagName("domicilioFiscal")[0]?.InnerText ?? "";
                string condicionIVA = doc.GetElementsByTagName("idIVA")[0]?.InnerText ?? "";

                return (nombre, domicilio, condicionIVA);
            }
        }
    }
}