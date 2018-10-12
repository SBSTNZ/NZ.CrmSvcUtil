using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmStatusType : EdmComplexType
    {
        public CrmStatusType(EdmDocument document)
            : base(document, "Status")
        {
        }
    }
}