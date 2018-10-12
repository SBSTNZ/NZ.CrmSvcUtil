using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmOwnerType : EdmComplexType
    {
        public CrmOwnerType(EdmDocument document)
            : base(document, "Owner")
        {
        }
    }
}