using Kentor.AuthServices;
using Kentor.AuthServices.Saml2P;
using Kentor.AuthServices.WebSso;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.IO;

namespace org.wso2.carbon.saml.agent.saml
{
    public class AuthenticationRequest : Saml2AuthenticationRequest
    {
        public static readonly Uri HttpRedirectUri = new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect");
        public static readonly Uri HttpPOSTUri = new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
        public static readonly Uri NameIDFormatPersistentUri = new Uri("urn:oasis:names:tc:SAML:2.0:nameid-format:persistent");

        public String buildRedirectRequest () {
            var x = new XElement(Saml2Namespaces.Saml2P + LocalName);

            x.Add(base.ToXNodes());
            x.Add(new XAttribute("ProtocolBinding", HttpRedirectUri));           
            x.Add(new XAttribute("AssertionConsumerServiceURL", AssertionConsumerServiceUrl));

           //TODO below line
           // x.Add(new XAttribute("AttributeConsumingServiceIndex", AttributeConsumingServiceIndex));

            if (ForceAuthentication)
            {
                x.Add(new XAttribute("ForceAuthn", ForceAuthentication));
            }

            //nameID policy
            XElement nameIDPolicy = new XElement(Saml2Namespaces.Saml2P + "NameIDPolicy");
            nameIDPolicy.Add(new XAttribute("Format", NameIDFormatPersistentUri));
            nameIDPolicy.Add(new XAttribute("SPNameQualifier","Issuer"));
            nameIDPolicy.Add(new XAttribute("AllowCreate", true));
            x.Add(nameIDPolicy);
            //end of nameid policy


            if (RequestedAuthnContext != null && RequestedAuthnContext.ClassRef != null)
            {
                x.Add(new XElement(Saml2Namespaces.Saml2P + "RequestedAuthnContext",
                    new XAttribute("Comparison", RequestedAuthnContext.Comparison.ToString().ToLowerInvariant()),

                    // Add the classref as original string to avoid URL normalization
                    // and make sure the emitted value is exactly the configured.
                    new XElement(Saml2Namespaces.Saml2 + "AuthnContextClassRef",
                        RequestedAuthnContext.ClassRef.OriginalString)));
            }

            return x.ToString();
        }

        public String buildPOSTRequest(X509Certificate2 cert) {
            var x = new XElement(Saml2Namespaces.Saml2P + LocalName);

            x.Add(base.ToXNodes());
            x.Add(new XAttribute("ProtocolBinding", HttpPOSTUri));
            x.Add(new XAttribute("AssertionConsumerServiceURL", AssertionConsumerServiceUrl));

            //TODO below line
            // x.Add(new XAttribute("AttributeConsumingServiceIndex", AttributeConsumingServiceIndex));

            if (ForceAuthentication)
            {
                x.Add(new XAttribute("ForceAuthn", ForceAuthentication));
            }
     
           //nameID policy
           XElement nameIDPolicy = new XElement(Saml2Namespaces.Saml2P + "NameIDPolicy");
           nameIDPolicy.Add(new XAttribute("Format", NameIDFormatPersistentUri));
           nameIDPolicy.Add(new XAttribute("SPNameQualifier", "Issuer"));
           nameIDPolicy.Add(new XAttribute("AllowCreate", true));
           x.Add(nameIDPolicy);
           //end of nameid policy

           if (RequestedAuthnContext != null && RequestedAuthnContext.ClassRef != null)
           {
               x.Add(new XElement(Saml2Namespaces.Saml2P + "RequestedAuthnContext",
                   new XAttribute("Comparison", RequestedAuthnContext.Comparison.ToString().ToLowerInvariant()),

                   // Add the classref as original string to avoid URL normalization
                   // and make sure the emitted value is exactly the configured.
                   new XElement(Saml2Namespaces.Saml2 + "AuthnContextClassRef",
                       RequestedAuthnContext.ClassRef.OriginalString)));
           }

          
            //IF SIGNING IS ENABLED, THEN...

            //generate signature xElement
            //x.Add(createSignatureXElement(cert));
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            BuildSignature(XElementToXMLDocument(x), base.Id.Value, cert).WriteTo(xw);
            return sw.ToString();        
            //end of Signature xElement          
        }

