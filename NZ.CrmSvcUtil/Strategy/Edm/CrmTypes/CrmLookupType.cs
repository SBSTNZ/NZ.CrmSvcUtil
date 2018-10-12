using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmLookupType : EdmComplexType
    {
        public CrmLookupType(EdmDocument document)
            : base(document, "Lookup")
        {
        }
    }
}