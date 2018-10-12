using System;
using System.Collections.Generic;
using System.Xml;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public class EdmEntity
    {
        public string Name { get; private set; }

        public string CollectionName { get; private set; }

        public EdmDocument EdmDocument { get; private set; }

        protected EdmDescription EntityDescription;

        public string Summary
        {
            get { return (EntityDescription == null) ? String.Empty : EntityDescription.Summary; }
            set { if (EntityDescription != null) EntityDescription.Summary = value; }
        }

        public string Description
        {
            get { return (EntityDescription == null) ? String.Empty : EntityDescription.Description; }
            set { if (EntityDescription != null) EntityDescription.Description = value; }
        }


        protected List<EdmEntityProperty> _Properties = new List<EdmEntityProperty>();
        public EdmEntityProperty[] Properties { get { return _Properties.ToArray(); } }

        protected List<EdmEntityProperty> _KeyProperties = new List<EdmEntityProperty>();
        public EdmEntityProperty[] KeyProperties
        {
            get { return _KeyProperties.ToArray(); }
        }

        public XmlElement EntitySetElement { get; private set; }

        public XmlElement EntityTypeElement { get; private set; }

        public EdmEntity(EdmDocument parent, string name, string collectionName, XmlElement entitySet, XmlElement entityType)
        {
            Name = name;
            CollectionName = collectionName;

            EdmDocument = parent;
            EntitySetElement = entitySet;
            EntityTypeElement = entityType;

            EntityDescription = new EdmDescription(EdmDocument, EntityTypeElement, EdmDescription.InsertPosition.First);
        }

        public void DefineKeyProperty(EdmEntityProperty property)
        {
            if (_KeyProperties.Contains(property)) return;

            var propRef = EdmDocument.CreateElement("PropertyRef", EdmDocument.NsManager.LookupNamespace("edm"));
            propRef.SetAttribute("Name", property.Name);
            foreach (XmlNode childNode in EntityTypeElement.ChildNodes)
            {
                if (childNode.Name == "Key")
                    childNode.AppendChild(propRef);
            }
            _KeyProperties.Add(property);
        }

        public EdmEntityProperty CreateProperty(string name, string type, bool isNullable)
        {
            var newProp = new EdmScalarProperty(this, name, isNullable, type);
            _Properties.Add(newProp);
            return newProp;
        }

        public EdmNavigationProperty CreateNavigationProperty(string name, EdmAssociation assoc)
        {
            return new EdmNavigationProperty(this, name, assoc);
        }
    }

}