using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace AfterSchool.Controllers
{
    public class SubscriptionCreationStripeController : Controller
    {
        // GET: SubscriptionCreationStripe
        public string Index(DateTime enddate)
        {
            SystemMessages msgs = new SystemMessages();
            var metaData = new Dictionary<string, string>();
            var subscription = StripeHelper.GetStripeHelperConnectAccount().CreateRegistrantSubscription(
                customerId: "cus_O3CX9NkjA7u96Y",
                planId: "plan_ODflHP17fUTQHp",
                isProrated: false,
                sourceId: "src_1NH69mCEgXQRZLhkOGC9oTBw",
                platformFeePercent: 0,
                stripeAccountId: null,
                startDate: DateTime.UtcNow,
                endDate: enddate,
                billingCycleAnchor: null,
                stripeCouponId: null,
                metaData: metaData,
                msgs: msgs);
            return subscription.Id;
        }
    }
}
