using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using NZ.CrmSvcUtil.Strategy.Edm.CrmTypes;
using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm
{
    public class CodeGenerationService : CodeGenerationServiceBase
    {
        public string DocumentName { get; set; }

        protected bool _excludeSystemAttribs { get; set; }

        protected Dictionary<AttributeTypeCode, string> _propertyTypeNameMap = new Dictionary<AttributeTypeCode, string>();

        private static Dictionary<AttributeTypeCode, string> _primitiveTypes = new Dictionary<AttributeTypeCode, string>()
        {
            { AttributeTypeCode.BigInt, "Int64" },
            { AttributeTypeCode.Integer, "Int16" },
            { AttributeTypeCode.Boolean, "Boolean" },
            { AttributeTypeCode.DateTime, "DateTime" },
            { AttributeTypeCode.Decimal, "Decimal" },
            { AttributeTypeCode.Double, "Double" },
            { AttributeTypeCode.String, "String" },
            { AttributeTypeCode.Uniqueidentifier, "Guid" },
        };

        private static Dictionary<AttributeTypeCode, Type> _complexTypes = new Dictionary<AttributeTypeCode, Type>()
        {
            { AttributeTypeCode.CalendarRules, typeof(CrmCalendarRulesType) },
            { AttributeTypeCode.Customer, typeof(CrmCustomerType) },
            { AttributeTypeCode.EntityName, typeof(CrmEntityNameType) },
            { AttributeTypeCode.Lookup, typeof(CrmLookupType) },
            { AttributeTypeCode.ManagedProperty, typeof(CrmManagedPropertyType) },
            { AttributeTypeCode.Memo, typeof(CrmMemoType) },
            { AttributeTypeCode.Money, typeof(CrmMoneyType) },
            { AttributeTypeCode.Owner, typeof(CrmOwnerType) },
            { AttributeTypeCode.PartyList, typeof(CrmPartyListType) },
            { AttributeTypeCode.Picklist, typeof(CrmPicklistType) },
            { AttributeTypeCode.State, typeof(CrmStateType) },
            { AttributeTypeCode.Status, typeof(CrmStatusType) },
            //{ AttributeTypeCode.Uniqueidentifier, typeof(CrmUniqueIdentifierType) },
            { AttributeTypeCode.Virtual, typeof(CrmVirtualType) },
        };

        public CodeGenerationService() : this(false)
        {
        }

        public CodeGenerationService(bool exclSystemAttributes = false)
        {
            _excludeSystemAttribs = exclSystemAttributes;
            DocumentName = $"CrmModel";
        }

        protected void DefinePrimitiveTypes(EdmDocument doc)
        {
            foreach (var entry in _primitiveTypes)
            {
                _propertyTypeNameMap.Add(entry.Key, entry.Value);
            }
        }

        protected void DefineComplexTypes(EdmDocument doc, string modelName)
        {
            foreach (var entry in _complexTypes)
            {
                dynamic instance = Convert.ChangeType(
                    Activator.CreateInstance(entry.Value, new Object[] { doc }), 
                    entry.Value
                );
                _propertyTypeNameMap.Add(entry.Key, $"{modelName}.{instance.Name}");
            }
        }
        
        #region Overrides of CodeGenerationServiceBase

        /// <inheritdoc />
        public override void Write(IOrganizationMetadata organizationMetadata, string language, string outputFile, string targetNamespace,
            IServiceProvider services)
        {
            var entityTable = new Dictionary<EntityMetadata, EdmEntity>();
            var attributeTable = new Dictionary<Attribute, EdmEntityProperty>();
            var uniqueRelTable = new HashSet<string>();

            var intersectEntities = organizationMetadata.Entities.Where(e => e.IsIntersect.HasValue ? e.IsIntersect.Value : false).ToArray();
            var nonIntersectEntities = organizationMetadata.Entities.Where(e => e.IsIntersect.HasValue ? !e.IsIntersect.Value : false).ToArray();

            var modelName = Regex.Replace(DocumentName ?? "UnnamedCrmModel", @"^\w|_\w", match => match.Value.Replace("_", "").ToUpper());
            var edmDoc = new EdmDocument(modelName);

            DefinePrimitiveTypes(edmDoc);
            DefineComplexTypes(edmDoc, modelName);

            // First create all CRM entities/attributes as corresponding EDM objects
            foreach (EntityMetadata entity in nonIntersectEntities.OrderBy(e => e.LogicalName))
            {
                var entityName = Regex.Replace(entity.LogicalName, @"\s+", String.Empty);
                var entityCollectionName = Regex.Replace(entity.LogicalCollectionName ?? (entityName + "Set"), @"\s+", String.Empty);
                var edmEntity = edmDoc.CreateEntity(entityName, entityCollectionName);
                entityTable[entity] = edmEntity;

                foreach (AttributeMetadata attr in entity.Attributes.OrderBy(attr => attr.LogicalName))
                {
                    if (_excludeSystemAttribs && !(attr.IsCustomAttribute ?? false)) continue;

                    var attrType = attr.AttributeType ?? AttributeTypeCode.String;
                    var attrName = attr.LogicalName;
                    var propType = _propertyTypeNameMap.ContainsKey(attrType) ? _propertyTypeNameMap[attrType] : "String";
                    var isComplexPropType = propType.Contains(".");
                    var isNullable = !isComplexPropType && !(attr.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired
                                      || attr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired);

                    if (attrName == entityName)
                    {
                        // CSDL does not allow a member to have the same name as its enclosing entity type
                        // So we can either rename it (append something) or skipp it. I decided to skip it
                        //continue;
                    }

                    var edmAttr = edmEntity.CreateProperty(attrName, propType, isNullable);

                    if (attr.Description.UserLocalizedLabel != null)
                        edmAttr.Description = attr.Description.UserLocalizedLabel.Label;

                    var isKeyProp = (attr.IsPrimaryId ?? false) || entity.Keys.Any(keyMd => keyMd.LogicalName == attrName);
                    if (isKeyProp)
                        edmEntity.DefineKeyProperty(edmAttr);
                }

                if (edmEntity.KeyProperties.Length == 0)
                {
                    // Add default key since it is required by CSDL schema
                    edmEntity.DefineKeyProperty(
                        edmEntity.CreateProperty("Id", AttributeTypeCode.Uniqueidentifier.ToString(), false));
                }
            }

            // Then create relationships
            foreach (EntityMetadata entity in nonIntersectEntities)
            {
                foreach (OneToManyRelationshipMetadata o2m in entity.OneToManyRelationships)
                {
                    var entity1 = organizationMetadata.Entities.First(e => e.LogicalName == o2m.ReferencedEntity);
                    var entity2 = organizationMetadata.Entities.First(e => e.LogicalName == o2m.ReferencingEntity);
                    if (intersectEntities.Any(e => e.LogicalName == o2m.SchemaName) || ((entity1.IsIntersect ?? false) || (entity2.IsIntersect ?? false)))
                    {
                        Console.WriteLine($"Skipping materialized 1:N relationship '{o2m.SchemaName}'");
                    }
                    else if (uniqueRelTable.Add(o2m.SchemaName))
                    {
                        Console.WriteLine($"Created 1:N assocation");
                        edmDoc.CreateAssociation(o2m.SchemaName,
                            new EdmAssociationEnd(entityTable[entity1], o2m.ReferencedEntityNavigationPropertyName, EdmMultiplicity.One),
                            new EdmAssociationEnd(entityTable[entity2], o2m.ReferencingEntityNavigationPropertyName, EdmMultiplicity.Many));
                    }
                }

                foreach (OneToManyRelationshipMetadata m2o in entity.ManyToOneRelationships)
                {
                    var entity1 = organizationMetadata.Entities.First(e => e.LogicalName == m2o.ReferencedEntity);
                    var entity2 = organizationMetadata.Entities.First(e => e.LogicalName == m2o.ReferencingEntity);
                    if (intersectEntities.Any(e => e.LogicalName == m2o.SchemaName) || ((entity1.IsIntersect ?? false) || (entity2.IsIntersect ?? false)))
                    {
                        Console.WriteLine($"Skipping materialized N:1 relationship '{m2o.SchemaName}'");
                    }
                    else if (uniqueRelTable.Add(m2o.SchemaName))
                    {
                        Console.WriteLine($"Created N:1 assocation");
                        edmDoc.CreateAssociation(m2o.SchemaName,
                            new EdmAssociationEnd(entityTable[entity1], m2o.ReferencedEntityNavigationPropertyName, EdmMultiplicity.Many),
                            new EdmAssociationEnd(entityTable[entity2], m2o.ReferencingEntityNavigationPropertyName, EdmMultiplicity.One));
                    }
                }

                foreach (ManyToManyRelationshipMetadata m2m in entity.ManyToManyRelationships)
                {
                    var entity1 = organizationMetadata.Entities.First(e => e.LogicalName == m2m.Entity1LogicalName);
                    var entity2 = organizationMetadata.Entities.First(e => e.LogicalName == m2m.Entity2LogicalName);
                    if (intersectEntities.Any(e => e.LogicalName == m2m.SchemaName) || ((entity1.IsIntersect ?? false) || (entity2.IsIntersect ?? false)))
                    {
                        Console.WriteLine($"Skipping materialized N:M relationship '{m2m.SchemaName}'");
                    }
                    else if (uniqueRelTable.Add(m2m.SchemaName))
                    {
                        Console.WriteLine($"Created N:M assocation");
                        edmDoc.CreateAssociation(m2m.SchemaName,
                            new EdmAssociationEnd(entityTable[entity1], m2m.Entity1NavigationPropertyName, EdmMultiplicity.Many),
                            new EdmAssociationEnd(entityTable[entity2], m2m.Entity2NavigationPropertyName, EdmMultiplicity.Many));
                    }
                }
            }
                  
            Console.WriteLine($"Writing {edmDoc.Entities.Length} entities");
            Console.WriteLine($"Writing {edmDoc.Associations.Length} assocations");

            using (var xmlWriter = XmlWriter.Create(outputFile, new XmlWriterSettings() {Indent = true}))
                edmDoc.WriteTo(xmlWriter);
        }

        #endregion


    }


}