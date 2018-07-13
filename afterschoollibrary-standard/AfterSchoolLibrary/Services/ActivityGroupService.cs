using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityGroupService
    {
        public static SystemMessages InviteAccount(this ActivityGroup activityGroup, Account accountToInvite, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var aag = AccountActivityGroupService.CreateNew(accountToInvite, activityGroup, InviteStatus.Invited, loggedInUser, msgs, dc);

            // send email to invited account
            if (msgs.HasNoErrors())
            {
                // TODO: Write this
            }

            return msgs;
        }

        public static PaymentPlan AddPaymentPlan(this ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return PaymentPlanService.CreateNew(
                activityGroup.GetAccount(msgs, dc),
                activityGroup,
                costPerPeriod,
                billingPeriodType,
                totalPeriods,
                loggedInUser,
                msgs,
                dc);
        }

        public static SubscriptionPlan AddSubscriptionPlan(this ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SubscriptionPlanService.CreateNew(
                activityGroup.GetAccount(msgs, dc),
                activityGroup,
                costPerPeriod,
                billingPeriodType,
                doesAutoRenew,
                loggedInUser,
                msgs,
                dc);
        }
    }
}
