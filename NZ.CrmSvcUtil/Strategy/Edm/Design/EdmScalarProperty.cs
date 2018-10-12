namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public class EdmScalarProperty : EdmEntityProperty
    {
        public EdmScalarProperty(EdmEntity parentEntity, string name, bool isNullable, string type) : base(parentEntity, name)
        {
            var xmlDoc = Entity.EntityTypeElement.OwnerDocument;
            var edmDoc = Entity.EdmDocument;

            PropertyElement = xmlDoc.CreateElement("Property", edmDoc.NsManager.LookupNamespace("edm"));
            PropertyDescription = new EdmDescription(edmDoc, PropertyElement);

            PropertyElement.SetAttribute("Name", Name);
            PropertyElement.SetAttribute("Type", type);
            PropertyElement.SetAttribute("Nullable", isNullable ? "true" : "false");

            Entity.EntityTypeElement.AppendChild(PropertyElement);
        }
    }

    
}