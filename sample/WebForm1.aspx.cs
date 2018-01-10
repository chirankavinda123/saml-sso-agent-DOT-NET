using Kentor.AuthServices;
using Kentor.AuthServices.Saml2P;
using Kentor.AuthServices.WebSso;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Org.BouncyCastle.Pkcs;

using B = Org.BouncyCastle.X509; //Bouncy certificates
using W = System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;

namespace sample
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Saml2AuthenticationRequest samlRequest = new Saml2AuthenticationRequest();

            samlRequest.AssertionConsumerServiceUrl = new Uri("http://localhost:8080/demo/callback");
            samlRequest.DestinationUrl = new Uri("https://localhost:9443/samlsso");
            samlRequest.ForceAuthentication = false;
            //id
            //issuer instant
            //version
            samlRequest.Binding = Saml2BindingType.HttpPost;

            samlRequest.Issuer = new EntityId("demo-sso-agent");

            Saml2NameIdPolicy nameIdPolocy = new Saml2NameIdPolicy(true, NameIdFormat.Persistent);

            Saml2RequestedAuthnContext saml2RequestedAuthnContext = new Saml2RequestedAuthnContext(
                new Uri("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport"), AuthnContextComparisonType.Exact);
            samlRequest.RequestedAuthnContext = saml2RequestedAuthnContext;

            //sign
            var baseFolder = AppDomain.CurrentDomain.BaseDirectory;
            string certificateFilePath = $"{baseFolder}\\wso2carbon.p12";
            
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certificatesCollection = store.Certificates;
           
            X509Certificate2 cert = null;
            RSACryptoServiceProvider cryptKey = null;
            foreach (X509Certificate2 ce in certificatesCollection)
            {
                if (ce.FriendlyName.Equals("wso2carbon") && ce.HasPrivateKey) {
                    
                    cryptKey = (RSACryptoServiceProvider)ce.PrivateKey;
                    cert = new X509Certificate2(ce);
                }


                // Import the certificates into X509Store objects
            }


            //before trying my own signing ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         /*   XmlDocument xmlDoc = XmlHelpers.XmlDocumentFromString(samlRequest.ToXml());
         
            XmlHelpers.Sign(xmlDoc, cert, false);

            //covert xmlDoc to a String
            StringWriter stringWriter = new StringWriter();
            XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter);
            
            xmlDoc.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            String samlRequestString = stringWriter.GetStringBuilder().ToString();
            //end of xmlDoc convert back to String
       
       */
            String samlRequestString = "SAMLRequest=" + EncodeSamlAuthnRequest(samlRequest.ToXml());


            String signedReq = DoSignRequest(samlRequestString,cryptKey);
            
            //finally redirect
            Response.Redirect(string.Concat("https://localhost:9443/samlsso" , "?", signedReq));
        }

        private String DoSignRequest(string samlRequestString,RSACryptoServiceProvider key)
        {
            String sigAlgAdded =  String.Concat(samlRequestString,"&SigAlg=", HttpUtility.UrlEncode("http://www.w3.org/2000/09/xmldsig#rsa-sha1", Encoding.UTF8));

            byte[] data = Encoding.UTF8.GetBytes(sigAlgAdded);
           
            byte[] sig = key.SignData(data,new SHA1Managed());

            String base64Str = Convert.ToBase64String(sig,Base64FormattingOptions.None);
            
            return String.Concat(sigAlgAdded,"&Signature=", HttpUtility.UrlEncode(base64Str));
        }

        public static string EncodeSamlAuthnRequest(String authnRequest)
        {
            var bytes = Encoding.UTF8.GetBytes(authnRequest);
            using (var output = new MemoryStream())
            {
                using (var zip = new DeflateStream(output, CompressionMode.Compress))
                {
                    zip.Write(bytes, 0, bytes.Length);
                }
                var base64 = Convert.ToBase64String(output.ToArray());
                return HttpUtility.UrlEncode(base64);
            }
        }



    }
        
    
}