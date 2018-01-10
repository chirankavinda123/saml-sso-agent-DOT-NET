using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Agent.util
{
    class XMLUtil
    {
        public static XmlDocument XElementToXMLDocument(XElement x)
        {
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
    }
}
