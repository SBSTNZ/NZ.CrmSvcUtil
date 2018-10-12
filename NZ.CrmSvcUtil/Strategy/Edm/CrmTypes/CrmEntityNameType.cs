using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmEntityNameType : EdmComplexType
    {
        public CrmEntityNameType(EdmDocument document)
            : base(document, "EntityName")
        {
        }
    }
}