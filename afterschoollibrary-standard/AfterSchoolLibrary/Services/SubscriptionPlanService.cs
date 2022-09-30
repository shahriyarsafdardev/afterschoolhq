using System;
using System.Collections.Generic;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using Account = AfterSchoolLibrary.Models.Account;
using Coupon = AfterSchoolLibrary.Models.Coupon;
using Product = Stripe.Product;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionPlanService
    {
        public static SubscriptionPlan CreateNewSubscriptionPlan(Account planOwner, Activity activity, decimal costPerPeriod,
            BillingPeriodType billingPeriodType, bool doesAutoRenew, bool splitCostEvenly, bool regFeeAsDownPayment, bool isConvenienceFeeTakenOutOfCost,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscriptionPlan(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, doesAutoRenew,
                splitCostEvenly, regFeeAsDownPayment, isConvenienceFeeTakenOutOfCost, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscriptionPlan(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod,
            BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscriptionPlan(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType,
                doesAutoRenew, false, false, false, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscriptionPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId,
            decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, bool splitCostEvenly, bool regFeeAsDownPayment,
            bool isConvenienceFeeTakenOutOfCost, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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
                SplitCostEvenly = splitCostEvenly,
                PlanTypeId = SubscriptionPlanType.Subscription,
                RegFeeAsDownPayment = regFeeAsDownPayment,
                IsConvenienceFeeTakenOutOfCost = isConvenienceFeeTakenOutOfCost,
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static SubscriptionPlan CreateNewSubscriptionPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId,
            decimal costPerPeriod, BillingPeriodType billingPeriodType,
            PlanPaymentStartType paymentStartType, DateTime? subscriptionStartDate, DateTime? subscriptionEndDate, bool isongoingsubscription,
            bool doesAutoRenew, bool splitCostEvenly, bool regFeeAsDownPayment,
            bool isConvenienceFeeTakenOutOfCost, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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
                PaymentStartTypeId = paymentStartType,
                IsOngoingSubscription = isongoingsubscription,
                DoesAutoRenew = doesAutoRenew,
                SplitCostEvenly = splitCostEvenly,
                PlanTypeId = SubscriptionPlanType.Subscription,
                RegFeeAsDownPayment = regFeeAsDownPayment,
                IsConvenienceFeeTakenOutOfCost = isConvenienceFeeTakenOutOfCost,
                StartDate = subscriptionStartDate,
                EndDate = subscriptionEndDate,
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType,
            int totalPeriods, bool isConvenienceFeeTakenOutOfCost, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, totalPeriods,
                PlanPaymentStartType.WhenActivityStarts, isConvenienceFeeTakenOutOfCost, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType,
            int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, totalPeriods,
                PlanPaymentStartType.WhenActivityStarts, false, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod,
            BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType,
                totalPeriods, PlanPaymentStartType.WhenActivityStarts, false, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId,
            decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, PlanPaymentStartType paymentStartType,
            bool isConvenienceFeeTakenOutOfCost, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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
                PaymentStartTypeId = paymentStartType,
                IsConvenienceFeeTakenOutOfCost = isConvenienceFeeTakenOutOfCost,
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static SubscriptionPlan CreateNewPostPayPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId,
            decimal costPerPeriod, BillingPeriodType billingPeriodType, bool isConvenienceFeeTakenOutOfCost, int? prepaidPeriods, int? loggedInUserId,
            SystemMessages msgs, DatabaseConnection dc = null)
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
                TotalPeriods = null,
                PlanTypeId = SubscriptionPlanType.PostPayPlan,
                PaymentStartTypeId = PlanPaymentStartType.NotSet,
                IsConvenienceFeeTakenOutOfCost = isConvenienceFeeTakenOutOfCost,
                NumberOfPeriodsPrePaid = prepaidPeriods,
            };

            msgs.AddRange(plan.Save(dc));

            return plan;
        }

        public static ParticipationObject GetParticipationObject(this SubscriptionPlan plan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(plan.ParticipationObjectId.Value, plan.ParticipationObjectTypeId, msgs, dc);
        }

        private static (Account account, Guid uid, string name, string uniqueName) GetActivityInfoDuringSave(this SubscriptionPlan subscriptionPlan,
            SystemMessages msgs, DatabaseConnection dc)
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

        private static Plan CreateOrUpdateStripePlan(this SubscriptionPlan subscriptionPlan, Product product, Account account, Guid uid, string name,
            string stripeUserId, SystemMessages msgs, DatabaseConnection dc)
        {
            Plan plan = null;
            var stripeHelper = new StripeHelper();

            if (subscriptionPlan.StripePlanId.IsNotEmpty())
            {
                plan = stripeHelper.GetPlan(subscriptionPlan.StripePlanId, stripeUserId, msgs);
                stripeHelper.UpdatePlan(subscriptionPlan.StripePlanId, plan.Nickname, plan.Active, stripeUserId, msgs);
            }
            else
                plan = stripeHelper.CreatePlan(
                    product,
                    null,
                    subscriptionPlan.GetTotalCostWithFees(account, msgs, dc),
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

        private static Product GetOrCreateStripeProduct(this SubscriptionPlan subscriptionPlan, Account account, Guid uid, string name, string uniqueName,
            string stripeUserId, SystemMessages msgs)
        {
            var stripeHelper = new StripeHelper();
            var product = stripeHelper.GetProduct(uid.ToString(), stripeUserId, msgs);

            if (product.IsEmpty())
                product = stripeHelper.CreateProduct(
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
                {"AccountUid", account.Uid.ToString()},
                {"AccountName", account.Name},
                {"AccountId", account.ID.ToString()},
                {"ActivityType", subscriptionPlan.ParticipationObjectTypeId.ToString()},
                {"ActivityUid", uid.ToString()},
                {"ActivityName", name},
            };
        }

        public static SystemMessages SaveWithStripe(this SubscriptionPlan subscriptionPlan, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(subscriptionPlan.Save(dc));

            if (msgs.HasErrors())
                return msgs;

            var (account, uid, name, uniqueName) = subscriptionPlan.GetActivityInfoDuringSave(msgs, dc);
            string stripeUserId = null; // account.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc) ? account.StripeUserId : null;

            // do stripe
            var product = subscriptionPlan.GetOrCreateStripeProduct(account, uid, name, uniqueName, stripeUserId, msgs);

            if (msgs.HasErrors())
                return msgs;

            subscriptionPlan.CreateOrUpdateStripePlan(product, account, uid, name, stripeUserId, msgs, dc);

            return msgs;
        }

        public static SubscriptionPlan Copy(this SubscriptionPlan subscriptionPlan, int newParticipationObjectId, int loggedInUserId, SystemMessages msgs,
            DatabaseConnection dc = null)
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
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return copy;
        }

        private static SubscriptionPlan CopyFields(this SubscriptionPlan plan)
        {
            return new SubscriptionPlan()
            {
                ParticipationObjectTypeId = plan.ParticipationObjectTypeId,
                PlanOwnerAccountId = plan.PlanOwnerAccountId,
                CostPerPeriod = plan.CostPerPeriod,
                BillingPeriodTypeId = plan.BillingPeriodTypeId,
                DoesAutoRenew = plan.DoesAutoRenew,
                IsActive = plan.IsActive,
                PlanTypeId = plan.PlanTypeId,
                TotalPeriods = plan.TotalPeriods,
                PaymentStartTypeId = plan.PaymentStartTypeId,
                SplitCostEvenly = plan.SplitCostEvenly,
                RegFeeAsDownPayment = plan.RegFeeAsDownPayment,
                IsConvenienceFeeTakenOutOfCost = plan.IsConvenienceFeeTakenOutOfCost,
                NumberOfPeriodsPrePaid = plan.NumberOfPeriodsPrePaid,
            };
        }

        public static bool CancelSubscription(this SubscriptionPlan plan, RegistrantSubscriptionPlan rsp, Account billingAccount, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (plan.PlanTypeId == SubscriptionPlanType.Subscription)
            {
                string stripeUserId = null;

                //if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                //    stripeUserId = billingAccount.StripeUserId;

                new StripeHelper().CancelSubscription(rsp.StripeSubscriptionId);

                rsp.MarkAsCancelled(loggedInUser, msgs, dc);

                if (msgs.HasNoErrors())
                    return true;
            }

            return false;
        }

        public static decimal GetTotalCostWithFees(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ownerAccount ??= subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var creditCardFee = subscriptionPlan.GetCreditCardFee(ownerAccount, msgs, dc);
            return subscriptionPlan.GetTotalCostWithConvenienceFee(ownerAccount, msgs, dc) + creditCardFee;
        }

        public static decimal GetTotalCostWithConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ownerAccount ??= subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var convenienceFee = subscriptionPlan.IsConvenienceFeeTakenOutOfCost ? 0 : subscriptionPlan.GetConvenienceFee(ownerAccount, msgs, dc);
            return subscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc) + convenienceFee;
        }

        public static decimal GetMinCostWithConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (!subscriptionPlan.SplitCostEvenly)
                return subscriptionPlan.GetTotalCostWithConvenienceFee(ownerAccount, msgs, dc);

            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var participationObject = subscriptionPlan.GetParticipationObject();
            var subscriptionPlanCostPerPeriod = subscriptionPlan.CostPerPeriod.Value / participationObject.SeatsAvailable.Value;
            var splitCostConvenienceFee = subscriptionPlan.GetSplitCostConvenienceFee(ownerAccount, participationObject.SeatsAvailable.Value, msgs, dc);
            return subscriptionPlanCostPerPeriod + splitCostConvenienceFee;
        }

        public static decimal GetMaxCostWithConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (!subscriptionPlan.SplitCostEvenly)
                return subscriptionPlan.GetTotalCostWithConvenienceFee(ownerAccount, msgs, dc);

            if (ownerAccount.IsEmpty())
                ownerAccount = subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var po = subscriptionPlan.GetParticipationObject();
            var minimumSeats = po.MinSeatsRequired ?? 1;
            var subscriptionPlanCostPerPeriodPerSeat = subscriptionPlan.CostPerPeriod.Value / minimumSeats;
            var splitCostConvenienceFee = subscriptionPlan.GetSplitCostConvenienceFee(ownerAccount, minimumSeats, msgs, dc);
            return subscriptionPlanCostPerPeriodPerSeat + splitCostConvenienceFee;
        }

        public static decimal GetCreditCardFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ownerAccount ??= subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var feeCalculationObject = new FeeCalculationObject(subscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc), ownerAccount, msgs, dc);
            return feeCalculationObject.GetCreditCardFee();
        }

        public static decimal GetConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ownerAccount ??= subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var feeCalculationObject = new FeeCalculationObject(subscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc), ownerAccount, msgs, dc);
            return feeCalculationObject.GetConvenienceFee();
        }

        public static decimal GetSplitCostConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, int seats, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ownerAccount ??= subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var feeCalculationObject = new FeeCalculationObject(subscriptionPlan.CostPerPeriod.Value / seats, ownerAccount, msgs, dc);
            return feeCalculationObject.GetConvenienceFee();
        }

        public static bool DoesCouponApply(this SubscriptionPlan plan, Coupon coupon)
        {
            if (coupon.IsEmpty() || !coupon.ID.HasValue)
                return false;

            return (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && coupon.DoesApplyToRegistrationFee())
                   || (plan.PlanTypeId == SubscriptionPlanType.Subscription && coupon.DoesApplyToSubscriptionPlan());
        }

        public static RegistrantSubscriptionPlan AddRegistrant(this SubscriptionPlan plan, Registrant registrant, DateTime? startDateTime, Purchase purchase,
            string stripeSubscriptionId, PlanFeeObject feeObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static List<RegistrantSubscriptionPlan> GetRegistrantSubscriptionPlans(this SubscriptionPlan plan, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            return RegistrantSubscriptionPlan.LoadBySubscriptionPlanId(plan.ID.Value, msgs, dc);
        }

        public static decimal GetCostPerPeriodPerPerson(this SubscriptionPlan plan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!plan.SplitCostEvenly)
                return plan.CostPerPeriod.Value;

            var po = plan.GetParticipationObject(msgs, dc);
            var seats = po.SeatsTaken ?? 1;

            return plan.CostPerPeriod.Value / (seats == 0 ? 1 : seats);
        }

        public static List<SubscriptionPlanPrePayment> CreatePrePayments(this SubscriptionPlan plan, Account payerAccount,
            ParticipationObject participationObject, Transaction transaction, Registration registration, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            List<SubscriptionPlanPrePayment> list = null;

            if (plan.PlanTypeId == SubscriptionPlanType.PostPayPlan && plan.NumberOfPeriodsPrePaid.IsPositive())
            {
                list = new List<SubscriptionPlanPrePayment>();

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        for (var i = 0; i < plan.NumberOfPeriodsPrePaid.Value; i++)
                        {
                            var prePayment = new SubscriptionPlanPrePayment()
                            {
                                PayerAccountId = payerAccount.ID,
                                TransactionId = transaction.ID,
                                ParticipationObjectId = participationObject.ObjectID,
                                ParticipationObjectTypeId = participationObject.ObjectType,
                                RegistrationId = registration.ID,
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = loggedInUser?.ID,
                            };

                            amsgs.AddRange(prePayment.Save(adc));

                            if (amsgs.HasNoErrors())
                                list.Add(prePayment);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                    dc: dc
                );
            }

            return list;
        }

        public static bool CheckIfSubscriptionRequired(this SubscriptionPlan plan, DateTime startDateTime, DateTime endDateTime)
        {
            var dateSpan = DateTimeSpan.CompareDates(startDateTime, endDateTime);
            if ((plan.BillingPeriodTypeId == BillingPeriodType.Month && dateSpan.Months <= 1) ||
                (plan.BillingPeriodTypeId == BillingPeriodType.Week && dateSpan.Days <= 7))
                return false;

            return true;
        }

        public static DateTime? GetPlanEndDateForRegistrantSubscriptionUtc(this SubscriptionPlan plan, DateTime startDateTime,
            ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var billingAccount = plan.GetAccount(msgs, dc);

            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            if (plan.PlanTypeId == SubscriptionPlanType.PostPayPlan || plan.PlanTypeId == SubscriptionPlanType.NotSet)
                return null;

            DateTime? endDate = null;

            if (plan.EndDate.HasValue)
                endDate = billingAccount.ConvertFromAccountTimeZone(plan.EndDate.Value);

            if (endDate.IsEmpty())
            {
                if (plan.PlanTypeId == SubscriptionPlanType.Subscription)
                    endDate = plan.GetSubscriptionEndDateUtc(participationObject, msgs);

                // no need for UTC since it just calculates based on the passed in start date time
                else if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan)
                    endDate = plan.GetPaymenPlanEndDate(startDateTime, msgs);
            }

            // make sure endDate > startDate for stripe
            if (endDate.HasValue && DateTimeSpan.CompareDates(startDateTime, endDate.Value).TotalDays < 1)
                endDate = startDateTime.AddDays(1);

            return endDate;
        }

        public static DateTime? GetPlanEndDateForRegistrantSubscription(this SubscriptionPlan plan, DateTime startDateTime,
            ParticipationObject participationObject, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            if (plan.PlanTypeId == SubscriptionPlanType.PostPayPlan || plan.PlanTypeId == SubscriptionPlanType.NotSet)
                return null;

            DateTime? endDate = null;

            if (plan.EndDate.HasValue)
                endDate = plan.EndDate.Value;

            if (endDate.IsEmpty())
            {
                if (plan.PlanTypeId == SubscriptionPlanType.Subscription)
                    endDate = plan.GetSubscriptionEndDate(participationObject, msgs);

                if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan)
                    endDate = plan.GetPaymenPlanEndDate(startDateTime, msgs);
            }

            // make sure endDate > startDate for stripe
            if (endDate.HasValue && DateTimeSpan.CompareDates(startDateTime, endDate.Value).TotalDays < 1)
                endDate = startDateTime.AddDays(1);

            return endDate;
        }

        private static DateTime? GetSubscriptionEndDateUtc(this SubscriptionPlan plan, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var billingAccount = plan.GetAccount(msgs, dc);
            var endDate = plan.GetSubscriptionEndDate(participationObject, msgs);

            if (endDate.HasValue)
                return billingAccount.ConvertFromAccountTimeZone(endDate.Value);

            return null;
        }

        private static DateTime? GetSubscriptionEndDate(this SubscriptionPlan plan, ParticipationObject participationObject, SystemMessages msgs = null)
        {
            if (plan.PlanTypeId == SubscriptionPlanType.Subscription && participationObject.EndDate.HasValue)
                return participationObject.EndDate.Value.AddDays(1); // don't want it to end at the start of the end date, should end at the end of the end date

            return null;
        }

        public static DateTime? GetPaymenPlanEndDate(this SubscriptionPlan plan, DateTime startDateTime, SystemMessages msgs = null)
        {
            if (plan.PlanTypeId != SubscriptionPlanType.PaymentPlan)
                return null;

            var endDate = startDateTime;

            if (plan.TotalPeriods.IsEmpty())
                return endDate;

            var periodsToAdd = plan.TotalPeriods.IfEmpty(0) - plan.NumberOfPeriodsPrePaid.IfEmpty(0);
            if (periodsToAdd > 0)
            {
                switch (plan.BillingPeriodTypeId)
                {
                    case BillingPeriodType.Day:
                        endDate = endDate.AddDays(periodsToAdd);
                        break;
                    case BillingPeriodType.Week:
                        endDate = endDate.AddWeeks(periodsToAdd);
                        break;
                    case BillingPeriodType.TwoWeeks:
                        endDate = endDate.AddWeeks(periodsToAdd * 2);
                        break;
                    case BillingPeriodType.Month:
                        endDate = endDate.AddMonths(periodsToAdd);
                        break;
                    case BillingPeriodType.Year:
                        endDate = endDate.AddYears(periodsToAdd);
                        break;
                    default:
                        msgs.Add("Payment plan missing billing period type", MessageType.ApiError);
                        break;
                }
            }

            return endDate;
        }
    }
}