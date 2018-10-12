using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmMemoType : EdmComplexType
    {
        public CrmMemoType(EdmDocument document)
            : base(document, "Memo")
        {
        }
    }
}