using System;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public class EdmNavigationProperty : EdmEntityProperty
    {
        public EdmNavigationProperty(EdmEntity parentEntity, string name, EdmAssociation assoc) : base(parentEntity, name)
        {
            if (assoc.End1.Entity != parentEntity && assoc.End2.Entity != parentEntity)
            {
                throw new Exception("Navigational Property can only be created on entity that is part of given association");
            }

            var xmlDoc = Entity.EntityTypeElement.OwnerDocument;
            var edmDoc = Entity.EdmDocument;

            PropertyElement = xmlDoc.CreateElement("NavigationProperty", edmDoc.NsManager.LookupNamespace("edm"));
            PropertyDescription = new EdmDescription(edmDoc, PropertyElement);

            PropertyElement.SetAttribute("Name", Name);
            PropertyElement.SetAttribute("Relationship", $"{edmDoc.ModelName}.{assoc.Name}");
            PropertyElement.SetAttribute("FromRole", parentEntity.Name);
            PropertyElement.SetAttribute("ToRole", (assoc.End1.Entity == parentEntity ? assoc.End2 : assoc.End1).Entity.Name);

            Entity.EntityTypeElement.AppendChild(PropertyElement);
        }
    }

    
}