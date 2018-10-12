using System;
using System.Xml;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{

    public class EdmDescription
    {
        public enum InsertPosition
        {
            First,
            Last
        }

        protected InsertPosition InsertAt;
        protected bool IsAppended = false;
        protected XmlElement TargetElement;
        protected XmlElement DocumentationElement;
        protected XmlElement SummaryElement;
        protected XmlElement DescriptionElement;

        public string Summary
        {
            get { return (IsAppended) ? SummaryElement.InnerText : String.Empty; }
            set
            {
                if (!IsAppended)
                {
                    if (InsertAt == InsertPosition.First)
                    {
                        TargetElement.PrependChild(DocumentationElement);
                    }
                    else
                    {
                        TargetElement.AppendChild(DocumentationElement);
                    }
                    IsAppended = true;
                }
                SummaryElement.InnerText = value;
            }
        }

        public string Description
        {
            get { return (IsAppended) ? DescriptionElement.InnerText : String.Empty; }
            set
            {
                if (!IsAppended)
                {
                    if (InsertAt == InsertPosition.First)
                    {
                        TargetElement.PrependChild(DocumentationElement);
                    }
                    else
                    {
                        TargetElement.AppendChild(DocumentationElement);
                    }
                    IsAppended = true;
                }
                DescriptionElement.InnerText = value;
            }
        }

        public EdmDescription(EdmDocument edmDoc, XmlElement target, InsertPosition pos = InsertPosition.Last)
        {
            if (target != null)
            {
                var nsUri = edmDoc.NsManager.LookupNamespace("edm");
                var xmlDoc = target.OwnerDocument;

                InsertAt = pos;
                TargetElement = target;
                DocumentationElement = xmlDoc.CreateElement("Documentation", nsUri);

                SummaryElement = xmlDoc.CreateElement("Summary", nsUri);
                DescriptionElement = xmlDoc.CreateElement("LongDescription", nsUri);
                DocumentationElement.AppendChild(SummaryElement);
                DocumentationElement.AppendChild(DescriptionElement);
            }
            else
            {
                throw new NullReferenceException("Target element not set to an instance");
            }

        }
    }

    
}