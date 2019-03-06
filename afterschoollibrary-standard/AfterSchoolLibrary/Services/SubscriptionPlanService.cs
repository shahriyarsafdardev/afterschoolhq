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

            // do stripe
            var product = Billing.StripeHelper.GetProduct(uid, account.StripeUserId, msgs);

            if (product.IsEmpty())
                product = Billing.StripeHelper.CreateProduct(
                    uid,
                    name,
                    uniqueName,
                    subscriptionPlan.IsActive,
                    account.StripeUserId,
                    msgs);

            if (msgs.HasErrors())
                return msgs;

            Stripe.StripePlan plan = null;

            if (subscriptionPlan.StripePlanId.IsNotEmpty())
            {
                plan = Billing.StripeHelper.GetPlan(subscriptionPlan.StripePlanId, account.StripeUserId, msgs);
                Billing.StripeHelper.UpdatePlan(subscriptionPlan.StripePlanId, plan.Nickname, plan.Active, account.StripeUserId, msgs);
            }
            else
            {
                plan = Billing.StripeHelper.CreatePlan(
                    product,
                    null,
                    subscriptionPlan.CostPerPeriod.Value,
                    subscriptionPlan.BillingPeriodTypeId,
                    subscriptionPlan.GetNickName(name),
                    account.StripeUserId,
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

            Database.ExecuteTransaction(
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

        public static void CheckAndCancelPaymentPlan(this SubscriptionPlan plan, RegistrantSubscriptionPlan registrantSubscription, Account billingAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var totalPayments = registrantSubscription.GetPaymentCount(msgs, dc);

            if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && totalPayments >= plan.TotalPeriods.Value)
                msgs.AddRange(Billing.StripeHelper.CancelSubscription(registrantSubscription.StripeSubscriptionId, billingAccount.StripeUserId));
        }
    }
}
