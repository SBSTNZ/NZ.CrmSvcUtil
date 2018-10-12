using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;

namespace NZ.CrmSvcUtil.Strategy.Text
{
    public class CodeGenerationService : CodeGenerationServiceBase
    {

        #region Generation logic

        private static readonly string LnBr = Environment.NewLine;
        
        private static string BuildMessages(SdkMessages sdkMessages, IServiceProvider serviceProvider)
        {
            var output = new StringBuilder();
            output.AppendLine("---------------------------------------------");
            output.AppendLine("Messages:" + LnBr + LnBr);

            var filterService = (ICodeWriterFilterService)serviceProvider.GetService(typeof(ICodeWriterFilterService));
            foreach (SdkMessage value in sdkMessages.MessageCollection.Values)
            {
                output.AppendLine($"  -> {value.Name}");
            }

            return output.ToString();
        }

        private static string BuildEntities(IEnumerable<EntityMetadata> entities, IServiceProvider services)
        {
            var output = new StringBuilder();
            output.AppendLine("---------------------------------------------");
            output.Append("Entities:" + LnBr + LnBr);

            output.Append(String.Join(LnBr,
                entities.SelectMany(e => new string[] { $"{e.LogicalName}" }
                    .Concat(e.Attributes.Select(a => $"  -> {a.LogicalName} ({a.AttributeTypeName.Value})"))
                )
            ));

            return output.ToString();
        }

        private static string BuildOptionSets(IEnumerable<OptionSetMetadataBase> optionSets, IServiceProvider services)
        {
            var output = new StringBuilder();
            output.AppendLine("---------------------------------------------");
            output.Append("OptionSets:" + LnBr + LnBr);

            output.Append(String.Join(LnBr,
                optionSets.SelectMany(os => new string[] { $"-> {os.Name} ({os.OptionSetType.Value})" }
                    .Concat((os as OptionSetMetadata).Options.Select(o => $"     -> {o.Label.UserLocalizedLabel.Label}"))
                )
            ));

            return output.ToString();
        } 

        #endregion

        /// <inheritdoc />
        public override void Write(IOrganizationMetadata metadata, string language, string outputFile, string targetNamespace, IServiceProvider services)
        {
            var sb = new StringBuilder();
            
            // Print some stats
            sb.AppendLine($"Code generated on: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}").AppendLine();
            sb.AppendLine($"Metadata summary:");
            sb.AppendLine($"  Entities: {metadata.Entities.Length}");
            sb.AppendLine($"  OptionSets: {metadata.OptionSets.Length}");
            sb.AppendLine($"  SdkMessages: {metadata.Messages.MessageCollection.Count}");
            sb.AppendLine();

            sb.Append(BuildEntities(metadata.Entities.OrderBy(e => e.LogicalName), services));
            sb.Append(BuildOptionSets(metadata.OptionSets.OrderBy(os => os.Name), services));
            sb.Append(BuildMessages(metadata.Messages, services));

            File.WriteAllText(outputFile, sb.ToString(), Encoding.UTF8);
        }
     
        
    }
}