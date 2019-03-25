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

            var totalCost = checkoutObject.TuitionCostNoFees;

            if (checkoutObject.PaymentPlan.IsNotEmpty())
                totalCost = checkoutObject.PaymentPlanCostTodayNoFees;
            else if (checkoutObject.SubscriptionPlan.IsNotEmpty())
                totalCost += checkoutObject.SubscriptionCostTodayNoFees;

            if (checkoutObject.OverrideAmount.HasValue)
                totalCost = checkoutObject.OverrideAmount.Value;

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
                PurchaserAccountId = checkoutObject.PayingAccount?.ID,
                RegistrantId = checkoutObject.MasterRegistrant?.ID,
                RegistrationId = checkoutObject.Registration.ID,
                StripeToken = checkoutObject.StripeToken,
                StripeSourceId = checkoutObject.SourceId
            };
            msgs.AddRange(purchase.Save(dc));

            return purchase;
        }
    }
}
