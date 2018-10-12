using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;

namespace NZ.CrmSvcUtil.Strategy.JavaScript
{
    public class CodeGenerationService : CodeGenerationServiceBase
    {
        private class OutputFileData
        {
            public string Filename { get; set; }

            public string Content { get; set; }
        }

        #region Code generation logic

        private static readonly string LnBr = Environment.NewLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sdkMessages"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static OutputFileData[] BuildMessages(SdkMessages sdkMessages, IServiceProvider serviceProvider)
        {
            var result = new List<OutputFileData>();

            var filterService = (ICodeWriterFilterService)serviceProvider.GetService(typeof(ICodeWriterFilterService));
            foreach (var message in sdkMessages.MessageCollection.Values.Where(m => !m.IsPrivate))
            {
                foreach (var messagePair in message.SdkMessagePairs.Values)
                {
                    var messageReq = messagePair.Request;
                    var messageResp = messagePair.Response;
                    var messagePairName = ((INamingService)serviceProvider.GetService(typeof(INamingService))).GetNameForMessagePair(messagePair, serviceProvider);

                    // Request
                    if (messageReq != null)
                    {
                        var src = new StringBuilder();
                        var requestName = String.Format("{0}Request", messagePairName);
                        src.AppendLine($"/** {requestName}  */");
                        src.AppendLine($"class {requestName} {{");
                        src.AppendLine("    constructor(data) {");
                        src.AppendLine("        if (typeof data !== 'object') throw new Error('No entity data object provided');");
                        src.AppendLine("        this.data = data;");
                        src.AppendLine("    }");

                        if (messageReq.RequestFields != null)      // Request fields are optional
                        {
                            foreach (var field in messageReq.RequestFields.Values.OrderBy(f => f.Index))
                            {
                                src.AppendLine($"    /**");
                                src.AppendLine($"     * {field.Name}");
                                src.AppendLine($"     * @type {{}}");
                                if (field.IsOptional) src.AppendLine($"     * @optional");
                                src.AppendLine($"     */");
                                src.AppendLine($"    get {field.Name}() {{ return this.data[\"{field.Name}\"]; }}");
                                src.AppendLine($"    set {field.Name}(value) {{ this.data[\"{field.Name}\"] = value; }}");
                                src.AppendLine();
                            } 
                        }

                        src.AppendLine($"}}");
                        result.Add(new OutputFileData()
                        {
                            Filename = $"{requestName}.js",
                            Content = src.ToString()
                        }); 
                    }

                    // Response
                    if (messageResp != null)
                    {
                        var src = new StringBuilder();
                        var responseName = String.Format("{0}Response", messagePairName);
                        src.AppendLine($"/** {responseName}  */");
                        src.AppendLine($"class {responseName} {{");
                        src.AppendLine("    constructor(data) {");
                        src.AppendLine("        if (typeof data !== 'object') throw new Error('No entity data object provided');");
                        src.AppendLine("        this.data = data;");
                        src.AppendLine("    }");

                        if (messageResp.ResponseFields != null) // Response fields are optional
                        {
                            foreach (var field in messageResp.ResponseFields.Values.OrderBy(f => f.Index))
                            {
                                src.AppendLine($"    /**");
                                src.AppendLine($"     * {field.Name}");
                                src.AppendLine($"     * @type {{}}");
                                src.AppendLine($"     */");
                                src.AppendLine($"    get {field.Name}() {{ return this.data[\"{field.Name}\"]; }}");
                                src.AppendLine();
                            } 
                        }

                        src.AppendLine($"}}");
                        result.Add(new OutputFileData()
                        {
                            Filename = $"{responseName}.js",
                            Content = src.ToString()
                        }); 
                    }
                }      
            }
         
            return result.ToArray();
        }

        private static OutputFileData[] BuildEntities(IEnumerable<EntityMetadata> entities, IServiceProvider services)
        {
            var result = new List<OutputFileData>();

            foreach (var entity in entities)
            {
                var src = new StringBuilder();
                src.AppendLine($"/** {entity.DisplayName.UserLocalizedLabel?.Label}: {entity.Description.UserLocalizedLabel?.Label}  */");
                src.AppendLine($"class {entity.SchemaName} {{");
                src.AppendLine("    constructor(data) {");
                src.AppendLine("        if (typeof data !== 'object') throw new Error('No entity data object provided');");
                src.AppendLine("        this.data = data;");
                src.AppendLine("    }");

                foreach (var attrib in entity.Attributes.OrderBy(a => a.LogicalName))
                {
                    src.AppendLine($"    /**");
                    src.AppendLine($"     * {attrib.DisplayName.UserLocalizedLabel?.Label}");
                    src.AppendLine($"     * @type {{{GetJsdocTypeForAttribute(attrib)}}}");
                    src.AppendLine($"     * @description {attrib.Description.UserLocalizedLabel?.Label}");
                    src.AppendLine($"     */");
                    src.AppendLine($"    get {attrib.SchemaName}() {{ return this.data[\"{attrib.LogicalName}\"]; }}");
                    src.AppendLine($"    set {attrib.SchemaName}(value) {{ this.data[\"{attrib.LogicalName}\"] = value; }}");
                    src.AppendLine();
                }

                src.AppendLine($"}}");
                result.Add(new OutputFileData()
                {
                    Filename = $"{entity.SchemaName}.js",
                    Content =  src.ToString()
                });
            }

            return result.ToArray();
        }

        

