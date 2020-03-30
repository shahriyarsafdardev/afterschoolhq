using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionPlanService
    {
        public static SubscriptionPlan CreateNewSubscriptionPlan(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscriptionPlan(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, doesAutoRenew, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscriptionPlan(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscriptionPlan(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, doesAutoRenew, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscriptionPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, PlanPaymentStartType.WhenActivityStarts, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, PlanPaymentStartType.WhenActivityStarts, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, PlanPaymentStartType paymentStartType, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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
                PlanTypeId = SubscriptionPlanType.PaymentPlan,
                PaymentStartTypeId = paymentStartType
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static ParticipationObject GetParticipationObject(this SubscriptionPlan plan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(plan.ParticipationObjectId.Value, plan.ParticipationObjectTypeId, msgs, dc);
        }

        private static (Account account, Guid uid, string name, string uniqueName) GetActivityInfoDuringSave(this SubscriptionPlan subscriptionPlan, SystemMessages msgs, DatabaseConnection dc)
        {
            Account account = null;
            Guid uid;
            string name = "";
            string uniqueName = "";

            if (subscriptionPlan.ParticipationObjectTypeId == ParticipationObjectType.Activity)
            {
                var activity = subscriptionPlan.GetActivity(msgs, dc);
                account = activity.GetAccount(msgs, dc);
                uid = activity.Uid;
                name = activity.Name;
                uniqueName = activity.UniqueName;
            }
            else if (subscriptionPlan.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
            {
                var activityGroup = subscriptionPlan.GetActivityGroup(msgs, dc);
                account = activityGroup.GetAccount(msgs, dc);
                uid = activityGroup.Uid;
                name = activityGroup.Name;
                uniqueName = activityGroup.UniqueName;
            }
            else
            {
                msgs.Add("Invalid Object Type", MessageType.Error);
            }

            return (account, uid, name, uniqueName);
        }

        private static Stripe.Plan CreateOrUpdateStripePlan(this SubscriptionPlan subscriptionPlan, Stripe.Product product, Account account, Guid uid, string name, string stripeUserId, SystemMessages msgs, DatabaseConnection dc)
        {
            Stripe.Plan plan = null;

            if (subscriptionPlan.StripePlanId.IsNotEmpty())
            {
                plan = Billing.StripeHelper.GetPlan(subscriptionPlan.StripePlanId, stripeUserId, msgs);
                Billing.StripeHelper.UpdatePlan(subscriptionPlan.StripePlanId, plan.Nickname, plan.Active, stripeUserId, msgs);
            }
            else
                plan = Billing.StripeHelper.CreatePlan(
                    product,
                    null,
                    subscriptionPlan.GetTotalCostWithFees(account),
                    subscriptionPlan.BillingPeriodTypeId,
                    subscriptionPlan.GetNickName(name),
                    stripeUserId,
                    subscriptionPlan.GetStripeMetaData(account, uid, name),
                    msgs
                    );

            if (plan.IsNotEmpty())
            {
                subscriptionPlan.StripePlanId = plan.Id;
                msgs.AddRange(subscriptionPlan.Save(dc));
            }

            return plan;
        }

        private static Stripe.Product GetOrCreateStripeProduct(this SubscriptionPlan subscriptionPlan, Account account, Guid uid, string name, string uniqueName, string stripeUserId, SystemMessages msgs)
        {
            var product = Billing.StripeHelper.GetProduct(uid.ToString(), stripeUserId, msgs);

            if (product.IsEmpty())
                product = Billing.StripeHelper.CreateProduct(
                    uid.ToString(),
                    name,
                    uniqueName,
                    subscriptionPlan.IsActive,
                    stripeUserId,
                    subscriptionPlan.GetStripeMetaData(account, uid, name),
                    msgs);

            return product;
        }

        private static Dictionary<string, string> GetStripeMetaData(this SubscriptionPlan subscriptionPlan, Account account, Guid uid, string name)
        {
            return new Dictionary<string, string>
            {
                { "AccountUid", account.Uid.ToString() },
                { "AccountName", account.Name },
                { "AccountId", account.ID.ToString() },
                { "ActivityType", subscriptionPlan.ParticipationObjectTypeId.ToString() },
                { "ActivityUid", uid.ToString() },
                { "ActivityName", name }
            };
        }

        public static SystemMessages SaveWithStripe(this SubscriptionPlan subscriptionPlan, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(subscriptionPlan.Save(dc));

            if (msgs.HasErrors())
                return msgs;
            
            var (account, uid, name, uniqueName) = subscriptionPlan.GetActivityInfoDuringSave(msgs, dc);
            string stripeUserId = account.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc) ? account.StripeUserId : null;

            // do stripe
            var product = subscriptionPlan.GetOrCreateStripeProduct(account, uid, name, uniqueName, stripeUserId, msgs);

            if (msgs.HasErrors())
                return msgs;

            subscriptionPlan.CreateOrUpdateStripePlan(product, account, uid, name, stripeUserId, msgs, dc);

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
                TotalPeriods = subscriptionPlan.TotalPeriods,
                PaymentStartTypeId = subscriptionPlan.PaymentStartTypeId
            };
        }

        public static bool CancelSubscription(this SubscriptionPlan plan, RegistrantSubscriptionPlan rsp, Account billingAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (plan.PlanTypeId == SubscriptionPlanType.Subscription)
            {                
                string stripeUserId = null;

                if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                    stripeUserId = billingAccount.StripeUserId;

                msgs.AddRange(Billing.StripeHelper.CancelSubscription(
                    rsp.StripeSubscriptionId,
                    stripeUserId
                ));

                rsp.MarkAsCancelled(loggedInUser, msgs, dc);

                if (msgs.HasNoErrors())
                    return true;
            }

            return false;
        }

        public static bool CheckAndCancelPaymentPlan(this SubscriptionPlan plan, RegistrantSubscriptionPlan rsp, Account billingAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {            
            if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan)
            {
                var totalPayments = rsp.GetPaymentCount(msgs, dc);
                var totalPeriods = rsp.TotalPeriods.HasValue ? rsp.TotalPeriods.Value : plan.TotalPeriods.Value;

                if (totalPayments >= totalPeriods)
                {
                    string stripeUserId = null;
                    
                    if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                        stripeUserId = billingAccount.StripeUserId;

                    var stripeSubscription = Billing.StripeHelper.GetSubscription(rsp.StripeSubscriptionId, stripeUserId, msgs);

                    if (stripeSubscription.IsNotEmpty() && !stripeSubscription.CanceledAt.HasValue)
                        msgs.AddRange(Billing.StripeHelper.CancelSubscription(
                            rsp.StripeSubscriptionId,
                            stripeUserId
                        ));

                    if (msgs.HasNoErrors())
                        return true;
                }
            }

            return false;
        }

        public static decimal GetTotalCostWithFees(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (subscriptionPlan.CostPerPeriod.HasValue)
                return subscriptionPlan.CostPerPeriod.Value 
                    + subscriptionPlan.GetCreditCardFee(ownerAccount, msgs, dc)
                    + subscriptionPlan.GetConvenienceFee(ownerAccount, msgs, dc);

            return 0;
        }

        public static decimal GetTotalCostWithConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (subscriptionPlan.CostPerPeriod.HasValue)
                return subscriptionPlan.CostPerPeriod.Value
                    + subscriptionPlan.GetConvenienceFee(ownerAccount, msgs, dc);

            return 0;
        }

        public static decimal GetCreditCardFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (subscriptionPlan.CostPerPeriod.HasValue && !ownerAccount.HasFeature(FeatureType.UseStripeConnectedAccount))
                return Billing.Platform.GetCreditCardFee(subscriptionPlan.CostPerPeriod.Value, ownerAccount);

            return 0;
        }

        public static decimal GetConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (subscriptionPlan.CostPerPeriod.HasValue && !ownerAccount.HasFeature(FeatureType.UseStripeConnectedAccount))
                return Billing.Platform.GetConvenienceFee(subscriptionPlan.CostPerPeriod.Value);

            return 0;
        }
                
        public static bool DoesCouponApply(this SubscriptionPlan plan, Coupon coupon)
        {
            if (coupon.IsNotEmpty() && coupon.ID.HasValue)
            {
                if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && coupon.DoesApplyToTuitionFee())
                    return true;
                else if (plan.PlanTypeId == SubscriptionPlanType.Subscription && coupon.DoesApplyToSubscriptionPlan())
                    return true;
            }
            
            return false;
        }

        public static RegistrantSubscriptionPlan AddRegistrant(this SubscriptionPlan plan, Registrant registrant, DateTime? startDateTime, Purchase purchase, string stripeSubscriptionId, PlanFeeObject feeObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantSubscriptionPlanService.CreateNew(
                registrant,
                plan,
                startDateTime.IfEmpty(DateTime.UtcNow),
                purchase.ID,
                stripeSubscriptionId,
                feeObject,
                loggedInUser,
                msgs,
                dc);
        }
    }
}
