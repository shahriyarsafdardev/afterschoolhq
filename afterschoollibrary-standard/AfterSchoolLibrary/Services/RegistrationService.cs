using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrationService
    {
        public static bool IsAlreadyPaidFor(this Registration registration, SystemMessages msgs = null)
        {
            var purchase = registration.GetPurchase(msgs);

            if (purchase.IsNotEmpty())
                return Transaction.GetCountByPurchaseId(purchase.ID.Value) > 0;

            return false;
        }
    }
}
