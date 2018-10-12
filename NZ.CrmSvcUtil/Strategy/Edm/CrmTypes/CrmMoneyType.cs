using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmMoneyType : EdmComplexType
    {
        public CrmMoneyType(EdmDocument document)
            : base(document, "Money")
        {
        }
    }
}