using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using Twilio.Rest.Api.V2010.Account;
using Account = AfterSchoolLibrary.Models.Account;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantService
    {
        public static WebUser MatchCreateOrUpdateUser(this Registrant registrant, string firstname, string lastname, string email, string password = null,
            bool isAfterSchool = false, bool forceMatch = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // connecting with an existing account that has a password
            // connecting with an existing account that doesn't have a password
            //  and you aren't trying to create an official account
            //  and you are trying to create an official account
            // creating a new account

            var creatingNewAccount = password.IsNotEmpty();

            if (WebUser.ExistsByEmail(email, msgs, dc))
            {
                var user = WebUser.LoadByEmail(email, msgs, dc);
                if (!creatingNewAccount)
                {
                    // not creating an account, current account has no password
                    if (user.Password.IsEmpty() || forceMatch)
                        return user;

                    msgs.Add("You already have an account with this email. Login to register.", MessageType.UserError);
                    return null;
                }

                // if user already has a password and user is trying to create a new account.
                if (user.Password.IsNotEmpty())
                {
                    msgs.Add("You already have an account with this email. Login to register.", MessageType.UserError);
                    return null;
                }
            }

            return CreateOrUpdateUser(registrant, firstname, lastname, email, password, isAfterSchool, msgs, dc: dc);
        }

        /// <summary>
        ///     TODO: Refactor and simplify
        /// </summary>
        /// <param name="registrant"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="isAfterSchool"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        private static WebUser CreateOrUpdateUser(Registrant registrant, string firstname, string lastname, string email, string password = null,
            bool isAfterSchool = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            WebUser user = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (WebUser.ExistsByEmail(email, amsgs, adc))
                    {
                        user = WebUser.LoadByEmail(email, amsgs, adc);

                        if (registrant.WebUserId.HasValue && user.ID != registrant.WebUserId)
                            amsgs.Add("This email is already registered to another account", MessageType.UserError);
                    }

                    if (amsgs.HasNoErrors())
                    {
                        if (user.IsEmpty())
                        {
                            user = new WebUser
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = 0,
                                Email = email.MaxChars(150),
                                VerifiedAccount = false,
                                IsActive = false,
                                IsRegistered = false,
                            };

                            user.Username = WebUserService.GenerateUsername(user.FirstName, user.LastName, amsgs, adc);
                        }

                        user.FirstName = firstname.MaxChars(50);
                        user.LastName = lastname.MaxChars(50);
                        user.LastModifiedDate = DateTime.UtcNow;
                        user.LastModifiedBy = registrant.WebUserId;

                        if (password.IsNotBlank())
                        {
                            user.IsActive = true;
                            user.IsRegistered = true;
                            user.Password = WebUserService.EncryptPassword(password);
                        }

                        msgs.AddRange(user.Save(adc));

                        registrant.WebUserId = user.ID;
                        msgs.AddRange(registrant.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.Add("Your account has been created", MessageType.Success);

                    // verify if there is a password and the user hasn't changed their email
                    if (password.IsNotBlank() && user.Email.ToLower() != registrant.Email.ToLower())
                        user.SendVerificationEmail();
                },
                onFailure: (SystemMessages fmsgs) => { msgs.Add("Error creating account", MessageType.Error); },
                dc: dc
            );

            return user;
        }

        private static (Customer customer, string sourceId) GetCustomerAndSourceForSubscription(CheckoutObject checkoutObject, SystemMessages msgs)
        {
            if (checkoutObject.StripeSourceId.IsEmpty())
                checkoutObject.LoadStripeSource(StripeAccountType.ConnectAccount, msgs);

            return (checkoutObject.PayingAccount.GetStripeCustomer(StripeAccountType.ConnectAccount, msgs), checkoutObject.StripeSourceId);
        }

        public static void StartPaymentPlan(this Registrant submittedRecord, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            submittedRecord.StartPlan(checkoutObject, SubscriptionPlanType.PaymentPlan, loggedInUser, msgs, dc);
        }

        public static void StartSubscriptionPlan(this Registrant submittedRecord, CheckoutObject checkoutObject, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            submittedRecord.StartPlan(checkoutObject, SubscriptionPlanType.Subscription, loggedInUser, msgs, dc);
        }

        public static int GetSubscriptionCount(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var plan = registrant.GetRegistrantSubscriptionPlans(msgs, dc)
                .Where(rsp => !rsp.DateCancelled.HasValue && rsp.IsForParticipationObject(participationObject, msgs, dc));
            if (plan.IsEmpty())
                return 0;

            var planMain = plan.FirstOrDefault();
            var payments = planMain.IsNotEmpty() ? planMain.GetPayments(msgs, dc) : null;
            if (planMain.IsNotEmpty() && planMain.TotalPeriods.HasValue)
                return planMain.TotalPeriods.Value - payments.Count;

            return payments.IsNotEmpty() ? payments.Count : 0;
        }

        private static void StartPlan(this Registrant submittedRecord, CheckoutObject checkoutObject, SubscriptionPlanType subscriptionPlanType,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs ??= new SystemMessages();
            var plan = subscriptionPlanType == SubscriptionPlanType.PaymentPlan ? checkoutObject.PaymentPlan : checkoutObject.SubscriptionPlan;

            var participationObject = checkoutObject.ParticipationObject;
            var billingAccount = checkoutObject.BillingAccount;
            var purchasingAccount = checkoutObject.PayingAccount;
            var useStripeConnectedAccount = billingAccount.HasAccountPackageFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);
            var (customer, sourceId) = GetCustomerAndSourceForSubscription(checkoutObject, msgs);
            var startDateTime = submittedRecord.GetPlanStartDateUtc(plan, checkoutObject.ParticipationObject, msgs, dc);
            var endDateTime = plan.GetPlanEndDateForRegistrantSubscriptionUtc(startDateTime, checkoutObject.ParticipationObject, msgs, dc);
            var stripeMetaData = new Dictionary<string, string>
                            {
                                {"BillingAccountUid", billingAccount.Uid.ToString()},
                                {"BillingAccountName", billingAccount.Name},
                                {"PayingAccountUid", purchasingAccount.Uid.ToString()},
                                {"PayingAccountName", purchasingAccount.Name},
                                {"ParticipationObjectType", checkoutObject.ParticipationObject.ObjectType.ToString()},
                                {"ParticipationObjectName", checkoutObject.ParticipationObject.Name},
                                {"SourceId", sourceId},
                                {"ApprovedBy", loggedInUser.Uid.ToString()},
                                {"DateRequested", DateTime.UtcNow.ToShortDateTimeString()},
                            };

            var platformFeePercent = useStripeConnectedAccount
                                        ? billingAccount.GetSubscriptionPlatformFeePercent(plan, checkoutObject.PaymentType, msgs, dc)
                                        : new decimal?();
            var stripeAccountId = useStripeConnectedAccount ? billingAccount.StripeUserId : null;
            var stripeCouponId = GetACHCoupon(checkoutObject, billingAccount, plan, null, msgs, dc);
            var anchorDate = new DateTime?();
            var proratingCalculation = new ProratingCalculationHelper(plan, participationObject, billingAccount, checkoutObject.PaymentType, msgs, dc);

            if (proratingCalculation.GetIsProrated())
            {
                stripeMetaData.Add("Prorated Charge With CC Fee", proratingCalculation.GetPredictedStripeCharge().ToCurrency());
                stripeMetaData.Add("Prorated CC Fee", proratingCalculation.GetProratedCCFee().ToCurrency());
                stripeMetaData.Add("Prorated Amount Without CC Fee", proratingCalculation.GetProratedAmountWithoutCCFee().ToCurrency());
                stripeMetaData.Add("Prorated Convenience Fee", proratingCalculation.GetProratedConvenienceFee().ToCurrency());
                stripeMetaData.Add("Prorated Amount Without Fees", proratingCalculation.GetProratedAmountWithoutFees().ToCurrency());

                startDateTime = proratingCalculation.GetStartDate();
                endDateTime = proratingCalculation.GetEndDate();
                anchorDate = proratingCalculation.GetAnchorDate();

                // can't set during proration since it is permanently assigned to subscription
                // have to keep the same platform percent determined for the full price
                // which means we may gain or lose money
                // platformFeePercent = proratingCalculation.GetPlatformFeePercent();

                // coupons don't apply to prorated charges in stripe, so have to use balance transaction
                if (msgs.HasNoErrors())
                    proratingCalculation.ProcessCustomerBalanceTransactionAsCoupon(checkoutObject.PaymentType, customer, msgs);
            }

            // create the subscription on the stripe account (could be added to main account or connected account)
            var subscription = StripeHelper.GetStripeHelperConnectAccount().CreateRegistrantSubscription(
                customerId: customer.Id,
                planId: plan.StripePlanId,
                isProrated: proratingCalculation.GetIsProrated(),
                sourceId: sourceId,
                platformFeePercent: platformFeePercent,
                stripeAccountId: stripeAccountId,
                startDate: startDateTime,
                endDate: endDateTime,
                billingCycleAnchor: anchorDate,
                stripeCouponId: stripeCouponId,
                metaData: stripeMetaData,
                msgs: msgs);

            if (msgs.HasErrors())
                return;

            var purchase = checkoutObject.CreateOrUpdatePurchase(loggedInUser, msgs, dc);
            var feeObject = GetFeeObject(checkoutObject, billingAccount, plan, msgs, dc);

            checkoutObject.RegistrantSubscriptionPlan = plan.AddRegistrant(
                submittedRecord,
                startDateTime,
                purchase,
                subscription.Id,
                feeObject,
                loggedInUser,
                msgs,
                dc);

            checkoutObject.ProratingCalculation = proratingCalculation;
        }

        /// <summary>
        ///     Give the purchaser a discount off of the subscription if they are purchasing with ACH, since the subscription is stored with the CC fee
        ///     And ACH checkouts shouldn't charge the extra CC fee
        /// </summary>
        /// <param name="checkoutObject"></param>
        /// <param name="billingAccount"></param>
        /// <param name="plan"></param>
        /// <param name="stripeAccountId"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        private static string GetACHCoupon(CheckoutObject checkoutObject, Account billingAccount, SubscriptionPlan plan, string stripeAccountId,
            SystemMessages msgs, DatabaseConnection dc)
        {
            var ccFee = plan.GetCreditCardFee(billingAccount, msgs, dc);
            var totalCost = plan.GetTotalCostWithFees(billingAccount, msgs, dc);

            return GetACHCoupon(checkoutObject, plan, totalCost, ccFee, stripeAccountId, msgs, dc);
        }

        private static string GetACHCoupon(CheckoutObject checkoutObject, SubscriptionPlan plan, decimal totalCost, decimal ccFee, string stripeAccountId,
            SystemMessages msgs, DatabaseConnection dc)
        {
            if (checkoutObject.PaymentType == PaymentType.ACH)
            {
                //need to add a coupon to subtract the credit card amount that is stored in the payment plan
                var coupon = new StripeHelper().CreateCoupon(
                    couponName: $"{plan.StripePlanId}/{totalCost.ToCurrency()}-{ccFee.ToCurrency()}/ACH",
                    discountValue: ccFee,
                    couponDuration: StripeCouponDuration.Forever,
                    durationInMonths: null,
                    couponAmountType: CouponAmountType.FlatAmount,
                    stripeAccountId: stripeAccountId,
                    msgs: msgs);

                return coupon.Id;
            }

            return null;
        }

        public static bool HasActiveSubscription(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return registrant.GetActiveSubscription(participationObject, msgs, dc).IsNotEmpty();
        }

        public static RegistrantSubscriptionPlan GetActiveSubscription(this Registrant registrant, ParticipationObject participationObject,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrant
                .GetRegistrantSubscriptionPlans(msgs, dc)
                .Where(rsp => !rsp.DateCancelled.HasValue && rsp.IsForParticipationObject(participationObject, msgs, dc))
                .ToList()
                .FirstOrDefault();
        }

        public static int GetAllActiveSubscriptionCount(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return registrant
                .GetRegistrantSubscriptionPlans(msgs, dc)
                .Where(rsp => !rsp.DateCancelled.HasValue && rsp.IsForParticipationObject(participationObject, msgs, dc))
                .Count();
        }

        public static bool HasCancelledSubscription(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return registrant
                .GetCancelledRegistrantSubscriptionPlans(participationObject, msgs, dc)
                .Any();
        }

        public static List<RegistrantSubscriptionPlan> GetCancelledRegistrantSubscriptionPlans(this Registrant registrant,
            ParticipationObject participationObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return registrant
                .GetRegistrantSubscriptionPlans(msgs, dc)
                .Where(rsp => rsp.DateCancelled.HasValue && rsp.IsForParticipationObject(participationObject, msgs, dc))
                .ToList();
        }

        public static bool HasUnexpiredCancelledSubscription(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return registrant
                .GetUnexpiredCancelledRegistrantSubscriptionPlans(participationObject, msgs, dc)
                .Any();
        }

        public static DateTime? GetUnexpiredCancelledSubscriptionExpireDate(this Registrant registrant, ParticipationObject participationObject,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            return registrant
                .GetUnexpiredCancelledRegistrantSubscriptionPlans(participationObject, msgs, dc)
                .OrderByDescending(rsp => rsp.ValidUntil)
                .FirstOrDefault()?
                .ValidUntil.Value;
        }

        public static List<RegistrantSubscriptionPlan> GetUnexpiredCancelledRegistrantSubscriptionPlans(this Registrant registrant,
            ParticipationObject participationObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return registrant
                .GetRegistrantSubscriptionPlans(msgs, dc)
                .Where(rsp =>
                    rsp.DateCancelled.HasValue &&
                    rsp.ValidUntil.HasValue &&
                    rsp.ValidUntil.Value.IsOnOrAfter(DateTime.UtcNow) &&
                    rsp.IsForParticipationObject(participationObject, msgs, dc))
                .OrderBy(rsp => rsp.ValidUntil)
                .ToList();
        }

        /// <summary>
        ///     should be used for returns to stripe or other services that require UTC
        /// </summary>
        /// <param name="registrant"></param>
        /// <param name="plan"></param>
        /// <param name="participationObject"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static DateTime GetPlanStartDateUtc(this Registrant registrant, SubscriptionPlan plan, ParticipationObject participationObject,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var billingAccount = plan.GetAccount(msgs, dc);
            var planStartDate = registrant.GetPlanStartDate(plan, participationObject, msgs, dc);
            return billingAccount.ConvertFromAccountTimeZone(planStartDate);
        }

        public static DateTime GetPlanStartDate(this Registrant registrant, SubscriptionPlan plan, ParticipationObject participationObject,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var billingAccount = plan.GetAccount(msgs, dc);
            var unexpiredCancelledRegistrantSubscriptionPlans = registrant.GetUnexpiredCancelledRegistrantSubscriptionPlans(participationObject, msgs, dc);
            return GetPlanStartDate(plan, participationObject, billingAccount, unexpiredCancelledRegistrantSubscriptionPlans);
        }

        internal static DateTime GetPlanStartDate(SubscriptionPlan plan, ParticipationObject participationObject, Account billingAccount,
            IReadOnlyCollection<RegistrantSubscriptionPlan> unexpiredCancelledRegistrantSubscriptionPlans)
        {
            // start dates are in the provider's timezone, so when comparing dates, we need to start there
            var today = billingAccount.ConvertToAccountTimeZone(DateTime.UtcNow);

            switch (plan.PlanTypeId, plan.PaymentStartTypeId)
            {
                case (SubscriptionPlanType.Subscription, PlanPaymentStartType.WhenActivityStarts):
                    // find existing canceled plans and make new plan start the day after it ends.
                    // for a person restarting a plan after cancellation
                    if (unexpiredCancelledRegistrantSubscriptionPlans.Any(rsp => rsp.ValidUntil.HasValue))
                        return unexpiredCancelledRegistrantSubscriptionPlans.Last(rsp => rsp.ValidUntil.HasValue).ValidUntil!.Value
                            .AddDays(1); // need to double check this
                    if (participationObject.StartDate.HasValue && participationObject.StartDate.Value.Date.IsAfter(today.Date))
                        return participationObject.StartDate.Value;
                    break;
                case (SubscriptionPlanType.Subscription, PlanPaymentStartType.OnCheckout):  
                    // if on checkout, start on checkout
                    return today;
                    break;
                case (SubscriptionPlanType.Subscription, PlanPaymentStartType.OnPeriodStart):
                    if (plan.StartDate.HasValue && plan.StartDate.Value.Date.IsAfter(today))
                        return plan.StartDate.Value;
                    break;
                case (SubscriptionPlanType.PaymentPlan, PlanPaymentStartType.WhenActivityStarts):
                    if (participationObject.StartDate.HasValue && participationObject.StartDate.Value.Date.IsAfter(today.Date))
                        return participationObject.StartDate.Value;
                    break;
            }

            // convert back to utc because dates may be transferred to other methods that expect utc, such as stripe, etc.
            return today;
        }

        private static PlanFeeObject GetFeeObject(CheckoutObject checkoutObject, Account billingAccount, SubscriptionPlan plan, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            decimal? convenienceFee = 0;
            decimal? paymentTypeFee = 0;
            decimal? amount = 0;
            int? totalPeriods = null;

            if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan)
            {
                convenienceFee = checkoutObject.GetPaymentPlanCostTodayConvenienceFee(msgs, dc);
                paymentTypeFee = checkoutObject.GetPaymentPlanCostTodayCreditCardFee(msgs, dc);
                amount = checkoutObject.PaymentPlan.CostPerPeriod;
                totalPeriods = checkoutObject.PaymentPlan.TotalPeriods;
            }
            else
            {
                convenienceFee = checkoutObject.GetSubscriptionCostConvenienceFee(msgs, dc);
                paymentTypeFee = checkoutObject.GetSubscriptionCostCreditCardFee(msgs, dc);
                amount = checkoutObject.SubscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc);

                var po = checkoutObject.ParticipationObject;

                if (po.Schedule.IsNotEmpty() && !po.Schedule.IsOngoing && po.Schedule.EndDate.HasValue)
                {
                    var endDate = po.Schedule.EndDate.Value;
                    var submittedRecord = checkoutObject.Registration.GetRegistrant(msgs, dc);
                    var startDate = submittedRecord.GetPlanStartDate(plan, po, msgs, dc);

                    switch (plan.BillingPeriodTypeId)
                    {
                        case BillingPeriodType.Day:
                            totalPeriods = Math.Abs(Math.Ceiling(startDate.DateDiff(endDate, "d", true))).ToInteger();
                            break;
                        case BillingPeriodType.Month:
                            totalPeriods = Math.Abs(Math.Ceiling(startDate.DateDiff(endDate, "m", true))).ToInteger();
                            break;
                        case BillingPeriodType.TwoWeeks:
                            totalPeriods = Math.Abs(Math.Ceiling(startDate.DateDiff(endDate, "w", true) / 2)).ToInteger();
                            break;
                        case BillingPeriodType.Week:
                            totalPeriods = Math.Abs(Math.Ceiling(startDate.DateDiff(endDate, "w", true))).ToInteger();
                            break;
                        case BillingPeriodType.Year:
                            totalPeriods = Math.Abs(Math.Ceiling(startDate.DateDiff(endDate, "y", true))).ToInteger();
                            break;
                    }
                }
            }

            return new PlanFeeObject()
            {
                ConvenienceFee = convenienceFee.Value,
                ConvenienceFeesPaidBy = plan.IsConvenienceFeeTakenOutOfCost ? FeesPaidByType.Account : FeesPaidByType.Parent,
                PaymentTypeFee = paymentTypeFee.Value,
                PaymentTypeFeesPaidBy = billingAccount.CreditCardFeesPaidBy,
                Amount = amount.Value,
                TotalPeriods = totalPeriods,
                PaymentType = checkoutObject.PaymentType,
                BillingPeriodType = plan.BillingPeriodTypeId,
            };
        }

        public static string GetParentOrGuardianEmail(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrant.ParentRegistrantId.HasValue)
                return registrant.GetMasterRegistrant(msgs, dc).GetParentOrGuardian(msgs, dc).Email;

            if (registrant.IsMaster)
                return registrant.GetParentOrGuardian(msgs, dc).Email;

            return null;
        }

        public static bool HasReceivedTextMessages(this Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return TwilioSMS.GetCount(
                criterias: new Hashtable() {{"RegistrantId", registrant.ID.Value}},
                msgs: msgs,
                dc: dc) > 0;
        }

        internal static void SendText(this Registrant registrant, Account sendingAccount, WebUser loggedInUser, ActivityMessage message, string activityName,
            string subject, string body, string senderName, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (!registrant.AllowTextMessages)
                msgs.Add($"{registrant.DisplayName} does not allow text messaging", MessageType.Notice);
            else if (registrant.Phone.IsEmpty())
                msgs.Add($"{registrant.DisplayName} has no phone number available", MessageType.Notice);

            if (registrant.Phone.IsNotEmpty() && registrant.AllowTextMessages)
            {
                if (!sendingAccount.IsActive)
                {
                    msgs.Add("Account is not active and cannot send text messages", MessageType.UserError);
                    return;
                }

                // if phone number is on the block list
                if (TwilioBlockedPhone.ExistsByPhone(registrant.Phone, msgs, dc))
                    return;

                // if person has already received this text message
                if (TwilioSMS.ExistsByRegistrantIdByActivityMessageId(registrant.ID.Value, message.ID.Value, msgs, dc))
                    return;

                // if user hasn't received a text from us yet, need to send standard text info
                if (!registrant.HasReceivedTextMessages(msgs, dc))
                    TwilioHelper.SendSMS(
                        registrant.Phone,
                        $"Welcome to {Utilities.AppName()} Alerts! You are receiving this message because you enabled text notifications when you registered your child for activities. \nMsg&data rates may apply. \nReply HELP for help, STOP to cancel",
                        null,
                        null,
                        msgs);

                var resource = ActivityMessageService.SendTextMessage(
                    registrant.Phone,
                    senderName,
                    subject,
                    activityName,
                    message.Uid.ToString(),
                    registrant.Uid.ToString(),
                    msgs);

                if (resource.IsEmpty())
                    return;

                if (resource.ErrorCode.HasValue)
                    registrant.AddPhoneToTwilioBlockedList(resource, loggedInUser, msgs, dc);
                else if (msgs.HasNoErrors())
                    registrant.SaveTwilioSms(resource, message, sendingAccount, loggedInUser, msgs, dc);
            }
        }

        private static void SaveTwilioSms(this Registrant registrant, MessageResource resource, ActivityMessage message, Account sendingAccount,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var twilioSms = new TwilioSMS()
            {
                TwilioResourceId = resource.Sid,
                TwilioStatus = resource.Status.ToString(),
                ActivityMessageId = message.ID,
                RegistrantId = registrant.ID,
                SenderId = loggedInUser.ID,
                SendingAccountId = sendingAccount.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser.ID,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser.ID,
            };
            msgs.AddRange(twilioSms.Save(dc));
        }

        public static void AddPhoneToTwilioBlockedList(this Registrant registrant, MessageResource resource, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            // add to block list
            switch (resource.ErrorCode.Value)
            {
                case 30004:
                case 30005:
                case 30006:

                    registrant.AllowTextMessages = false;
                    msgs.AddRange(registrant.Save(dc));

                    var tbp = new TwilioBlockedPhone()
                    {
                        Phone = registrant.Phone,
                        ErrorCode = resource.ErrorCode,
                        ErrorMessage = resource.ErrorMessage.MaxChars(100),
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    };
                    msgs.AddRange(tbp.Save(dc));

                    break;
            }
        }

        internal static void SendEmail(this Registrant registrant, Account account, string activityName, string fromName, string replyToEmail,
            string replyToName, string subject, string body, string utmCampaign = "", SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            if (registrant.Email.IsEmpty())
                msgs.Add($"No email address available for {registrant.Name}", MessageType.Notice);

            if (registrant.Email.IsNotEmpty())
                ActivityMessageService.SendEmailMessage(
                    account,
                    activityName,
                    fromName,
                    registrant.Email,
                    registrant.DisplayName,
                    replyToEmail,
                    replyToName,
                    subject,
                    body,
                    utmCampaign,
                    msgs);
        }

        public static List<ParticipationObject> GetParticipationObjects(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(registrant.ID.Value, msgs, dc)
                .Select(x => x.GetParticipationObject(msgs, dc))
                .ToList();
        }

        public static void AddUsersWithAccess(this Registrant registrant, List<WebUser> users, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            users.ForEach(user => registrant.AddUserWithAccess(user, loggedInUser, msgs, dc));
        }

        public static void AddUserWithAccess(this Registrant registrant, WebUser user, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (user.IsNotEmpty())
            {
                var registrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(registrant.ID.Value, user.ID.Value, msgs, dc);

                if (registrantWebUser.IsEmpty())
                {
                    registrantWebUser = new RegistrantWebUser()
                    {
                        RegistrantId = registrant.ID.Value,
                        WebUserId = user.ID.Value,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    };
                    msgs.AddRange(registrantWebUser.Save(dc));
                }
            }
        }

        public static WebUser GetWebUser(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrant.WebUserId.HasValue)
                return WebUser.Load(registrant.WebUserId.Value, msgs, dc);

            return null;
        }

        public static void SetProfileAsCompleted(this Registrant registrant, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            registrant.UpdateProfileStatus(RegistrantProfileStatus.Completed, loggedInUser, msgs, dc);
        }

        public static void UpdateProfileStatus(this Registrant registrant, RegistrantProfileStatus profileStatus, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            registrant.ProfileStatusId = profileStatus;
            registrant.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            registrant.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(registrant.Save(dc));
        }

        private static bool HasParticipationAssignment(this Registrant masterRegistrant, ParticipationObject participationObject,
            List<RegistrationAssignmentType> assignmentTypes, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc)
                    .HasParticipationAssignment(participationObject, assignmentTypes, hasParentalConsent, msgs, dc);

            var submittedRecords = masterRegistrant.GetSubmittedRecords(participationObject.OwnerAccount.ID.Value, msgs, dc);

            return submittedRecords.Any() && RegistrantActivity.ExistsByParticipationObjectByRegistrants(
                participationObjectId: participationObject.ObjectID.Value,
                participationObjectType: participationObject.ObjectType,
                registrantIds: submittedRecords.Select(item => item.ID.Value).ToList(),
                assignmentTypes: assignmentTypes,
                hasParentalConsent: hasParentalConsent,
                msgs: msgs,
                dc: dc);
        }

        public static bool MasterRegistrantHasPlan(this Registrant masterRegistrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).MasterRegistrantHasPlan(participationObject, msgs, dc);

            return masterRegistrant.MasterRegistrantHasPlan(
                participationObject.OwnerAccount.ID.Value,
                participationObject.ObjectID.Value,
                participationObject.ObjectType,
                msgs,
                dc);
        }

        public static bool MasterRegistrantHasPlan(this Registrant masterRegistrant, int billingAccountId, int participationObjectId,
            ParticipationObjectType participationObjectType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc)
                    .MasterRegistrantHasPlan(billingAccountId, participationObjectId, participationObjectType, msgs, dc);

            var submittedRecords = masterRegistrant.GetSubmittedRecords(billingAccountId, msgs, dc);

            return submittedRecords.Any() && RegistrantSubscriptionPlan.ExistsByParticipationObjectByRegistrants(
                participationObjectId: participationObjectId,
                participationObjectType: participationObjectType,
                registrantIds: submittedRecords.Select(item => item.ID.Value).ToList(),
                msgs: msgs,
                dc: dc);
        }

        public static bool HasPlan(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (registrant.IsMaster)
                return registrant.MasterRegistrantHasPlan(participationObject, msgs, dc);

            return registrant.HasPlan(participationObject.ObjectID.Value, participationObject.ObjectType, msgs, dc);
        }

        public static bool HasPlan(this Registrant registrant, int participationObjectId, ParticipationObjectType participationObjectType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrant.IsMaster)
                return registrant.MasterRegistrantHasPlan(new ParticipationObject(participationObjectId, participationObjectType, msgs, dc), msgs, dc);

            return RegistrantSubscriptionPlan.ExistsByParticipationObjectByRegistrants(
                participationObjectId: participationObjectId,
                participationObjectType: participationObjectType,
                registrantIds: registrant.ID.Value.AsList(),
                msgs: msgs,
                dc: dc);
        }

        public static List<RegistrantSubscriptionPlan> GetMasterRegistrantSubscriptionPlans(this Registrant registrant, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var submittedRecords = registrant.GetSubmittedRecords(msgs, dc);
            var plans = new List<RegistrantSubscriptionPlan>();

            submittedRecords.ForEach(submittedRecord => { plans.AddRange(submittedRecord.GetRegistrantSubscriptionPlans(msgs, dc)); });

            return plans;
        }

        public static List<RegistrantSubscriptionPlan> GetRegistrantSubscriptionPlans(this Registrant registrant, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (registrant.IsMaster)
                return registrant.GetMasterRegistrantSubscriptionPlans(msgs, dc);

            return RegistrantSubscriptionPlan.LoadByRegistrantId(registrant.ID.Value, msgs, dc);
        }

        public static bool CancelSubcriptionIfExists(this Registrant registrant, int participationObjectId, ParticipationObjectType participationObjectType,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // TODO: Has to be a better way
            if (!registrant.HasPlan(participationObjectId, participationObjectType, msgs, dc))
                return false;

            var rsps = registrant.GetRegistrantSubscriptionPlans(msgs, dc);
            foreach (var rsp in rsps.Where(rsp => !rsp.DateCancelled.HasValue))
            {
                var plan = rsp.GetSubscriptionPlan(msgs, dc);
                var isSubscription = plan.PlanTypeId == SubscriptionPlanType.Subscription;

                if (plan.ParticipationObjectId == participationObjectId && plan.ParticipationObjectTypeId == participationObjectType && isSubscription)
                    return plan.CancelSubscription(rsp, plan.GetAccount(msgs, dc), loggedInUser, msgs, dc);
            }

            return false;
        }

        public static bool IsAppliedFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).IsAppliedFor(participationObject, hasParentalConsent, msgs, dc);

            return HasParticipationAssignment(masterRegistrant, participationObject, RegistrationAssignmentType.Applied.AsList(), hasParentalConsent, msgs, dc);
        }

        public static bool HasConsentFor(this Registrant masterRegistrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).HasConsentFor(participationObject, msgs, dc);

            return HasParticipationAssignment(masterRegistrant, participationObject, null, true, msgs, dc);
        }

        public static bool OnWaitingListFor(this Registrant masterRegistrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).OnWaitingListFor(participationObject, msgs, dc);

            return HasParticipationAssignment(
                masterRegistrant,
                participationObject,
                RegistrationAssignmentType.WaitingList.AsList(),
                true,
                msgs,
                dc);
        }

        public static bool IsReservedFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).IsReservedFor(participationObject, hasParentalConsent, msgs, dc);

            var assignmentTypes = new List<RegistrationAssignmentType>()
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
            };

            return HasParticipationAssignment(masterRegistrant, participationObject, assignmentTypes, hasParentalConsent, msgs, dc);
        }

        public static bool IsRegisteredFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).IsReservedFor(participationObject, hasParentalConsent, msgs, dc);

            var assignmentTypes = new List<RegistrationAssignmentType>()
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
                RegistrationAssignmentType.Applied,
                RegistrationAssignmentType.WaitingList,
            };

            return HasParticipationAssignment(masterRegistrant, participationObject, assignmentTypes, hasParentalConsent, msgs, dc);
        }

        public static bool IsReservedForAllDates(this Registrant masterRegistrant, ParticipationObject participationObject, List<DateTime> dates,
            bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).IsReservedForAllDates(participationObject, dates, hasParentalConsent, msgs, dc);

            var assignmentTypes = new List<RegistrationAssignmentType>()
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
            };

            var hasAssignment = HasParticipationAssignment(masterRegistrant, participationObject, assignmentTypes, hasParentalConsent, msgs, dc);

            var submittedRecords = masterRegistrant.GetSubmittedRecords(participationObject.OwnerAccount.ID.Value, msgs, dc);
            var counter = 0;

            submittedRecords.ForEach(submittedRecord =>
            {
                var ra = submittedRecord.GetRegistrantActivity(participationObject, msgs, dc);

                if (ra.IsNotEmpty())
                    dates.ForEach(date =>
                    {
                        var instance = participationObject.Schedule.GetInSessionInstanceByDate(date, msgs, dc);

                        if (instance.IsNotEmpty())
                        {
                            var ais = ActivityInstanceSeats.LoadByRegistrantActivityIdByActivityInstanceId(ra.ID.Value, instance.ID.Value, msgs, dc);
                            if (ais.IsNotEmpty())
                                counter++;
                        }
                    });
            });

            return hasAssignment && counter == dates.Count;
        }

        public static bool IsReservedForDate(this Registrant masterRegistrant, ParticipationObject participationObject, DateTime date, bool? hasParentalConsent,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).IsReservedForDate(participationObject, date, hasParentalConsent, msgs, dc);

            var assignmentTypes = new List<RegistrationAssignmentType>()
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
            };

            var hasAssignment = HasParticipationAssignment(masterRegistrant, participationObject, assignmentTypes, hasParentalConsent, msgs, dc);

            var submittedRecords = masterRegistrant.GetSubmittedRecords(participationObject.OwnerAccount.ID.Value, msgs, dc);
            var counter = 0;

            submittedRecords.ForEach(submittedRecord =>
            {
                var ra = submittedRecord.GetRegistrantActivity(participationObject, msgs, dc);

                if (ra.IsNotEmpty())
                {
                    var instance = participationObject.Schedule.GetInSessionInstanceByDate(date, msgs, dc);

                    if (instance.IsNotEmpty())
                    {
                        var ais = ActivityInstanceSeats.LoadByRegistrantActivityIdByActivityInstanceId(ra.ID.Value, instance.ID.Value, msgs, dc);
                        if (ais.IsNotEmpty())
                            counter++;
                    }
                }
            });

            return hasAssignment && counter == 1;
        }

        public static bool IsReservedAndPaidFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).IsReservedAndPaidFor(participationObject, hasParentalConsent, msgs, dc);

            return HasParticipationAssignment(masterRegistrant, participationObject, RegistrationAssignmentType.Reserved.AsList(), hasParentalConsent, msgs,
                dc);
        }

        public static bool HasWithdrawn(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (registrant.IsMaster)
                return HasParticipationAssignment(registrant, participationObject, RegistrationAssignmentType.Withdrawn.AsList(), null, msgs, dc);

            var ra = registrant.GetRegistrantActivity(participationObject, msgs, dc);

            if (ra.IsNotEmpty())
                return ra.AssignmentTypeId == RegistrationAssignmentType.Withdrawn;

            return false;
        }

        public static bool HasSubmittedFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!masterRegistrant.IsMaster)
                return masterRegistrant.GetMasterRegistrant(msgs, dc).HasSubmittedFor(participationObject, hasParentalConsent, msgs, dc);

            var assignmentTypes = new List<RegistrationAssignmentType>()
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.Applied,
                RegistrationAssignmentType.WaitingList,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
                RegistrationAssignmentType.Withdrawn,
            };

            return HasParticipationAssignment(masterRegistrant, participationObject, assignmentTypes, hasParentalConsent, msgs, dc);
        }

        public static Account GetAccount(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrant.AccountId.HasValue ? Account.Load(registrant.AccountId.Value, msgs, dc) : null;
        }

        public static WebUser GetOwner(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrant.WebUserId.HasValue ? WebUser.Load(registrant.WebUserId.Value, msgs, dc) : null;
        }

        public static Location GetSchoolLocation(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrant.SchoolLocationId.HasValue ? Location.Load(registrant.SchoolLocationId.Value, msgs, dc) : null;
        }

        public static Registrant GetMasterRegistrant(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrant.IsMaster)
                return registrant;
            return registrant.ParentRegistrantId.HasValue ? Registrant.Load(registrant.ParentRegistrantId.Value, msgs, dc) : null;
        }

        public static Registrant SubmitRecordToAccount(this Registrant registrant, Account ownerAccount, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            Registrant copy = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    copy = registrant.Copy(loggedInUser, msgs, dc);
                    copy.AccountId = ownerAccount.ID;
                    copy.FirstRegistrantId = null;
                    copy.ParentRegistrantId = registrant.ID;
                    copy.IsContactOnly = false;
                    copy.IsMaster = false;
                    amsgs.AddRange(copy.Save(adc));

                    // need to relate registrants within the same account, specifically provider or school accounts
                    amsgs.AddRange(copy.FindAndSetFirstRegistrant(loggedInUser, adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return copy;
        }

        public static bool NeedsToRegister(this Registrant submittedRecord, Registration registration, ParticipationObject participationObject,
            List<DateTime> selectedDates, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (submittedRecord.IsEmpty())
                return true;

            if (registration.IsEmpty())
                return true;

            if (participationObject.RequiresParentalConsent && registration.ESignature.IsEmpty())
                return true;

            if (submittedRecord.HasWithdrawn(participationObject, msgs, dc))
                return true;

            if (participationObject.ShouldParentPay() && !submittedRecord.IsReservedAndPaidFor(participationObject, null, msgs, dc))
                return true;

            if (participationObject.CanRegisterPerClass && !submittedRecord.IsReservedForAllDates(participationObject, selectedDates, null, msgs, dc))
                return true;

            return false;
        }

        public static List<Registrant> GetSubmittedRecords(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByParentRegistrantId(registrant.ID.Value, msgs, dc);
        }

        public static List<Registrant> GetSubmittedRecords(this Registrant registrant, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByParentRegistrantIdByAccountId(registrant.ID.Value, accountId, msgs, dc);
        }

        public static (Registrant, Registration) GetSubmittedRecord(this Registrant registrant, ParticipationObject participationObject,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var submittedRecords = registrant.GetSubmittedRecords(msgs, dc);

            if (!submittedRecords.Any())
                return (null, null);

            var submittedRecordIds = submittedRecords.Select(child => child.ID.Value).ToList();

            var ras = RegistrantActivity.LoadByRegistrantIds(submittedRecordIds, msgs, dc);

            var submittedRecordRA = ras.FirstOrDefault(ra =>
                ra.ParticipationObjectId == participationObject.ObjectID && ra.ParticipationObjectTypeId == participationObject.ObjectType);

            if (submittedRecordRA.IsNotEmpty() && submittedRecordRA.RegistrationId.IsNotEmpty())
                return (
                    submittedRecords.First(child => child.ID == submittedRecordRA.RegistrantId),
                    Registration.Load(submittedRecordRA.RegistrationId.Value, msgs, dc)
                );

            return (null, null);
        }

        public static RegistrantActivity AddOrUpdateParticipationObject(this Registrant registrant, int registrationId, int participationObjectId,
            ParticipationObjectType participationObjectType, RegistrationAssignmentType assignmentType, int? registrationPeriodId, int? activityGroupId,
            decimal? amountCharged, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrant.AddOrUpdateParticipationObject(
                registrationId,
                participationObjectId,
                participationObjectType,
                assignmentType,
                1,
                registrationPeriodId,
                activityGroupId,
                amountCharged,
                loggedInUserId,
                msgs,
                dc);
        }

        public static RegistrantActivity AddOrUpdateParticipationObject(this Registrant registrant, int registrationId, int participationObjectId,
            ParticipationObjectType participationObjectType, RegistrationAssignmentType assignmentType, int preferenceNumber, int? registrationPeriodId,
            int? activityGroupId, decimal? amountCharged, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                participationObjectId,
                participationObjectType,
                registrant.ID.Value,
                msgs,
                dc);

            if (ra.IsEmpty())
                ra = new RegistrantActivity
                {
                    ParticipationObjectId = participationObjectId,
                    ParticipationObjectTypeId = participationObjectType,
                    RegistrantId = registrant.ID,
                    RegistrationId = registrationId,
                    ActivityGroupId = activityGroupId,
                    RegistrationPeriodId = registrationPeriodId,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId,
                    IsDeleted = false,
                };

            ra.LastModifiedDate = DateTime.UtcNow;
            ra.LastModifiedBy = loggedInUserId;
            ra.PreferenceNumber = preferenceNumber;

            if (amountCharged.HasValue)
                ra.AmountCharged = amountCharged;

            msgs.AddRange(ra.Save(dc));

            ra.ChangeSeatAssignment(assignmentType, new WebUser {ID = loggedInUserId}, msgs, dc);

            return ra;
        }

        public static FormQuestionAnswer MapAndAddAnswer(this Registrant registrant, List<FormQuestion> questions, Registrant fromRegistrant,
            QuestionDataType fromType, QuestionDataType toType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var originalAnswer = fromRegistrant.GetAnswer(fromType, msgs, dc);
            var destinationQuestion = questions.Find(question => question.QuestionDataTypeId == toType);
            if (destinationQuestion.IsNotEmpty() && originalAnswer.IsNotEmpty())
                return registrant.AddOrUpdateAnswer(originalAnswer.Text, destinationQuestion, loggedInUser, msgs, dc);

            return null;
        }

        private static string UploadFile(this Registrant registrant, Stream file, string filename, FormQuestion question, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var ah = Utilities.GetNewAmazonHelper();

            var regEx = new Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx,
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            var form = question.GetForm(msgs, dc);
            var account = registrant.GetAccount(msgs, dc);
            var accountFolder = $"{account.UniqueName}-{account.ID}";
            var formFolder = $"{form.Name.Replace(regEx, "")}-{form.ID}";
            var registrantFolder = $"{registrant.Name.Replace(regEx, "")}-{registrant.ID}";
            var amazonFilePath = $"{accountFolder}/{formFolder}/{registrantFolder}/{filename}".ToLower();

            msgs.AddRange(ah.AddFile(file, amazonFilePath));

            return amazonFilePath;
        }

        public static FormQuestionAnswer AddOrUpdateAnswer(this Registrant registrant, Stream file, string filename, FormQuestion question, int? registrationId,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
            {
                var amazonFilePath = registrant.UploadFile(file, filename, question, msgs, dc);

                if (msgs.HasNoErrors())
                    return registrant.AddOrUpdateAnswer(amazonFilePath, question, registrationId, loggedInUser, msgs, dc);
            }

            return null;
        }

        public static FormQuestionAnswer AddOrUpdateAnswer(this Registrant registrant, string answerText, FormQuestion question, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
                return registrant.AddOrUpdateAnswer(answerText, question, null, loggedInUser, msgs, dc);

            return null;
        }

        public static FormQuestionAnswer AddOrUpdateAnswer(this Registrant registrant, string answerText, FormQuestion question, int? registrationId,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
            {
                if (question.QuestionTypeId == QuestionType.CheckboxList && answerText.IsNotEmpty())
                    answerText = GetFormattedAnswerForCheckboxList(answerText);

                return registrant.AddOrUpdateAnswer(
                    answerText,
                    question.ID,
                    question.QuestionDataTypeId,
                    question.Text,
                    registrationId,
                    loggedInUser,
                    msgs,
                    dc);
            }

            return null;
        }

        public static FormQuestionAnswer AddOrUpdateAnswer(this Registrant registrant, string answerText, int? questionId, QuestionDataType questionDataType,
            string questionText, int? registrationId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            //check if answer for this one already exists, and where formquestion is null
            FormQuestionAnswer answer = null;

            if (questionDataType == QuestionDataType.NotSet)
            {
                if (registrationId.HasValue && questionId.HasValue && registrant.ID.HasValue)
                    answer = FormQuestionAnswer.LoadByRegistrantIdByFormQuestionIdByRegistrationId(
                        RegistrantId: registrant.ID.Value,
                        FormQuestionId: questionId.Value,
                        RegistrationId: registrationId.Value,
                        msgs: msgs,
                        dc: dc);
            }
            else
                answer = FormQuestionAnswer.LoadByRegistrantIdByQuestionDataTypeId(registrant.ID.Value, questionDataType, msgs, dc);

            if (answer.IsEmpty())
                answer = new FormQuestionAnswer
                {
                    RegistrantId = registrant.ID,
                    FormQuestionId = questionId,
                    QuestionDataTypeId = questionDataType,
                    QuestionText = questionText,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID,
                };

            answer.RegistrationId = registrationId;
            answer.Text = answerText;
            answer.LastModifiedDate = DateTime.UtcNow;
            answer.LastModifiedBy = loggedInUser?.ID;
            msgs.AddRange(answer.Save(dc));

            answer.UpdateRegistrantInfo(registrant, msgs, dc);

            return answer;
        }

        public static RegistrantActivity AddInviteToActivity(this Registrant registrant, int activityId, int loggedInUserId, int? activityGroupId,
            int? registrationPeriodId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                activityId,
                ParticipationObjectType.Activity,
                registrant.ID.Value,
                msgs,
                dc);

            if (ra.IsEmpty())
                ra = new RegistrantActivity
                {
                    ParticipationObjectId = activityId,
                    ParticipationObjectTypeId = ParticipationObjectType.Activity,
                    RegistrantId = registrant.ID,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId,
                    ActivityGroupId = activityGroupId,
                };

            ra.LastModifiedDate = DateTime.UtcNow;
            ra.LastModifiedBy = loggedInUserId;
            ra.RegistrationPeriodId = registrationPeriodId;
            ra.AssignmentTypeId = RegistrationAssignmentType.Invited;

            msgs.AddRange(ra.Save(dc: dc));

            return ra;
        }

        public static void AddInviteToActivities(this Registrant registrant, List<int> activityIds, int loggedInUserId, int? activityGroupId,
            int? registrationPeriodId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            activityIds.ForEach(activityId =>
                registrant.AddInviteToActivity(activityId, loggedInUserId, activityGroupId, registrationPeriodId, msgs, dc)
            );
        }

        public static Registrant CreateMasterCopy(this Registrant registrant, List<FormQuestion> questions, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var master = registrant.CopyWithAnswers(questions, loggedInUser, msgs, dc);
            master.IsMaster = true;
            master.IsContactOnly = true;
            master.AccountId = null;
            msgs.AddRange(master.Save(dc));

            registrant.ParentRegistrantId = master.ID;
            msgs.AddRange(registrant.Save(dc: dc));

            return master;
        }

        public static Registrant CopyWithAnswers(this Registrant registrant, List<FormQuestion> questions, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var copy = registrant.Copy(loggedInUser, msgs, dc);

            if (questions.IsEmpty())
                questions.ForEach(question =>
                    copy.AddOrUpdateAnswer(question.GetAnswer(registrant.ID.Value, msgs, dc).Text, question, null, loggedInUser, msgs, dc)
                );

            return copy;
        }

        public static void CopyAnswersTo(this Registrant registrant, Registrant copyToRegistrant, Registration registration, Form copyToForm, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // copy all answers
                    var answers = registrant.GetAnswers(amsgs, adc);
                    answers.ForEach(answer =>
                    {
                        FormQuestion question = null;

                        if (answer.QuestionDataTypeId != QuestionDataType.NotSet && copyToForm.IsNotEmpty())
                            // need to load the question from the provider account form
                            // to ensure data is stored with the form on provider account and not generic form
                            question = copyToForm.GetQuestion(answer.QuestionDataTypeId, msgs, dc);

                        // not doing an else if just in case the question comes back null
                        if (question.IsEmpty())
                            question = answer.ToFormQuestion(true, amsgs, adc);
                            
                        copyToRegistrant.AddOrUpdateAnswer(
                            answerText: answer.Text,
                            question: question,
                            registrationId: registration?.ID,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);
                    });
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void WithdrawRegistration(this Registrant registrant, int participationObjectId, ParticipationObjectType participationObjectType,
            WebUser loggedInUser, Account account, bool isCancelPayment, SystemMessages msgs, bool isParent = false, DatabaseConnection dc = null)
        {
            var masterRegistrant = registrant.GetMasterRegistrant(msgs, dc);
            var ras = registrant.GetRegistrantActivities(msgs, dc);
            var ra = ras.Find(item => item.ParticipationObjectId == participationObjectId && item.ParticipationObjectTypeId == participationObjectType);

            if (ra.IsEmpty())
            {
                msgs.Add("Registration not found", MessageType.Notice);
                return;
            }

            var activity = ra.GetActivity(msgs, dc);
            var hasAccessToActivity = activity.IsNotEmpty()
                ? ActivityWebUser.LoadByActivityIdByWebUserId(activity.ID.Value, loggedInUser.ID.Value, msgs, dc).IsNotEmpty()
                : false;

            if (!loggedInUser.IsTechSupportOwnerOrAdmin(account, msgs, dc)
                && !loggedInUser.HasAccessToRegistrant(masterRegistrant, msgs, dc)
                && !hasAccessToActivity)
            {
                msgs.Add("You don't have permission to modify this registration", MessageType.UserError);
                return;
            }

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    ra.ChangeSeatAssignment(RegistrationAssignmentType.Withdrawn, loggedInUser, amsgs, adc);

                    // cancel subscription if it has one
                    if (msgs.HasNoErrors() && isCancelPayment)
                        registrant.CancelSubcriptionIfExists(participationObjectId, participationObjectType, loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);

                    // Notify Provider
                    var participationObject = ra.GetParticipationObject(msgs, dc);
                    EmailHelper.SendRegistrationWithdrawal(registrant, participationObject, loggedInUser, isParent, isCancelPayment, msgs);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static Registration AddRegistration(this Registrant registrant, int ownerAccountId, int? activityGroupId, int? accountActivityGroupId,
            int? periodId, RegistrationCompletionStatus completionStatus, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var reg = new Registration
            {
                AccountId = ownerAccountId,
                RegistrantId = registrant.ID,
                ActivityGroupId = activityGroupId,
                AccountActivityGroupId = accountActivityGroupId,
                RegistrationPeriodId = periodId,
                CompletionStatusId = completionStatus,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId,
                LastUpdated = DateTime.UtcNow,
            };
            msgs.AddRange(reg.Save(dc));

            if (registrant.FirstName.IsNotEmpty() && registrant.LastName.IsNotEmpty() && registrant.Email.IsNotEmpty())
                reg.FindAndSetFirstRegistration(registrant.FirstName, registrant.LastName, registrant.Email, msgs, dc);

            return reg.Reload(msgs, dc);
        }

        public static Registration AddRegistrationWithESignature(this Registrant registrant, int ownerAccountId, int? activityGroupId,
            int? accountActivityGroupId, int? periodId, string esignature, string userAgent, string ipAddress, int loggedInUserId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var reg = new Registration
            {
                AccountId = ownerAccountId,
                RegistrantId = registrant.ID,
                ActivityGroupId = activityGroupId,
                AccountActivityGroupId = accountActivityGroupId,
                RegistrationPeriodId = periodId,
                ESignature = esignature,
                CompletionStatusId = RegistrationCompletionStatus.Signed,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId,
                LastUpdated = DateTime.UtcNow,
                ESignatureUserAgent = userAgent,
                ESignatureIPAddress = ipAddress,
            };
            msgs.AddRange(reg.Save(dc));

            return reg;
        }

        public static List<Registration> GetRegistrationsFromMaster(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!registrant.IsMaster)
                registrant.GetRegistrations(msgs, dc);

            var registrations = new List<Registration>();
            var submittedRecords = registrant.GetSubmittedRecords(msgs, dc);

            submittedRecords.ForEach(submittedRecord => registrations.AddRange(submittedRecord.GetRegistrations(msgs, dc)));

            return registrations;
        }

        public static List<Registration> GetRegistrations(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrant.IsMaster)
                return registrant.GetRegistrationsFromMaster(msgs, dc);

            return Registration.LoadByRegistrantId(registrant.ID.Value, msgs, dc: dc);
        }

        public static Registration GetRegistrationWithParticipationObject(this Registrant registrant, ParticipationObject participationObject,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrations = registrant.GetRegistrations(msgs, dc);

            foreach (var registration in registrations.Where(registration => registration.AccountId == participationObject.OwnerAccount.ID))
            {
                if (registration.GetParticipationObjects(msgs, dc).Any(po => po.IsMatch(participationObject)))
                    return registration;
            }

            return null;
        }

        public static List<Registrant> FindRelatedRegistrants(this Registrant registrant, string firstName = null, string lastName = null, string email = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.FindRelatedRegistrants(
                registrant.AccountId.Value,
                firstName ?? registrant.FirstName,
                lastName ?? registrant.LastName,
                email ?? registrant.Email,
                msgs,
                dc);
        }

        public static SystemMessages AddRelatedRegistrant(this Registrant registrant, Registrant otherRegistrant, int loggedInUserId,
            DatabaseConnection dc = null)
        {
            otherRegistrant.FirstRegistrantId = registrant.ID.Value;
            otherRegistrant.LastModifiedBy = loggedInUserId;
            otherRegistrant.LastModifiedDate = DateTime.UtcNow;
            return otherRegistrant.Save(dc);
        }

        public static List<Registrant> GetRelatedRegistrants(this Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrants = new List<Registrant>();
            if (registrant.FirstRegistrantId.HasValue)
            {
                registrants.Add(Registrant.Load(registrant.FirstRegistrantId.Value, dc: dc));
                registrants.AddRange(Registrant.LoadByFirstRegistrantId(registrant.FirstRegistrantId.Value, dc: dc));
            }
            else
                registrants.AddRange(Registrant.LoadByFirstRegistrantId(registrant.ID.Value, dc: dc));

            return registrants.Where(x => x.ID != registrant.ID).OrderByDescending(x => x.CreateDate).ToList();
        }

        public static bool IsRelated(this Registrant registrant, Registrant otherRegistrant)
        {
            return registrant.FirstName.Trim().ToLower() == otherRegistrant.FirstName.Trim().ToLower() &&
                   registrant.LastName.Trim().ToLower() == otherRegistrant.LastName.Trim().ToLower() &&
                   registrant.Email.Trim().ToLower() == otherRegistrant.Email.Trim().ToLower();
        }

        public static void DeleteWithRelatedRecords(this Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    registrant.DeleteSessionActions(amsgs, adc);
                    registrant.DeleteRegistrantActivities(amsgs, adc);
                    registrant.IsDeleted = true;
                    amsgs.AddRange(registrant.Save(adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        internal static void DeleteSessionActions(this Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            SessionAction.LoadByRegistrantId(registrant.ID.Value, msgs, dc).ForEach(sa => msgs.AddRange(sa.Delete(dc)));
        }

        internal static void DeleteRegistrantActivities(this Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            registrant.GetRegistrantActivities(msgs, dc).ForEach(ra => ra.CancelRegistration(msgs, dc));
        }

        public static int GetRegistrationCount(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.GetCount(new Hashtable() {{"RegistrantId", registrant.ID.Value}}, msgs: msgs, dc: dc);
        }

        public static Registrant GetParentOrGuardian(this Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var masterRegistrant = registrant;

            if (!registrant.IsMaster)
                masterRegistrant = registrant.GetMasterRegistrant(msgs, dc);

            if (!masterRegistrant.IsEmpty())
                return Registrant.LoadByAccountIdByIsDeleted(masterRegistrant.AccountId.Value, false, msgs, dc)
                    .Where(item => item.IsParentOrGuardian)
                    .OrderBy(item => item.ID)
                    .FirstOrDefault();

            return null;
        }

        public static void DeleteRegistrantActivity(this Registrant registrant, int activityId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrantAcitivity = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                activityId,
                ParticipationObjectType.Activity,
                registrant.ID.Value,
                msgs,
                dc);

            msgs.AddRange(registrantAcitivity.Delete(dc));
        }

        public static decimal GetAmountPaid(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.LoadByRegistrantIdByIsDeleted(registrant.ID.Value, false, msgs, dc).Sum(payment => payment.Amount.Value);
        }

        public static RegistrantActivity GetRegistrantActivity(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByParticipationObjectByRegistrant(
                participationObject.ObjectID.Value,
                participationObject.ObjectType,
                registrant.ID.Value,
                msgs,
                dc);
        }

        public static void MarkAsDeleted(this Registrant registrant, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            registrant.IsDeleted = true;
            registrant.LastModifiedDate = DateTime.UtcNow;
            registrant.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(registrant.Save(dc));
        }

        public static Registrant CreateNewContact(Account account, string firstName, string lastName, string email, string phone, bool allowDuplicates,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var doesContactExist = account.DoesContactExist(
                firstName.IfEmpty("").Trim(),
                lastName.IfEmpty("").Trim(),
                email.IfEmpty("").Trim(),
                msgs,
                dc);

            if (doesContactExist && !allowDuplicates)
                return null;

            var contact = new Registrant
            {
                AccountId = account.ID,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID,
                LastModifiedBy = loggedInUser?.ID,
                IsContactOnly = true,
                RegistrantTypeId = RegistrantType.Individual,
                Email = email.IfEmpty("").Trim(),
                FirstName = firstName.IfEmpty("").Trim(),
                LastName = lastName.IfEmpty("").Trim(),
                Phone = phone.IfEmpty("").Trim(),
            };

            msgs.AddRange(contact.Save(dc));

            return contact;
        }

        public static void ProcessActivityPayment(this Registrant registrant, CheckoutObject checkoutObject, Transaction transaction, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (checkoutObject.AmountPaidByParent.HasValue && checkoutObject.AmountReceivedByProvider.HasValue)
            {
                // need to track a payment for each Activity/ActivityGroup
                checkoutObject.ParticipationObject.AddPayment(
                    amountReceivedByProvider: checkoutObject.AmountReceivedByProvider.Value,
                    amountPaidByParent: checkoutObject.AmountPaidByParent.Value,
                    isAmountProRated: checkoutObject.IsAmountProRated,
                    registrant: registrant,
                    transaction: transaction,
                    registrantSubscriptionPlan: checkoutObject.RegistrantSubscriptionPlan,
                    createDate: checkoutObject.Date,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);
                return;
            }

            var amountPaidByParentWithoutFees = checkoutObject.OverrideAmount ?? (checkoutObject.PostPayPlan.IsNotEmpty()
                ? checkoutObject.GetPostPayPlanCostTodayWithFees()
                : checkoutObject.GetTotalCostWithoutSubscriptionToPurchaseTodayNoFees(msgs, dc));

            // need to track a payment for each Activity/ActivityGroup
            checkoutObject.ParticipationObject.AddPayment(
                amount: amountPaidByParentWithoutFees,
                registrant: registrant,
                transaction: transaction,
                registrantSubscriptionPlan: checkoutObject.RegistrantSubscriptionPlan,
                createDate: checkoutObject.Date,
                loggedInUser: loggedInUser,
                msgs: msgs,
                dc: dc);
        }

        private static string GetFormattedAnswerForCheckboxList(string s) => $"[{s}]";

        public static RegistrantHelper PrepareRegistrantDetails(this List<Registrant> registrants, Registrant masterRegistrant, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var activityInstances =
                new List<(ActivityInstance ActivityInstance, Registrant Registrant, int? ParticipationObjectId, ParticipationObjectType ParticipationObjectType
                    )>();
            var providers = new List<Account>();
            var registrantActivities = new List<RegistrantActivity>();

            registrants.ForEach(registrant =>
            {
                if (registrant.SchoolLocationId.HasValue)
                {
                    var account = registrant.GetSchoolLocation(msgs, dc)?.GetHostAccount(msgs, dc);
                    if (account.IsNotEmpty() && !providers.Any(provider => provider.ID == account.ID))
                        providers.Add(account);
                }

                registrant.GetSubmittedRecords(msgs, dc).ForEach(submittedRecord =>
                {
                    var ras = submittedRecord.GetRegistrantActivities(Registration.ReservedStatuses, msgs, dc);
                    var account = submittedRecord.GetAccount(msgs, dc);

                    ras.ForEach(ra =>
                    {
                        var schedule = ra.GetSchedule(msgs, dc);
                        var instances = schedule?.GetInSessionInstances(msgs: msgs, dc: dc);
                        var participantObject = new ParticipationObject(ra.ParticipationObjectId.Value, ra.ParticipationObjectTypeId, msgs, dc);

                        if (instances.IsNotEmpty())
                        {
                            var activityInstanceSeats = ActivityInstanceSeats.LoadByRegistrantActivityId(ra.ID.Value, msgs, dc);
                            if (participantObject.CanRegisterPerClass)
                                instances.RemoveAll(instance =>
                                    instance.ID.NotIn(activityInstanceSeats.Select(activityInstanceSeat => activityInstanceSeat.ActivityInstanceId)));

                            // remove out of session instances from in session
                            // this will required when there are dates for (Enter any dates that you will not meet), check schedule page of provider side
                            instances.RemoveOutOfSessionInstances(schedule, msgs, dc);

                            activityInstances.AddRange(instances.Select(instance => (
                                instance,
                                registrant,
                                ra.ParticipationObjectId,
                                ra.ParticipationObjectTypeId)));
                        }
                    });

                    if (masterRegistrant.IsParentOrGuardian)
                        registrantActivities.AddRange(ras);

                    if (!providers.Any(provider => provider.ID == account.ID))
                        providers.Add(account);
                });
            });

            var model = new RegistrantHelper();
            model.Providers = providers;

            model.ActivityInstances = activityInstances.Where(item => item.ActivityInstance.StartDateTime.Value.Date >= DateTime.UtcNow.Date)
                .OrderBy(item => item.ActivityInstance.StartDateTime.Value)
                .Take(10)
                .ToList();

            model.RegistrantActivities = registrantActivities.Where(ra =>
                    (ra.AssignmentTypeId.In(new List<RegistrationAssignmentType>()
                         {RegistrationAssignmentType.ReservedPendingParentalConsent, RegistrationAssignmentType.ReservedPendingPayment}) ||
                     (ra.AssignmentTypeId == RegistrationAssignmentType.Applied && ra.GetRegistration(msgs, dc).ESignature.IsEmpty())) &&
                    !ra.HasPlan(msgs, dc))
                .ToList();

            return model;
        }

        public static void UpdateGenderFromFormQuestionAnswers(this Registrant registrant, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (registrant.GenderId != GenderType.NotSet || !registrant.ID.HasValue) return;
            var formQuestionAnswer =
                FormQuestionAnswer.LoadByRegistrantIdByQuestionDataTypeId((int)registrant.ID , QuestionDataType.RegistrantGender, msgs , dc);
            if (formQuestionAnswer.IsNotEmpty() && formQuestionAnswer.Text.IsNotEmpty())
            {
                registrant.GenderId = FormService.GetGenderFromAnswer(formQuestionAnswer.Text);
                msgs.AddRange(registrant.Save(dc));
            }
        }
    }
}