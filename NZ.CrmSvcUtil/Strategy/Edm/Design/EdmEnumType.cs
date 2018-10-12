using System;
using System.Collections.Generic;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public class EdmEnumType : EdmComplexType
    {
        protected Dictionary<string, int?> Members = new Dictionary<string, int?>();

        public EdmEnumType(EdmDocument document, string name) : base(document, name)
        {
        }

        public bool AddMember(string name, int? value)
        {
            if (String.IsNullOrWhiteSpace(name)) 
                throw new NullReferenceException("No valid enum type name provided");

            if (Members.ContainsKey(name)) return false;

            var edmDoc = Doc;
            var xmlDoc = Doc;
            var edmURI = Doc.NsManager.LookupNamespace("edm");

            var memberEl = xmlDoc.CreateElement("Member", edmURI);
            memberEl.SetAttribute("Name", name);
            if (value.HasValue) memberEl.SetAttribute("Value", value.ToString());
            TypeElement.AppendChild(memberEl);

            return true;
        }
    }
}