        public XmlDocument XElementToXMLDocument(XElement x) {

            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = true;

            using (XmlWriter xw = XmlWriter.Create(sb, xws))
            {
                XElement child2 = x;
                child2.WriteTo(xw);
            }

            Console.WriteLine(sb.ToString());

            XmlDocument doc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            doc.LoadXml(sb.ToString());

            return doc;
        }

        public XElement XmlElementToXelement(XmlElement e)
        {
            return XElement.Parse(e.OuterXml);
        }

        private XElement CreateSignatureXElement(X509Certificate2 cert)
        {
            XElement Signature = new XElement("ds"+"Signature");
            Signature.Add(new XAttribute("xmlns"+"ds","http://www.w3.org/2000/09/xmldsig#"));

            XElement SignedInfo = new XElement("ds"+"SignedInfo");

            XElement canoMethod = new XElement("ds"+"CanonicalizationMethod");
            canoMethod.Add(new XAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#"));

            XElement signatureMethod = new XElement("ds"+"SignatureMethod");
            signatureMethod.Add("Algorithm", "http://www.w3.org/2000/09/xmldsig#rsa-sha1");

            //transforms section
            XElement transforms = new XElement("ds"+"Transforms");

            XElement transform1 = new XElement("ds"+"Transform");
            transform1.Add(new XAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#enveloped-signature"));

            XElement transform2 = new XElement("ds"+"Transform");
            transform2.Add(new XAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#"));

            transforms.Add(transform1);
            transforms.Add(transform2);
            //end of transforms
          
            XElement reference = new XElement("ds"+"Reference");
            reference.Add(new XAttribute("URI", "#pfx41d8ef22-e612-8c50-9960-1b16f15741b3"));
            reference.Add(transforms);

            SignedInfo.Add(canoMethod);
            SignedInfo.Add(signatureMethod);
            SignedInfo.Add(reference);

            XElement signatureValue = new XElement("ds"+"SignatureValue");

            XElement x509Data = new XElement("ds"+"X509Certificate");
            x509Data.Add(new XElement("ds"+"X509Certificate"));
            XElement keyInfo = new XElement("ds"+"KeyInfo");
            keyInfo.Add(x509Data);

            Signature.Add(SignedInfo);
            Signature.Add(signatureValue);
            Signature.Add(keyInfo);
            Signature.Add();

            return Signature;
        }

        private XmlDocument BuildSignature(XmlDocument doc, String id, X509Certificate2 cert)
        {
            X509Certificate2 requestSigningCert = cert;

            doc.PreserveWhitespace = true;

            SignedXml signedXml = new SignedXml(doc);

            KeyInfo keyInfo = new KeyInfo();

            keyInfo.AddClause(new KeyInfoX509Data(requestSigningCert));

            signedXml.KeyInfo = keyInfo;

            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            RSACryptoServiceProvider rsaKey = (RSACryptoServiceProvider)requestSigningCert.PrivateKey;

            signedXml.SigningKey = rsaKey;

            Reference reference = new Reference();

            reference.Uri = "#" + id;

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();

            reference.AddTransform(env);

            XmlDsigExcC14NTransform c16n = new XmlDsigExcC14NTransform();

            //c16n.PropagatedNamespaces.Add("ds", "http://www.w3.org/2000/09/xmldsig#");

            // line above throws null pointer exception...
            reference.AddTransform(c16n);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Now we can compute the signature.
            signedXml.ComputeSignature();

            // Append signature to document
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            //PropagatePrefix(xmlDigitalSignature, "ds");

            //Above method adds ds: namespace to the XmlElement. I've tried with and without this. The Propagate method in the Transforms fail as well.


            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

            doc.DocumentElement.InsertAfter(doc.ImportNode(xmlDigitalSignature, true), doc.SelectSingleNode("//saml2:Issuer", nsManager));
           
            return doc;
        }

        public String BuildSimpleRequest()
        {
            var x = new XElement(Saml2Namespaces.Saml2P + LocalName);

            x.Add(base.ToXNodes());
            x.Add(new XAttribute("ProtocolBinding", HttpRedirectUri));
            x.Add(new XAttribute("AssertionConsumerServiceURL", AssertionConsumerServiceUrl));
            
            return x.ToString();
        }
    }
}