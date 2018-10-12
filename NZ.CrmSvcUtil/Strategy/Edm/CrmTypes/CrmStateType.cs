using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmStateType : EdmComplexType
    {
        public CrmStateType(EdmDocument document)
            : base(document, "State")
        {
        }
    }
}