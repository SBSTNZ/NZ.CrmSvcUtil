using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmVirtualType : EdmComplexType
    {
        public CrmVirtualType(EdmDocument document)
            : base(document, "Virtual")
        {
        }
    }
}