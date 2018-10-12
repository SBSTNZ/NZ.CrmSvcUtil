using NZ.CrmSvcUtil.Strategy.Edm.Design;

namespace NZ.CrmSvcUtil.Strategy.Edm.CrmTypes
{
    public class CrmPartyListType : EdmComplexType
    {
        public CrmPartyListType(EdmDocument document)
            : base(document, "PartyList")
        {
        }
    }
}