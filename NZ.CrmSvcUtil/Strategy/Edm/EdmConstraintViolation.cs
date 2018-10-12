using System;

namespace NZ.CrmSvcUtil.Strategy.Edm
{
    public class EdmConstraintViolation : Exception
    {
        public EdmConstraintViolation(string message) : base(message)
        {
        }
    }
}