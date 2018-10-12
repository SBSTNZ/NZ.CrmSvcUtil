using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmCustomerType : EdmComplexType
    {
        public CrmCustomerType(EdmDocument document)
            : base(document, "Customer")
        {
        }
    }
}