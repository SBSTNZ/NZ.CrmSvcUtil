using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public class EdmDocument : XmlDocument
    {
        public string ModelName { get; private set; }

        public XmlNamespaceManager NsManager { get; private set; }
        
        public XmlElement ConceptualEntityContainer { get; private set; }

        public XmlElement ConceptualSchema { get; private set; }

        private int _uniqueRoleCounter = 0;

        private HashSet<string> _uniqueSymbolNameTable = new HashSet<string>();

        private HashSet<EdmEntity> _entities = new HashSet<EdmEntity>();
        /// <summary>
        /// Access list of entities contained in current document
        /// </summary>
        public EdmEntity[] Entities => _entities.ToArray();

        private HashSet<EdmAssociation> _associations = new HashSet<EdmAssociation>();
        /// <summary>
        /// List of associations contained in current document
        /// </summary>
        public EdmAssociation[] Associations => _associations.ToArray();


        public EdmDocument(string modelName, bool UseEmptyDefaultDiagram = true)
        {
            ModelName = modelName;
            NsManager = new XmlNamespaceManager(NameTable);
            RegisterNamespaces();

            InsertBefore(CreateXmlDeclaration("1.0", "UTF-8", null), DocumentElement);

            var edmxRoot = CreateElement("edmx", "Edmx", NsManager.LookupNamespace("edmx"));
            edmxRoot.SetAttribute("Version", "3.0");
            AppendChild(edmxRoot);

            edmxRoot.AppendChild(CreateComment("EF Runtime content"));
            var edmxRuntime = CreateElement("Runtime", NsManager.LookupNamespace("edmx"));
            edmxRoot.AppendChild(edmxRuntime);

            edmxRuntime.AppendChild(CreateComment("SSDL content"));
            var storageModels = CreateElement("StorageModels", NsManager.LookupNamespace("edmx"));
            edmxRuntime.AppendChild(storageModels);

            edmxRuntime.AppendChild(CreateComment("CSDL content"));
            var schema = CreateElement(String.Empty, "Schema", NsManager.LookupNamespace("ssdl"));
            schema.SetAttribute("Namespace", $"{ModelName}.Store");
            schema.SetAttribute("Alias", "Self");
            schema.SetAttribute("Provider", "System.Data.SqlClient");
            schema.SetAttribute("ProviderManifestToken", "2005");
            storageModels.AppendChild(schema); 
            var storageEntityContainer = CreateElement(String.Empty, "EntityContainer", NsManager.LookupNamespace("ssdl"));
            storageEntityContainer.SetAttribute("Name", $"{ModelName}TargetContainer");
            schema.AppendChild(storageEntityContainer);

            var edmxConceptModel = CreateElement("ConceptualModels", NsManager.LookupNamespace("edmx"));
            edmxRuntime.AppendChild(edmxConceptModel);

            var conceptSchema = ConceptualSchema = CreateElement(String.Empty, "Schema", NsManager.LookupNamespace("edm"));
            conceptSchema.SetAttribute("Namespace", ModelName);
            conceptSchema.SetAttribute("Alias", "Self");
            conceptSchema.SetAttribute("UseStrongSpatialTypes", NsManager.LookupNamespace("annotation"), "false");
            edmxConceptModel.AppendChild(conceptSchema);

            var conEntityContainer = ConceptualEntityContainer = CreateElement("EntityContainer", NsManager.LookupNamespace("edm"));
            conEntityContainer.SetAttribute("Name", $"{ModelName}Container");
            conEntityContainer.SetAttribute("LazyLoadingEnabled", NsManager.LookupNamespace("annotation"), "true");
            conceptSchema.AppendChild(conEntityContainer);

            edmxRuntime.AppendChild(CreateComment("C-S mapping content"));

            var edmxMappings = CreateElement("edmx", "Mappings", NsManager.LookupNamespace("edmx"));
            edmxRuntime.AppendChild(edmxMappings);

            var edmxMapping = CreateElement(String.Empty, "Mapping", NsManager.LookupNamespace("cs"));
            edmxMapping.SetAttribute("Space", "C-S");
            edmxMappings.AppendChild(edmxMapping);
            var modelAlias = CreateElement("Alias", NsManager.LookupNamespace("cs"));
            modelAlias.SetAttribute("Key", "Model");
            modelAlias.SetAttribute("Value", ModelName);
            edmxMapping.AppendChild(modelAlias);
            var targetAlias = CreateElement("Alias", NsManager.LookupNamespace("cs"));
            targetAlias.SetAttribute("Key", "Target");
            targetAlias.SetAttribute("Value", $"{ModelName}.Store");
            edmxMapping.AppendChild(targetAlias);
            var entityContainerMapping = CreateElement("EntityContainerMapping", NsManager.LookupNamespace("cs"));
            entityContainerMapping.SetAttribute("CdmEntityContainer", $"{ModelName}Container");
            entityContainerMapping.SetAttribute("StorageEntityContainer", $"{ModelName}TargetContainer");
            edmxMapping.AppendChild(entityContainerMapping);

            edmxRoot.AppendChild(CreateComment("EF Designer content (DO NOT EDIT MANUALLY BELOW HERE)"));
            var edmxDesigner = CreateElement("Designer", NsManager.LookupNamespace("edmx"));
            edmxRoot.AppendChild(edmxDesigner);

            var edmxConnection = CreateElement("Connection", NsManager.LookupNamespace("edmx"));
            edmxDesigner.AppendChild(edmxConnection);
            var designerInfoPropSet = CreateElement("DesignerInfoPropertySet", NsManager.LookupNamespace("edmx"));
            edmxConnection.AppendChild(designerInfoPropSet);
            var designerProp = CreateElement("DesignerProperty", NsManager.LookupNamespace("edmx"));
            designerProp.SetAttribute("Name", "MetadataArtifactProcessing");
            designerProp.SetAttribute("Value", "EmbedInOutputAssembly");
            designerInfoPropSet.AppendChild(designerProp);

            var edmxOpts = CreateElement("Options", NsManager.LookupNamespace("edmx"));
            edmxDesigner.AppendChild(edmxOpts);
            var optsDesignerInfoPropSet = CreateElement("DesignerInfoPropertySet", NsManager.LookupNamespace("edmx"));
            edmxOpts.AppendChild(optsDesignerInfoPropSet);

            var prop1 = CreateElement("DesignerProperty", NsManager.LookupNamespace("edmx"));
            prop1.SetAttribute("Name", "ValidateOnBuild");
            prop1.SetAttribute("Value", "true");
            optsDesignerInfoPropSet.AppendChild(prop1);
            var prop2 = CreateElement("DesignerProperty", NsManager.LookupNamespace("edmx"));
            prop2.SetAttribute("Name", "EnablePluralization");
            prop2.SetAttribute("Value", "false");
            optsDesignerInfoPropSet.AppendChild(prop2);
            var prop3 = CreateElement("DesignerProperty", NsManager.LookupNamespace("edmx"));
            prop3.SetAttribute("Name", "CodeGenerationStrategy");
            prop3.SetAttribute("Value", "None");
            optsDesignerInfoPropSet.AppendChild(prop3);

            edmxRoot.AppendChild(CreateComment("Diagram content (shape and connector positions)"));

            var edmxDiagrams = CreateElement("edmx", "Diagrams", NsManager.LookupNamespace("edmx"));
            edmxDesigner.AppendChild(edmxDiagrams);

            if (UseEmptyDefaultDiagram)
            {
                edmxDiagrams.AppendChild(CreateComment("Embed empty default diagram in order to avoid VS crash because of too many artifacts being rendered the first time EDMX is opened"));

                var defaultDiagr = CreateElement("Diagram", NsManager.LookupNamespace("edmx"));
                defaultDiagr.SetAttribute("DiagramId", Guid.NewGuid().ToString("N"));
                defaultDiagr.SetAttribute("Name", "Default");
                edmxDiagrams.AppendChild(defaultDiagr);
            }
        }

        public bool ContainsSymbol(string name)
        {
            return _uniqueSymbolNameTable.Contains(name);
        }

        public EdmEntity CreateEntity(string name, string collectionName)
        {
            if (_uniqueSymbolNameTable.Contains(name))
            {
                throw new EdmConstraintViolation($"A symbol with name '{name}' is already defined in current EDM context. No duplicates permitted.");
            }

            if (_uniqueSymbolNameTable.Contains(collectionName))
            {
                throw new EdmConstraintViolation($"A symbol with name '{name}' is already defined in current EDM context. No duplicates permitted.");
            }

            _uniqueSymbolNameTable.Add(name);

            var edmURI = NsManager.LookupNamespace("edm");

            var newEntitySet = CreateElement("EntitySet", edmURI);
            newEntitySet.SetAttribute("Name", collectionName);
            newEntitySet.SetAttribute("EntityType", $"{ModelName}.{name}");
            ConceptualEntityContainer.AppendChild(newEntitySet);

            var newEntityType = CreateElement("EntityType", edmURI);
            newEntityType.SetAttribute("Name", name);
            ConceptualSchema.AppendChild(newEntityType);

            var newEntityKey = CreateElement("Key", edmURI);
            newEntityType.AppendChild(newEntityKey);

            Console.WriteLine($"Creating entity \"{name}\"");

            var newEntity = new EdmEntity(this, name, collectionName, newEntitySet, newEntityType);
            _entities.Add(newEntity);

            return newEntity;
        }

        public EdmAssociation CreateAssociation(string name, EdmAssociationEnd entity1, EdmAssociationEnd entity2)
        {
            if (_uniqueSymbolNameTable.Contains(name))
            {
                throw new EdmConstraintViolation($"A symbol with name '{name}' is already defined in current EDM context. No duplicates permitted.");
            }
            
            _uniqueSymbolNameTable.Add(name);

            Console.WriteLine($"Creating {entity1.MultiplicityString}:{entity2.MultiplicityString} association \"{name}\" ({entity1.Entity.Name}/{entity2.Entity.Name})");

            var newAssoc = new EdmAssociation(this, name, entity1, entity2);
            _associations.Add(newAssoc);

            return newAssoc;
        }


        public string MakeUniqueRoleName()
        {
            return "Role" + (_uniqueRoleCounter++).ToString();
        }

        protected void RegisterNamespaces()
        {
            NsManager.AddNamespace("edmx", "http://schemas.microsoft.com/ado/2009/11/edmx");
            NsManager.AddNamespace("ssdl", "http://schemas.microsoft.com/ado/2009/11/edm/ssdl");
            NsManager.AddNamespace("edm", "http://schemas.microsoft.com/ado/2009/11/edm");
            NsManager.AddNamespace("cg", "http://schemas.microsoft.com/ado/2006/04/codegeneration");
            NsManager.AddNamespace("annotation", "http://schemas.microsoft.com/ado/2009/02/edm/annotation");
            NsManager.AddNamespace("store", "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator");
            NsManager.AddNamespace("cs", "http://schemas.microsoft.com/ado/2009/11/mapping/cs");
        }
    }

    
}