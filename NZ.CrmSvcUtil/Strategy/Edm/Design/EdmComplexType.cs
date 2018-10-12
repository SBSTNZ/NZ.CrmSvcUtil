using System;
using System.Xml;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public class EdmComplexType
    {
        protected EdmDocument Doc;

        public XmlElement TypeElement { get; private set; }

        public string Name { get; private set; }

        public EdmComplexType(EdmDocument document, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new NullReferenceException("Complex Type name must not be null/empty");

            Name = name;

            var edmDoc = Doc = document;
            var xmlDoc = document;

            var edmURI = document.NsManager.LookupNamespace("edm");

            var cte = TypeElement = xmlDoc.CreateElement("ComplexType", edmURI);
            cte.SetAttribute("Name", name);
            edmDoc.ConceptualSchema.AppendChild(cte);
        }
    }
}