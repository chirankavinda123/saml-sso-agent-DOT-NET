using Kentor.AuthServices;
using Kentor.AuthServices.Saml2P;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Metadata;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;
using System.Net;

namespace org.wso2.carbon.saml.agent.saml
{
    public class SAML2SSOManager
    {
        public SAML2SSOManager() {
            //TODO: add ssoAgentConfig as a param
        }

        public String BuildRedirectRequest()
        {
            AuthenticationRequest samlRequest = new AuthenticationRequest();

            samlRequest.AssertionConsumerServiceUrl = new Uri("http://localhost:49763/sample/callback");
            samlRequest.DestinationUrl = new Uri("https://localhost:9443/samlsso");
            samlRequest.ForceAuthentication = false;

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
                if (ce.FriendlyName.Equals("wso2carbon") && ce.HasPrivateKey)
                {
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

            //buid saml redirect request
            String samlRequestString = "SAMLRequest=" + EncodeSamlAuthnRequest(samlRequest.buildRedirectRequest());
            
            String signedReq = DoSignRequest(samlRequestString, cryptKey);

            //finally redirect http-redirect binding
            return string.Concat("https://localhost:9443/samlsso", "?", signedReq);  
        }

        public void BuildPOSTRequest(HttpContext context) {

            AuthenticationRequest samlRequest = new AuthenticationRequest();

            samlRequest.AssertionConsumerServiceUrl = new Uri("http://localhost:49763/sample/callback");
            samlRequest.DestinationUrl = new Uri("https://localhost:9443/samlsso");
            samlRequest.ForceAuthentication = false;

            samlRequest.Issuer = new EntityId("demo-sso-agent");

            Saml2NameIdPolicy nameIdPolocy = new Saml2NameIdPolicy(true, NameIdFormat.Persistent);

            Saml2RequestedAuthnContext saml2RequestedAuthnContext = new Saml2RequestedAuthnContext(
                new Uri("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport"), AuthnContextComparisonType.Exact);
            samlRequest.RequestedAuthnContext = saml2RequestedAuthnContext;
            
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certificatesCollection = store.Certificates;

            X509Certificate2 cert = null;
            RSACryptoServiceProvider cryptKey = null;
            foreach (X509Certificate2 ce in certificatesCollection)
            {
                if (ce.FriendlyName.Equals("wso2carbon") && ce.HasPrivateKey)
                {
                    cryptKey = (RSACryptoServiceProvider)ce.PrivateKey;
                    cert = new X509Certificate2(ce);
                }
            }

            //buid saml redirect request
            String samlRequestString =  EncodeSamlAuthnRequest(samlRequest.BuildSimpleRequest());

            String signedReq = DoSignRequest(samlRequestString, cryptKey);

            

           // context.Response.Redirect(string.Concat("https://localhost:9443/samlsso", "?", signedReq));
            //sendPOSTRequest(samlRequestString);

            context.Response.Clear();

            context.Response.ContentType = "text/html";
            StringBuilder sb = new StringBuilder();
           
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'><p>you are redirecting to IDP ...</p>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", "https://localhost:9443/samlsso");
            sb.AppendFormat("<input type='hidden' name='SAMLRequest' value='{0}'>", EncodeSamlAuthnRequestPOSTBinding(samlRequest.buildPOSTRequest(cert)));

            // Other params go here
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            
            context.Response.Write(sb.ToString());

            context.Response.End();
        }

        private void sendPOSTRequest(String postDataSet)
        {
            WebRequest request = WebRequest.Create("https://localhost:9443/samlsso");
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            // Set the Method property of the request to POST.  
            request.Method = "POST";
            // Create POST data and convert it to a byte array.  
            string postData = postDataSet;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.  
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.  
            request.ContentLength = byteArray.Length;
            // Get the request stream.  
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.  
            dataStream.Close();
           
            
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            Console.WriteLine(responseFromServer);
            // Clean up the streams.  
   
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        public void ProcessSAMLResponse(HttpRequest request,HttpResponse response)
        {
            String samlResponse = request.Params["SAMLResponse"];

            if (samlResponse != null) {
                String decodedResponse = Encoding.UTF8.GetString(Convert.FromBase64String(samlResponse));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(decodedResponse);
                XmlElement element = xmlDoc.DocumentElement;
                if (element.LocalName.Equals("LogoutResponse"))
                {
                    //this is a logout response
                    //SHUOLD GO TO WELCOME OR INDEX
                }
                else {
                    
                    ProcessSSOResponse(request,response);
                }
            }
        }

        private void ProcessSSOResponse(HttpRequest request,HttpResponse response)
        {
            String samlResponseString = request.Params["SAMLResponse"];

            if (samlResponseString != null)
            {
                String decodedResponse = Encoding.UTF8.GetString(Convert.FromBase64String(samlResponseString));

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(decodedResponse);
                XmlElement xmlElement = xmlDoc.DocumentElement;

                ValidateResponse(xmlElement);

                ValidateAssertionValidityPeriod(xmlElement);

                ValidateAudienceRestriction(xmlDoc);

                //TODO : validate signature
                XmlDocument doc = new XmlDocument() { PreserveWhitespace = true};
                doc.LoadXml(decodedResponse);
                ValidateSignature(doc);

                Dictionary<String, String> claims = ProcessAttributeStatement(xmlDoc);
                
                response.Redirect("https://www.validationsPerformed.com?validationResult?dictionaryCount=" + claims.Count);
            }

        }

        private Dictionary<String, String> ProcessAttributeStatement(XmlDocument xmlDocument)
        {
            Dictionary<String, String> claimsDictionary = new Dictionary<string, string>();
            
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            nsManager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

            XmlNode attributeStmtXmlNode = xmlDocument.SelectSingleNode("//saml2:AttributeStatement",nsManager);
            XmlNodeList attributeNodesList = attributeStmtXmlNode.SelectNodes("//saml2:Attribute",nsManager);

            foreach (XmlNode attributeNode in attributeNodesList) {
                String attributeName = attributeNode.Attributes["Name"].Value;
                String attributeValue = attributeNode.FirstChild.InnerText;
                claimsDictionary[attributeName] = attributeValue;
            }

            return claimsDictionary;
        }

        private void ValidateSignature(XmlDocument Doc)
        {
            if (Doc == null)
                throw new ArgumentException("Doc");

            SignedXml signedXml = new SignedXml(Doc);
            var nsManager = new XmlNamespaceManager(Doc.NameTable);
            nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var node = Doc.SelectSingleNode("//ds:Signature", nsManager);
            // find signature node
            var certElement = Doc.SelectSingleNode("//ds:X509Certificate", nsManager);
            // find certificate node
            var cert = new X509Certificate2(Convert.FromBase64String(certElement.InnerText));
            signedXml.LoadXml((XmlElement)node);

            if (!signedXml.CheckSignature()) {
                throw new Exception("Signature validation Failed for SAML Response. Cannot Proceed Further.");
            }
        }

        private void ValidateAudienceRestriction(XmlDocument xmlDocument)
        {
            XmlElement response = xmlDocument.DocumentElement;
            
            XmlNodeList audienceRestrictions = response.GetElementsByTagName("saml2:AudienceRestriction");
            if (audienceRestrictions.Count > 0) {
                Boolean expectedAudiencefound = false;

                foreach (XmlNode audienceRestriction in audienceRestrictions) {
                    XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDocument.NameTable);
                    nsManager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
                    XmlNodeList audiences = audienceRestriction.SelectNodes("/saml2:AudienceRestriction/saml2:Audience",nsManager);
                    foreach (XmlNode audience in audiences)
                    {
                        if ("demo-sso-agent".Equals(audience.InnerText)) {
                            expectedAudiencefound = true;
                            break;
                        }
                    }
                }
                if (expectedAudiencefound) {
                    throw new Exception("Audience restriction valudation failed for the SAML Assertion.");
                }
            }
            else
            {
                throw new Exception("Response does not contain audience restrictions");
            }
        }

        private void ValidateAssertionValidityPeriod(XmlElement response)
        {
            XmlNodeList conditionsNodeList =  response.GetElementsByTagName("Conditions");
            if (conditionsNodeList.Count == 1) {
                
                DateTime notBefore = DateTime.ParseExact(conditionsNodeList[0].Attributes["NotBefore"].Value, "yyyy-MM-ddThh:mm:ssZ", 
                    CultureInfo.InvariantCulture);
                DateTime notAfter = DateTime.ParseExact(conditionsNodeList[0].Attributes["NotOnOrAfter"].Value, "yyyy-MM-ddThh:mm:ssZ", 
                    CultureInfo.InvariantCulture);

                if (notBefore != null && notBefore.AddSeconds(-300) > DateTime.Now)
                {
                    throw new Exception("Falied to meet assertion condition NotBefore.");
                }
               
                if (notAfter != null && notAfter.AddSeconds(300) < DateTime.Now)
                {
                    throw new Exception("Falied to meet assertion condition NotAfter.");
                }

                if (notBefore > notAfter)
                {
                    throw new Exception("Invalid values for NotBefore and NotAfer conditions since NotBefore > NotAfter.");
                }
            }
        }

        private void ValidateResponse(XmlElement xmlElement)
        {
            if (xmlElement.LocalName != "Response" || xmlElement.NamespaceURI != Saml2Namespaces.Saml2P)
            {
                throw new XmlException("Expected a SAML2 assertion document. Response element was not found!");
            }

            XmlNodeList nodeList = xmlElement.GetElementsByTagName("Response", Saml2Namespaces.Saml2PName);
            if (nodeList.Count > 1)
            {
                throw new XmlException("Invalid Schema for SAML respose since multiple Response elements were detected.");
            }

            if (xmlElement.Attributes["Version"].Value != "2.0")
            {
                throw new XmlException("Unsupported SAML version encountered.Version 2.0 is expected.");
            }

            XmlNodeList assertionNodes = xmlElement.GetElementsByTagName("Assertion", Saml2Namespaces.Saml2Name);
            if (assertionNodes.Count > 1)
            {
                throw new XmlException("Invalid SAML2 schema since multiple Assertion elements were found.");
            }
            
        }

        private String DoSignRequest(string samlRequestString, RSACryptoServiceProvider key)
        {
            String sigAlgAdded = String.Concat(samlRequestString, "&SigAlg=", HttpUtility.UrlEncode("http://www.w3.org/2000/09/xmldsig#rsa-sha1", Encoding.UTF8));

            byte[] data = Encoding.UTF8.GetBytes(sigAlgAdded);

            byte[] sig = key.SignData(data, new SHA1Managed());

            String base64Str = Convert.ToBase64String(sig, Base64FormattingOptions.None);

            return String.Concat(sigAlgAdded, "&Signature=", HttpUtility.UrlEncode(base64Str));
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

        public static string EncodeSamlAuthnRequestPOSTBinding(String authnRequest)
        {
            var bytes = Encoding.UTF8.GetBytes(authnRequest);
            var base64 = Convert.ToBase64String(bytes);
            return base64;
            
        }


    }
}