using System;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    

    public class EdmAssociation
    {
        public EdmAssociationEnd End1 { get; private set; }
        public EdmAssociationEnd End2 { get; private set; }

        public string Name { get; private set; }

        protected EdmDescription AssocDescription;
        public string Summary
        {
            get { return (AssocDescription == null) ? String.Empty : AssocDescription.Summary; }
            set { if (AssocDescription != null) AssocDescription.Summary = value; }
        }

        public string Description
        {
            get { return (AssocDescription == null) ? String.Empty : AssocDescription.Description; }
            set { if (AssocDescription != null) AssocDescription.Description = value; }
        }

        public EdmAssociation(EdmDocument document, string name, EdmAssociationEnd end1, EdmAssociationEnd end2)
        {
            End1 = end1;
            End2 = end2;
            Name = name;

            var edmDoc = document;
            var xmlDoc = document;
            var edmNsUri = xmlDoc.NsManager.LookupNamespace("edm");

            var roleNameEnd1 = end1.Role;
            var roleNameEnd2 = end2.Role;
            if (end1.Role == end2.Role)
            {
                // CSDL does not allow to have two assoc ends the same role
                Console.Error.WriteLine("Two association set ends which belong to the same association set must have different roles");
                roleNameEnd2 += "2";
            }

            var assocSet = xmlDoc.CreateElement("AssociationSet", edmNsUri);
            assocSet.SetAttribute("Name", Name);
            assocSet.SetAttribute("Association", $"{edmDoc.ModelName}.{Name}");
            edmDoc.ConceptualEntityContainer.AppendChild(assocSet);

            var assocSetEnd1 = xmlDoc.CreateElement("edm:End", edmNsUri);
            assocSetEnd1.SetAttribute("Role", roleNameEnd1);
            assocSetEnd1.SetAttribute("EntitySet", end1.Entity.CollectionName);
            assocSet.AppendChild(assocSetEnd1);

            var assocSetEnd2 = xmlDoc.CreateElement("edm:End", edmNsUri);
            assocSetEnd2.SetAttribute("Role", roleNameEnd2);
            assocSetEnd2.SetAttribute("EntitySet", end2.Entity.CollectionName);
            assocSet.AppendChild(assocSetEnd2);

            var assoc = xmlDoc.CreateElement("edm:Association", edmNsUri);
            assoc.SetAttribute("Name", name);
            edmDoc.ConceptualSchema.AppendChild(assoc);

            var assocEnd1 = xmlDoc.CreateElement("edm:End", edmNsUri);
            assocEnd1.SetAttribute("Type", $"{edmDoc.ModelName}.{end1.Entity.Name}");
            assocEnd1.SetAttribute("Role", roleNameEnd1);
            assocEnd1.SetAttribute("Multiplicity", end1.MultiplicityString);
            assoc.AppendChild(assocEnd1);

            var assocEnd2 = xmlDoc.CreateElement("edm:End", edmNsUri);
            assocEnd2.SetAttribute("Type", $"{edmDoc.ModelName}.{end2.Entity.Name}");
            assocEnd2.SetAttribute("Role", roleNameEnd2);
            assocEnd2.SetAttribute("Multiplicity", end2.MultiplicityString);
            assoc.AppendChild(assocEnd2);

            AssocDescription = new EdmDescription(edmDoc, assoc, EdmDescription.InsertPosition.First);
        }
    }
}