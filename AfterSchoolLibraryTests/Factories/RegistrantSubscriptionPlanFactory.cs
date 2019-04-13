using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System;

namespace AfterSchoolLibraryTests.Factories
{
    public class RegistrantSubscriptionPlanFactory
    {
        public static RegistrantSubscriptionPlan Create(Registrant registrant, SubscriptionPlan subscriptionPlan, Purchase purchase, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantSubscriptionPlan()
            {
                RegistrantId = registrant.ID,
                SubscriptionPlanId = subscriptionPlan.ID,
                StartDate = DateTime.UtcNow,
                PurchaseId = purchase.ID
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
