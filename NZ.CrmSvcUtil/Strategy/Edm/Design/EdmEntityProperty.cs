using System;
using System.Xml;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public abstract class EdmEntityProperty
    {
        protected EdmEntity Entity;
        protected XmlElement PropertyElement;

        public string Name { get; private set; }

        public EdmComplexType Type { get; private set; }

        protected EdmDescription PropertyDescription;
        public string Summary
        {
            get { return (PropertyDescription == null) ? String.Empty : PropertyDescription.Summary; }
            set { if (PropertyDescription != null) PropertyDescription.Summary = value; }
        }

        public string Description
        {
            get { return (PropertyDescription == null) ? String.Empty : PropertyDescription.Description; }
            set { if (PropertyDescription != null) PropertyDescription.Description = value; }
        }

        public EdmEntityProperty(EdmEntity parentEntity, string name)
        {
            Entity = parentEntity;
            Name = name;
        }

        public EdmEntityProperty(EdmEntity parentEntity, EdmComplexType ct)
        {
            Entity = parentEntity;
            Name = ct.Name;
            Type = ct;
        }

    }

    
}