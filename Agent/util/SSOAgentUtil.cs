using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;

namespace Agent.util
{
    public class SSOAgentUtil
    {
        private SSOAgentUtil() { }

        public static string EmbedSignatureIntoAuthnRequest(XElement xElement,string id, X509Certificate2 cert)
        {
            //IF SIGNING IS ENABLED, THEN...

            //x.Add(createSignatureXElement(cert));
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            BuildSignature(XMLUtil.XElementToXMLDocument(xElement), id, cert).WriteTo(xw);
            return sw.ToString();    
        }

        public static XmlDocument BuildSignature(XmlDocument doc, String id, X509Certificate2 cert)
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

            Reference reference = new Reference
            {
                Uri = "#" + id
            };

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
    }
}
