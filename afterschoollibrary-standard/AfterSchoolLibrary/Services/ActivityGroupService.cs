using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityGroupService
    {
        /// <summary>
        /// Invite an account to an external league or session
        /// </summary>
        /// <param name="activityGroup"></param>
        /// <param name="accountToInvite"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SystemMessages InviteAccount(this ActivityGroup activityGroup, Account accountToInvite, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var aag = AccountActivityGroupService.CreateNew(accountToInvite, activityGroup, InviteStatus.Invited, loggedInUser, msgs, dc);

            // send email to invited account
            if (msgs.HasNoErrors())
            {
                // TODO: Write this
                msgs.Add("No email sent", MessageType.Error);
            }

            return msgs;
        }

        /// <summary>
        /// Create a payment plan plan to allow a registrant to subscribe to when signing up
        /// </summary>
        /// <param name="activityGroup"></param>
        /// <param name="costPerPeriod"></param>
        /// <param name="billingPeriodType"></param>
        /// <param name="totalPeriods"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SubscriptionPlan AddPaymentPlan(this ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SubscriptionPlanService.CreateNewPaymentPlan(
                activityGroup.GetAccount(msgs, dc),
                activityGroup,
                costPerPeriod,
                billingPeriodType,
                totalPeriods,
                loggedInUser,
                msgs,
                dc);
        }

        /// <summary>
        /// Create a subscription plan to allow a registrant to subscribe to when signing up
        /// </summary>
        /// <param name="activityGroup"></param>
        /// <param name="costPerPeriod"></param>
        /// <param name="billingPeriodType"></param>
        /// <param name="doesAutoRenew"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SubscriptionPlan AddSubscriptionPlan(this ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SubscriptionPlanService.CreateNewSubscription(
                activityGroup.GetAccount(msgs, dc),
                activityGroup,
                costPerPeriod,
                billingPeriodType,
                doesAutoRenew,
                loggedInUser,
                msgs,
                dc);
        }

        public static List<SubscriptionPlan> GetPaymentPlans(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, SubscriptionPlanType.PaymentPlan, msgs, dc);
        }

        public static List<SubscriptionPlan> GetSubscriptionPlans(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, SubscriptionPlanType.Subscription, msgs, dc);
        }

        public static bool HasAccountActivityGroup(this ActivityGroup activityGroup, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activityGroup.AccountId != accountId)
                return AccountActivityGroup.GetCount(
                    new System.Collections.Hashtable()
                    {
                        { "AccountId", accountId },
                        { "ActivityGroupId", activityGroup.ID.Value }
                    },
                    msgs,
                    dc
                    ) > 0;

            return false;
        }

        public static AccountActivityGroup GetAccountActivityGroup(this ActivityGroup activityGroup, Account currentAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activityGroup.AccountId != currentAccount.ID ? AccountActivityGroup.LoadByAccountIdByActivityGroupId(currentAccount.ID.Value, activityGroup.ID.Value, msgs) : null;
        }

        public static int GetLeagueInviteCount(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.GetCount(
                new System.Collections.Hashtable()
                {
                    { "activitygroupid", activityGroup.ID.Value}
                },
                msgs);
        }
    }
}
