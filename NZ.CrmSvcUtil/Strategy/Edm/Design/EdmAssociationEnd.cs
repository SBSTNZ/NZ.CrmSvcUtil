using System;

namespace NZ.CrmSvcUtil.Strategy.Edm.Design
{
    public enum EdmMultiplicity
    {
        One,
        Many,
        OneOrNone
    }

    public class EdmAssociationEnd
    {
        public EdmEntity Entity { get; private set; }
        public EdmMultiplicity Multiplicity { get; private set; }
        public string Role { get; private set; }

        public string MultiplicityString
        {
            get
            {
                if (Multiplicity == EdmMultiplicity.One)
                {
                    return "1";
                }
                else if (Multiplicity == EdmMultiplicity.Many)
                {
                    return "*";
                }
                else if (Multiplicity == EdmMultiplicity.OneOrNone)
                {
                    return "0..1";
                }
                else
                {
                    return null;
                }
            }
        }

        public EdmAssociationEnd(EdmEntity entity, string role, EdmMultiplicity mul)
        {
            if (entity == null) throw new NullReferenceException("Entity argument not set to instance");

            Entity = entity;
            Multiplicity = mul;
            Role = String.IsNullOrWhiteSpace(role) ? entity.EdmDocument.MakeUniqueRoleName() : role;
        }
    }
}