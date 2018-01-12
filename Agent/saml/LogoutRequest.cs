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
        public XElement BuildRequest()
        {
            var x = new XElement(Saml2Namespaces.Saml2P + LocalName);

            x.Add(base.ToXNodes());

            // Set issuer.
            x.SetElementValue(Saml2Namespaces.Saml2 + "Issuer", "demo-sso-agent");
            // End of setting issuer.

            //nameID 
            Saml2Subject subject = (Saml2Subject)HttpContext.Current.Session["Saml2Subject"];

            XElement nameID = new XElement(Saml2Namespaces.Saml2 + "NameID", subject.NameId.Value);
            nameID.Add(new XAttribute("Format", subject.NameId.Format));
            x.Add(nameID);
            //end of nameid

            x.SetElementValue(Saml2Namespaces.Saml2P + "SessionIndex", HttpContext.Current.Session["SessionIndex"]);

            return x;
        }   
    }
}
