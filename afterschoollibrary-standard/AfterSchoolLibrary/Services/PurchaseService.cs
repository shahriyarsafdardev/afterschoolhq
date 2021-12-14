using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class PurchaseService
    {
        public static Purchase CreateNew(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var totalCost = checkoutObject.GetRegistrationCostWithFees(msgs, dc);

            if (checkoutObject.PaymentPlan.IsNotEmpty())
                totalCost = checkoutObject.PaymentPlanCostTodayNoFees;
            else if (checkoutObject.SubscriptionPlan.IsNotEmpty())
                totalCost += checkoutObject.GetSubscriptionCostTodayNoFees(msgs, dc);

            if (checkoutObject.OverrideAmount.HasValue)
                totalCost = checkoutObject.OverrideAmount.Value;

            var purchase = new Purchase()
            {
                AmountPaid = totalCost,
                CouponId = checkoutObject.Coupon?.ID,
                CreateDate = checkoutObject.Date,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = checkoutObject.Date,
                LoggedInUserId = loggedInUser?.ID.IfEmpty(0),
                NumberOfSeats = checkoutObject.NumberOfSeats,
                PurchaserAccountId = checkoutObject.PayingAccount?.ID,
                RegistrantId = checkoutObject.MasterRegistrant?.ID,
                RegistrationId = checkoutObject.Registration.ID,
                StripeToken = checkoutObject.CreditCardPurchaseObject?.StripeToken,
                StripeSourceId = checkoutObject.StripeSourceId
            };
            msgs.AddRange(purchase.Save(dc));

            return purchase;
        }

        public static Registrant GetRegistrant(this Purchase purchase, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (purchase.RegistrantId.HasValue)
                return Registrant.Load(purchase.RegistrantId.Value, msgs, dc);

            return null;
        }
    }
}
