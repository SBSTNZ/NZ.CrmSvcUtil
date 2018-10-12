using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmManagedPropertyType : EdmComplexType
    {
        public CrmManagedPropertyType(EdmDocument document)
            : base(document, "ManagedProperty")
        {
        }
    }
}