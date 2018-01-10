using Agent.util;
using Kentor.AuthServices;
using Kentor.AuthServices.Saml2P;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Agent.saml
{
    class LogoutRequest : Saml2LogoutRequest
    {
        public string BuildRedirectLogoutRequest()
        {
            var x = new XElement(Saml2Namespaces.Saml2P + LocalName);

            x.Add(base.ToXNodes());

            //issuer
            x.SetElementValue(Saml2Namespaces.Saml2 + "Issuer", "demo-sso-agent");
            //end of issuer

            //nameID
            XElement nameID = new XElement(Saml2Namespaces.Saml2 + "NameID","admin");
            nameID.Add(new XAttribute("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"));
            x.Add(nameID);
            //end of nameid 

            x.SetElementValue(Saml2Namespaces.Saml2P + "SessionIndex", HttpContext.Current.Application["SessionIndex"]);

            return x.ToString();
        }


        public string BuildPOSTLogoutRequest(X509Certificate2 cert)
        {
            var x = new XElement(Saml2Namespaces.Saml2P + LocalName);

            x.Add(base.ToXNodes());

            //issuer
            x.SetElementValue(Saml2Namespaces.Saml2 + "Issuer", "demo-sso-agent");
            //end of issuer

            //nameID 
            Saml2Subject subject = (Saml2Subject)HttpContext.Current.Application["Saml2Subject"];

            XElement nameID = new XElement(Saml2Namespaces.Saml2 + "NameID",subject.NameId.Value);
            nameID.Add(new XAttribute("Format",subject.NameId.Format));
            x.Add(nameID);
            //end of nameid 

            x.SetElementValue(Saml2Namespaces.Saml2P + "SessionIndex", HttpContext.Current.Application["SessionIndex"]);

            //IF SIGNING IS ENABLED, THEN...

            //generate signature xElement
            //x.Add(createSignatureXElement(cert));
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            BuildSignature(XMLUtil.XElementToXMLDocument(x), base.Id.Value, cert).WriteTo(xw);
            return sw.ToString();
            //end of Signature xElement   
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


    }
}
