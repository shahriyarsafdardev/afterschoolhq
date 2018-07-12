using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantSubscriptionPlanService
    {
        public static RegistrantSubscriptionPlan CreateNew(Registrant registrant, SubscriptionPlan subscriptionPlan, DateTime startDate, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var rpp = new RegistrantSubscriptionPlan()
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                SubscriptionPlanId = subscriptionPlan.ID,
                RegistrantId = registrant.ID,
                StartDate = startDate
            };

            msgs.AddRange(rpp.Save(dc));

            // TODO: Integrate Stripe

            return rpp;
        }
    }
}
