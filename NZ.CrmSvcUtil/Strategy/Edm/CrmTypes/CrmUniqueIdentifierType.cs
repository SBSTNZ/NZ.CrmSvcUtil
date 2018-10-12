using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmUniqueIdentifierType : EdmComplexType
    {
        public CrmUniqueIdentifierType(EdmDocument document)
            : base(document, "UniqueIdentifier")
        {
        }
    }
}