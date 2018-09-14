using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Services
{
    public static class PurchaseService
    {
        public static Purchase CreateNew(Account purchasingAccount, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var totalCost = checkoutObject.TuitionFee;

            if (checkoutObject.PaymentPlan.IsNotEmpty())
                totalCost = checkoutObject.PaymentPlanTotalCost;
            else if (checkoutObject.SubscriptionPlan.IsNotEmpty())
                totalCost += checkoutObject.SubscriptionCostToday;

            var purchase = new Purchase()
            {
                AmountPaid = totalCost,
                CouponId = checkoutObject.Coupon?.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LoggedInUserId = loggedInUser?.ID.IfEmpty(0),
                NumberOfSeats = checkoutObject.NumberOfSeats,
                PurchaserAccountId = purchasingAccount.ID,
                RegistrantId = checkoutObject.Registrant.ID,
                RegistrationId = checkoutObject.Registration.ID,
                StripeToken = checkoutObject.StripeToken,
                StripeSourceId = checkoutObject.SourceId
            };
            msgs.AddRange(purchase.Save(dc));

            return purchase;
        }
    }
}
