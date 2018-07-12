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
        public static PaymentPlan AddPaymentPlan(this ActivityGroup activityGroup, int costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return PaymentPlanService.CreateNew(activityGroup.GetAccount(msgs, dc), ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, loggedInUser, msgs, dc);
        }

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
    }
}
