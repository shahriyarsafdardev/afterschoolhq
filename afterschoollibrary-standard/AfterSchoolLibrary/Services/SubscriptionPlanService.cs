using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionPlanService
    {
        public static SubscriptionPlan CreateNewSubscription(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscription(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, doesAutoRenew, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscription(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscription(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, doesAutoRenew, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscription(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var plan = new SubscriptionPlan()
            {
                PlanOwnerAccountId = planOwner.ID,
                BillingPeriodTypeId = billingPeriodType,
                CostPerPeriod = costPerPeriod,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
                IsActive = true,
                ParticipationObjectId = participationObjectId,
                ParticipationObjectTypeId = participationObjectType,
                DoesAutoRenew = doesAutoRenew,
                PlanTypeId = SubscriptionPlanType.Subscription
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var plan = new SubscriptionPlan()
            {
                PlanOwnerAccountId = planOwner.ID,
                BillingPeriodTypeId = billingPeriodType,
                CostPerPeriod = costPerPeriod,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
                IsActive = true,
                ParticipationObjectId = participationObjectId,
                ParticipationObjectTypeId = participationObjectType,
                TotalPeriods = totalPeriods,
                PlanTypeId = SubscriptionPlanType.PaymentPlan
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static ParticipationObject GetParticipationObject(this SubscriptionPlan plan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(plan.ParticipationObjectId.Value, plan.ParticipationObjectTypeId, msgs, dc);
        }

        public static SystemMessages SaveWithStripe(this SubscriptionPlan subscriptionPlan, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(subscriptionPlan.Save(dc));

            if (msgs.HasErrors())
                return msgs;

            Account account = null;
            string uid = "";
            string name = "";
            string uniqueName = "";

            if (subscriptionPlan.ParticipationObjectTypeId == ParticipationObjectType.Activity)
            {
                var activity = subscriptionPlan.GetActivity(msgs, dc);
                account = activity.GetAccount(msgs, dc);
                uid = activity.Uid.ToString();
                name = activity.Name;
                uniqueName = activity.UniqueName;
            }
            else if (subscriptionPlan.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
            {
                var activityGroup = subscriptionPlan.GetActivityGroup(msgs, dc);
                account = activityGroup.GetAccount(msgs, dc);
                uid = activityGroup.Uid.ToString();
                name = activityGroup.Name;
                uniqueName = activityGroup.UniqueName;
            }
            else
            {
                msgs.Add("Invalid Object Type", MessageType.Error);
                return msgs;
            }

            string stripeUserId = null;

            if (account.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                stripeUserId = account.StripeUserId;

            // do stripe
            var product = Billing.StripeHelper.GetProduct(uid, stripeUserId, msgs);

            if (product.IsEmpty())
            {
                var metaData = new System.Collections.Generic.Dictionary<string, string>();
                metaData.Add("AccountUid", account.Uid.ToString());
                metaData.Add("AccountName", account.Name);
                metaData.Add("AccountId", account.ID.ToString());
                metaData.Add("ActivityType", subscriptionPlan.ParticipationObjectTypeId.ToString());
                metaData.Add("ActivityUid", uid);
                metaData.Add("ActivityName", name);

                product = Billing.StripeHelper.CreateProduct(
                    uid,
                    name,
                    uniqueName,
                    subscriptionPlan.IsActive,
                    stripeUserId,
                    metaData,
                    msgs);
            }

            if (msgs.HasErrors())
                return msgs;

            Stripe.StripePlan plan = null;

            if (subscriptionPlan.StripePlanId.IsNotEmpty())
            {
                plan = Billing.StripeHelper.GetPlan(subscriptionPlan.StripePlanId, stripeUserId, msgs);
                Billing.StripeHelper.UpdatePlan(subscriptionPlan.StripePlanId, plan.Nickname, plan.Active, stripeUserId, msgs);
            }
            else
            {
                var metaData = new System.Collections.Generic.Dictionary<string, string>();
                metaData.Add("AccountUid", account.Uid.ToString());
                metaData.Add("AccountName", account.Name);
                metaData.Add("AccountId", account.ID.ToString());
                metaData.Add("ActivityType", subscriptionPlan.ParticipationObjectTypeId.ToString());
                metaData.Add("ActivityUid", uid);
                metaData.Add("ActivityName", name);

                plan = Billing.StripeHelper.CreatePlan(
                    product,
                    null,
                    subscriptionPlan.GetTotalCostWithFees(account),
                    subscriptionPlan.BillingPeriodTypeId,
                    subscriptionPlan.GetNickName(name),
                    stripeUserId,
                    metaData,
                    msgs
                    );
            }

            // update stripe plan id
            subscriptionPlan.StripePlanId = plan.Id;
            msgs.AddRange(subscriptionPlan.Save(dc));

            return msgs;
        }

        public static SubscriptionPlan Copy(this SubscriptionPlan subscriptionPlan, int newParticipationObjectId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SubscriptionPlan copy = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   copy = subscriptionPlan.CopyFields();
                   copy.ParticipationObjectId = newParticipationObjectId;
                   copy.CreateDate = DateTime.UtcNow;
                   copy.CreatedBy = loggedInUserId;
                   copy.LastModifiedDate = DateTime.UtcNow;
                   copy.LastModifiedBy = loggedInUserId;
                   amsgs.AddRange(copy.SaveWithStripe(adc));
               },
               onSuccess: (SystemMessages smsgs) =>
               {
                   msgs.AddRange(smsgs);
               },
               onFailure: (SystemMessages fmsgs) =>
               {
                   msgs.AddRange(fmsgs);
               },
               onException: (SystemMessages emsgs) =>
               {
                   Helpers.Utilities.SystemErrorLog(emsgs);
               },
               dc: dc
           );

            return copy;
        }

        private static SubscriptionPlan CopyFields(this SubscriptionPlan subscriptionPlan)
        {
            return new SubscriptionPlan()
            {
                ParticipationObjectTypeId = subscriptionPlan.ParticipationObjectTypeId,
                PlanOwnerAccountId = subscriptionPlan.PlanOwnerAccountId,
                CostPerPeriod = subscriptionPlan.CostPerPeriod,
                BillingPeriodTypeId = subscriptionPlan.BillingPeriodTypeId,
                DoesAutoRenew = subscriptionPlan.DoesAutoRenew,
                IsActive = subscriptionPlan.IsActive,
                PlanTypeId = subscriptionPlan.PlanTypeId,
                TotalPeriods = subscriptionPlan.TotalPeriods
            };
        }

        public static void CheckAndCancelPaymentPlan(this SubscriptionPlan plan, RegistrantSubscriptionPlan registrantSubscriptionPlan, Account billingAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var totalPayments = registrantSubscriptionPlan.GetPaymentCount(msgs, dc);
            string stripeUserId = null;

            if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                stripeUserId = billingAccount.StripeUserId;

            if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && totalPayments >= plan.TotalPeriods.Value)
                msgs.AddRange(Billing.StripeHelper.CancelSubscription(
                    registrantSubscriptionPlan.StripeSubscriptionId,
                    stripeUserId
                ));
        }

        public static decimal GetTotalCostWithFees(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (subscriptionPlan.CostPerPeriod.HasValue)
                return subscriptionPlan.CostPerPeriod.Value + 
                    Billing.Platform.GetCreditCardFee(subscriptionPlan.CostPerPeriod.Value, ownerAccount.IsFeeTakenOutOfCost) + 
                    Billing.Platform.GetConvenienceFee(subscriptionPlan.CostPerPeriod.Value, ownerAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public static decimal GetCreditCardFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (subscriptionPlan.CostPerPeriod.HasValue)
                return Billing.Platform.GetCreditCardFee(subscriptionPlan.CostPerPeriod.Value, ownerAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public static decimal GetConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (subscriptionPlan.CostPerPeriod.HasValue)
                return Billing.Platform.GetConvenienceFee(subscriptionPlan.CostPerPeriod.Value, ownerAccount.IsFeeTakenOutOfCost);

            return 0;
        }
    }
}
