using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmPicklistType : EdmComplexType
    {
        public CrmPicklistType(EdmDocument document)
            : base(document, "Picklist")
        {
        }
    }
}