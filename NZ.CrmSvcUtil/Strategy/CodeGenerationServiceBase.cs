using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;

namespace NZ.CrmSvcUtil.Strategy
{
    public abstract class CodeGenerationServiceBase : ICodeGenerationService
    {
        protected readonly ICodeGenerationService _defaultServiceImpl;
        protected readonly Dictionary<string, string> _cliParameters;

        /// <inheritdoc />
        public CodeGenerationServiceBase()
        {
        }

        /// <inheritdoc />
        public CodeGenerationServiceBase(ICodeGenerationService defaultServiceImpl, Dictionary<string, string> cliParameters)
        {
            _defaultServiceImpl = defaultServiceImpl;
            _cliParameters = cliParameters;
        }

        #region Implementation of ICodeGenerationService

        /// <inheritdoc />
        public abstract void Write(IOrganizationMetadata organizationMetadata, string language, string outputFile, string targetNamespace, IServiceProvider services);
        
        /// <inheritdoc />
        public virtual CodeGenerationType GetTypeForOptionSet(EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata,
            IServiceProvider services)
        {
            return CodeGenerationType.Enum;
        }

        /// <inheritdoc />
        public virtual CodeGenerationType GetTypeForOption(OptionSetMetadataBase optionSetMetadata, OptionMetadata optionMetadata,
            IServiceProvider services)
        {
            return CodeGenerationType.Field;
        }

        /// <inheritdoc />
        public virtual CodeGenerationType GetTypeForEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            return CodeGenerationType.Class;
        }

        /// <inheritdoc />
        public virtual CodeGenerationType GetTypeForAttribute(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata,
            IServiceProvider services)
        {
            return CodeGenerationType.Property;
        }

        /// <inheritdoc />
        public virtual CodeGenerationType GetTypeForMessagePair(SdkMessagePair messagePair, IServiceProvider services)
        {
            return CodeGenerationType.Class;
        }

        /// <inheritdoc />
        public virtual CodeGenerationType GetTypeForRequestField(SdkMessageRequest request, SdkMessageRequestField requestField,
            IServiceProvider services)
        {
            return CodeGenerationType.Property;
        }

        /// <inheritdoc />
        public virtual CodeGenerationType GetTypeForResponseField(SdkMessageResponse response, SdkMessageResponseField responseField,
            IServiceProvider services)
        {
            return CodeGenerationType.Property;
        }

        #endregion
    }
}