        private static string BuildOptionSets(OptionSetMetadataBase[] optionSets, IServiceProvider services)
        {
            var output = new StringBuilder();
            output.AppendLine("---------------------------------------------");
            output.Append(LnBr + LnBr + "OptionSets:" + LnBr + LnBr);

            output.Append(String.Join(LnBr,
                optionSets.SelectMany(os => new string[] { $"-> {os.Name} ({os.OptionSetType.Value})" }
                    .Concat((os as OptionSetMetadata).Options.Select(o => $"     -> {o.Label.UserLocalizedLabel.Label}"))
                )
            ));

            return output.ToString();
        }

        private static string GetJsdocTypeForAttribute(AttributeMetadata attrib)
        {
            string jsdocType;

            switch (attrib.AttributeType)
            {
                case AttributeTypeCode.BigInt:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Decimal:
                    jsdocType = "number";
                    break;

                case AttributeTypeCode.Boolean:
                    jsdocType = "boolean";
                    break;

                case AttributeTypeCode.DateTime:
                    jsdocType = "Date";
                    break;

                case AttributeTypeCode.EntityName:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.Uniqueidentifier:
                case AttributeTypeCode.Virtual:
                    jsdocType = "string";
                    break;

                default:
                    jsdocType = attrib.AttributeTypeName.Value;
                    break;
            }

            return jsdocType;
        }

        private static string WrapIntoNamespace(string targetNamespace, string code)
        {
            var sb = new StringBuilder();

            // Build namespace
            var hasNamespace = !String.IsNullOrWhiteSpace(targetNamespace);
            if (hasNamespace)
            {
                var nsParts = targetNamespace.Split('.');
                sb.AppendLine($"var {nsParts[0]} =  {{}};");
                for (var i = 1; i<nsParts.Length; i++)
                {
                    for (int j = 0, nPrevParts = i; j<nPrevParts; j++)
                        sb.Append($"{nsParts[j]}");
                    sb.AppendLine($".{nsParts[i]} =  {{}};");
                }
            }

            sb.AppendLine().Append(code).AppendLine();

            return sb.ToString();
        }

        #endregion

        /// <inheritdoc />
        public override void Write(IOrganizationMetadata metadata, string language, string outputFile, string targetNamespace, IServiceProvider services)
        {
            language = String.IsNullOrEmpty(language) ? String.Empty : language;
            targetNamespace = String.IsNullOrEmpty(targetNamespace) ? String.Empty : targetNamespace;

            if (String.IsNullOrEmpty(outputFile))
            {
                throw new ArgumentNullException(nameof(outputFile), "Output file path must be provided");
            }

            var writeToSeparateFiles = true; //_cliParameters.ContainsKey("separatefiles");
            if (Regex.IsMatch(targetNamespace, @"[a-zA-Z0-9\.]+"))
            {
                throw new ArgumentOutOfRangeException("Namespace must only consist of characters, digits and dots");
            }
            
            // Create namespace directories if separate file flag is se
            var outputDir = Path.GetDirectoryName(Path.GetFullPath(outputFile));
            var baseNsDirPath = Path.Combine(outputDir, "Generated Code", targetNamespace.Replace(".", Path.DirectorySeparatorChar.ToString()));
            var classNsDirPath = Path.Combine(baseNsDirPath, "Entities");
            var optionsetNsDirPath = Path.Combine(baseNsDirPath, "OptionSets");
            var messageNsDirPath = Path.Combine(baseNsDirPath, "Messages");
            if (writeToSeparateFiles)
            {
                Directory.CreateDirectory(classNsDirPath);
                Directory.CreateDirectory(optionsetNsDirPath);
                Directory.CreateDirectory(messageNsDirPath);
            }

            // Build and output JS code
            
            var entityFiles = BuildEntities(metadata.Entities.OrderBy(e => e.LogicalName), services);
            var optionsetsSrc = BuildOptionSets(metadata.OptionSets, services);
            var sdkMessageFiles = BuildMessages(metadata.Messages, services);

            if (writeToSeparateFiles)
            {
                foreach (var file in entityFiles)
                {
                    var filePath = Path.Combine(classNsDirPath, file.Filename);
                    File.WriteAllText(filePath, file.Content, Encoding.UTF8);
                }

                foreach (var file in sdkMessageFiles)
                {
                    var filePath = Path.Combine(messageNsDirPath, file.Filename);
                    File.WriteAllText(filePath, file.Content, Encoding.UTF8);
                }

                File.WriteAllText(Path.Combine(baseNsDirPath, "OptionSets.js"), optionsetsSrc, Encoding.UTF8);
            }
            else
            {
                var sb = new StringBuilder();

                // Print some stats
                sb.AppendLine($"/* Code generated on: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}").AppendLine();
                sb.AppendLine($" * Metadata summary:");
                sb.AppendLine($" *   Entities: {metadata.Entities.Length}");
                sb.AppendLine($" *   OptionSets: {metadata.OptionSets.Length}");
                sb.AppendLine($" *   SdkMessages: {metadata.Messages.MessageCollection.Count}");
                sb.AppendLine($"*/");
                sb.AppendLine();

                sb.AppendLine(String.Join(Environment.NewLine, entityFiles.Select(t => t.Content)));
                sb.AppendLine(optionsetsSrc);
                sb.AppendLine(String.Join(Environment.NewLine, sdkMessageFiles.Select(t => t.Content)));

                File.WriteAllText(outputFile, sb.ToString(), Encoding.UTF8);
            }

        }
    }
}