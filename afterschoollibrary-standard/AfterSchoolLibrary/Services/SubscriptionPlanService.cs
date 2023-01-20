using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using System;
using System.Collections.Generic;
using Account = AfterSchoolLibrary.Models.Account;
using Product = Stripe.Product;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionPlanService
    {
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

            var plan = new SubscriptionPlan
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

            var plan = new SubscriptionPlan
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

            var plan = new SubscriptionPlan
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

        public static void CreateNewPostPayPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId,
            decimal costPerPeriod, BillingPeriodType billingPeriodType, bool isConvenienceFeeTakenOutOfCost, int? prepaidPeriods, int? loggedInUserId,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var plan = new SubscriptionPlan
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
            var name = "";
            var uniqueName = "";

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

        private static void CreateOrUpdateStripePlan(this SubscriptionPlan subscriptionPlan, Product product, Account account, Guid uid, string name,
            string stripeUserId, SystemMessages msgs, DatabaseConnection dc)
        {
            Plan plan;
            var stripeHelper = new StripeHelper();

            if (subscriptionPlan.StripePlanId.IsNotEmpty())
            {
                plan = stripeHelper.GetPlan(subscriptionPlan.StripePlanId, stripeUserId);
                stripeHelper.UpdatePlan(subscriptionPlan.StripePlanId, plan.Nickname, plan.Active, stripeUserId, msgs);
            }
            else
            {
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
            }

            if (plan.IsEmpty())
                return;
            subscriptionPlan.StripePlanId = plan.Id;
            msgs.AddRange(subscriptionPlan.Save(dc));
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

        public static void Copy(this SubscriptionPlan subscriptionPlan, int newParticipationObjectId, int loggedInUserId, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            msgs ??= new SystemMessages();

            Database.ExecuteTransaction
            (
                (amsgs, adc) =>
                {
                    var copy = subscriptionPlan.CopyFields();
                    copy.ParticipationObjectId = newParticipationObjectId;
                    copy.CreateDate = DateTime.UtcNow;
                    copy.CreatedBy = loggedInUserId;
                    copy.LastModifiedDate = DateTime.UtcNow;
                    copy.LastModifiedBy = loggedInUserId;
                    amsgs.AddRange(copy.SaveWithStripe(adc));
                },
                smsgs => msgs.AddRange(smsgs),
                fmsgs => msgs.AddRange(fmsgs),
                emsgs => Utilities.SystemErrorLog(emsgs),
                dc
            );
        }

        private static SubscriptionPlan CopyFields(this SubscriptionPlan plan)
        {
            return new SubscriptionPlan
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

        private static decimal GetConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ownerAccount ??= subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var feeCalculationObject = new FeeCalculationObject(subscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc), ownerAccount, msgs, dc);
            return feeCalculationObject.GetConvenienceFee();
        }

        private static decimal GetSplitCostConvenienceFee(this SubscriptionPlan subscriptionPlan, Account ownerAccount, int seats, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ownerAccount ??= subscriptionPlan.GetAccount(msgs, dc);

            if (!subscriptionPlan.CostPerPeriod.HasValue)
                return 0;

            var feeCalculationObject = new FeeCalculationObject(subscriptionPlan.CostPerPeriod.Value / seats, ownerAccount, msgs, dc);
            return feeCalculationObject.GetConvenienceFee();
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

        public static void CreatePrePayments(this SubscriptionPlan plan, Account payerAccount, ParticipationObject participationObject, Transaction transaction,
            Registration registration, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (plan.PlanTypeId != SubscriptionPlanType.PostPayPlan || !plan.NumberOfPeriodsPrePaid.IsPositive())
                return;

            Database.ExecuteTransaction
            (
                (amsgs, adc) =>
                {
                    for (var i = 0; i < plan.NumberOfPeriodsPrePaid.Value; i++)
                    {
                        var prePayment = new SubscriptionPlanPrePayment
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
                    }
                },
                smsgs => msgs.AddRange(smsgs),
                fmsgs => msgs.AddRange(fmsgs),
                emsgs => Utilities.SystemErrorLog(emsgs),
                dc
            );
        }

        public static FigureOutProratingResult FigureOutProrating(this SubscriptionPlan plan, DateTime todayUtc, IStripeProratingService stripeProratingService,
            ParticipationObject participationObject, DateTime startDateTime, Account currentAccount, SystemMessages msgs)
        {
            var result = new FigureOutProratingResult
            {
                ShouldWeBeProratingToday = false,
                ProratingValue = null,
                BackdateStartDate = null,
                BillingCycleAnchor = GetBillingCycleAnchor(participationObject, plan, startDateTime, todayUtc),
            };

            if (plan.IsEmpty())
                 return result;
            
            plan.StartDate ??= participationObject.StartDate;
            var planIsSubscription = plan.PlanTypeId == SubscriptionPlanType.Subscription;
            var activityStartDateIsInFutureButNotOnGoing = !participationObject.StartDate.HasValue ||
                                                           (!participationObject.StartDate.Value.IsBefore(todayUtc) && !participationObject.Schedule.IsOngoing);
            var subscriptionSetOnCheckout = plan.PaymentStartTypeId == PlanPaymentStartType.OnCheckout;
            var planStartsInFuture = !plan.StartDate.HasValue || !plan.StartDate.Value.IsBefore(todayUtc);
            if (!planIsSubscription || participationObject.Schedule.IsEmpty() || activityStartDateIsInFutureButNotOnGoing ||
                subscriptionSetOnCheckout || planStartsInFuture)
                return result;

            result.ProratingValue = stripeProratingService.GetProratingValue(currentAccount, plan, msgs);
            result.ShouldWeBeProratingToday = true;
            result.BackdateStartDate = todayUtc;
            result.BillingCycleAnchor = startDateTime.Date;
            return result;
        }


        private static DateTime? GetBillingCycleAnchor(ParticipationObject participationObject, SubscriptionPlan plan, DateTime startDateTime,
            DateTime todayUtc)
        {
            if (plan.IsEmpty() || plan.PlanTypeId != SubscriptionPlanType.Subscription || participationObject.Schedule.IsNotEmpty() ||
                todayUtc == todayUtc.FirstDateOfMonth().Date)
                return null;

            var billingCycleAnchor = plan.BillingPeriodTypeId switch
            {
                BillingPeriodType.Month => startDateTime.LastDateOfMonth().AddDays(1),
                BillingPeriodType.Week => startDateTime.LastDateOfWeek().AddDays(1),
                _ => (DateTime?)null,
            };
            return billingCycleAnchor;
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
                    endDate = plan.GetPaymentPlanEndDate(startDateTime, msgs);
            }

            // make sure endDate > startDate for stripe
            if (endDate.HasValue && Math.Abs((endDate.Value - startDateTime).TotalDays) < 1)
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
                    endDate = plan.GetPaymentPlanEndDate(startDateTime, msgs);
            }

            // make sure endDate > startDate for stripe
            if (endDate.HasValue && Math.Abs((endDate.Value - startDateTime).TotalDays) < 1)
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

        private static DateTime? GetPaymentPlanEndDate(this SubscriptionPlan plan, DateTime startDateTime, SystemMessages msgs = null)
        {
            if (plan.PlanTypeId != SubscriptionPlanType.PaymentPlan)
                return null;

            var endDate = startDateTime;

            if (plan.TotalPeriods.IsEmpty())
                return endDate;

            var periodsToAdd = plan.TotalPeriods.IfEmpty(0) - plan.NumberOfPeriodsPrePaid.IfEmpty(0);
            if (periodsToAdd > 0)
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

            return endDate;
        }

        public class FigureOutProratingResult
        {
            public bool ShouldWeBeProratingToday { get; set; }
            public decimal? ProratingValue { get; set; }
            public DateTime? BackdateStartDate { get; set; }
            public DateTime? BillingCycleAnchor { get; set; }
        }
    }
}