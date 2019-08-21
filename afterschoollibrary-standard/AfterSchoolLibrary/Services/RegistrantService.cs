using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantService
    {
        public static WebUser MatchCreateOrUpdateUser(this Registrant registrant, string firstname, string lastname, string email, string password = null, bool isAfterSchool = false, bool forceMatch = false, SystemMessages msgs = null, DatabaseConnection dc = null)
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
        /// TODO: Refactor and simplify
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
        private static WebUser CreateOrUpdateUser(Registrant registrant, string firstname, string lastname, string email, string password = null, bool isAfterSchool = false, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                                IsRegistered = false
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
                        user.SendVerificationEmail(isAfterSchool);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.Add("Error creating account", MessageType.Error);
                },
                dc: dc
            );

            return user;
        }

        private static (Stripe.Customer customer, string sourceId) GetCustomerAndSourceForSubscription(CheckoutObject checkoutObject, bool useStripeConnectedAccount)
        {
            // have to copy the source from the platform to be able to move it to the stripe connected account
            // unless you're not using connected accounts for subscriptions
            var msgs = new SystemMessages();
            var billingAccount = checkoutObject.BillingAccount;
            var purchasingAccount = checkoutObject.PayingAccount;
            string sourceId = null;
            Stripe.Customer customer = null;

            if (!useStripeConnectedAccount)
            {
                customer = purchasingAccount.GetStripeCustomer(msgs);
                sourceId = checkoutObject.StripeSourceId ?? purchasingAccount.CreateStripeSourceFromToken(checkoutObject.StripeToken, customer, msgs);
            }
            else
            {
                // if running the billing through stripe connected account
                sourceId = purchasingAccount.CopyStripeSourceToAccount(checkoutObject, msgs);

                // create the customer on the connected account
                customer = Billing.StripeHelper.CreateCustomer(
                    description: purchasingAccount.Name,
                    email: purchasingAccount.Email,
                    sourceId: sourceId,
                    stripeAccountId: billingAccount.StripeUserId,
                    msgs: msgs);
            }

            return (customer, sourceId);
        }

        public static void StartPaymentPlan(this Registrant registrant, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            registrant.StartPlan(checkoutObject, SubscriptionPlanType.PaymentPlan, loggedInUser, msgs, dc);
        }

        public static void StartSubscriptionPlan(this Registrant registrant, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            registrant.StartPlan(checkoutObject, SubscriptionPlanType.Subscription, loggedInUser, msgs, dc);
        }

        public static void StartPlan(this Registrant registrant, CheckoutObject checkoutObject, SubscriptionPlanType subscriptionPlanType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var billingAccount = checkoutObject.BillingAccount;
            var purchasingAccount = checkoutObject.PayingAccount;            
            var useStripeConnectedAccount = billingAccount.HasAccountPackageFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);
            var (customer, sourceId) = GetCustomerAndSourceForSubscription(checkoutObject, useStripeConnectedAccount);
            var plan = subscriptionPlanType == SubscriptionPlanType.PaymentPlan ? checkoutObject.PaymentPlan : checkoutObject.SubscriptionPlan;
            var stripeCouponId = plan.DoesCouponApply(checkoutObject.Coupon) ? checkoutObject.Coupon.StripeCouponId : null;
            DateTime? startDateTime = DateTime.UtcNow;
            var platformFeePercent = new decimal?();
            string stripeAccountId = null;

            if (checkoutObject.ParticipationObject.IsActivity())
            {
                var firstActivityDate = checkoutObject.ParticipationObject.GetActivity().GetFirstDate(msgs, dc);

                // can't start in the past
                if (firstActivityDate.HasValue && firstActivityDate.Value.IsAfter(startDateTime.Value))
                    startDateTime = firstActivityDate;
            }

            if (useStripeConnectedAccount)
            {
                platformFeePercent = billingAccount.GetSubscriptionPlatformFeePercent(plan, msgs, dc);
                stripeAccountId = billingAccount.StripeUserId;
            }

            // create the subscription on the stripe account (could be added to main account or connected account)
            var subscription = Billing.StripeHelper.CreateSubscription(
                customerId: customer.Id,
                planId: plan.StripePlanId,
                sourceId: sourceId,
                platformFeePercent: platformFeePercent,
                stripeAccountId: stripeAccountId,
                startDate: startDateTime,
                stripeCouponId: stripeCouponId,
                metaData: new Dictionary<string, string>()
                {
                    { "BillingAccountUid", billingAccount.Uid.ToString() },
                    { "BillingAccountName", billingAccount.Name.ToString() },
                    { "PayingAccountUid", purchasingAccount.Uid.ToString() },
                    { "PayingAccountName", purchasingAccount.Name.ToString() },
                    { "ParticipationObjectType", checkoutObject.ParticipationObject.ObjectType.ToString() },
                    { "ParticipationObjectName", checkoutObject.ParticipationObject.Name },
                },
                msgs: msgs);

            if (msgs.HasErrors())
                return;

            var purchase = checkoutObject.CreateOrUpdatePurchase(loggedInUser, msgs, dc);

            checkoutObject.RegistrantSubscriptionPlan = plan.AddRegistrant(
                registrant,
                startDateTime,
                purchase,
                subscription.Id,
                GetFeeObject(checkoutObject, billingAccount, subscriptionPlanType, msgs, dc),
                loggedInUser,
                msgs,
                dc);
        }

        private static PlanFeeObject GetFeeObject(CheckoutObject checkoutObject, Account billingAccount, SubscriptionPlanType subscriptionPlanType, SystemMessages msgs, DatabaseConnection dc = null)
        {
            decimal? convenienceFee = 0;
            decimal? paymentTypeFee = 0;
            decimal? amount = 0;
            int? totalPeriods = null;

            if (subscriptionPlanType == SubscriptionPlanType.PaymentPlan)
            {
                convenienceFee = checkoutObject.GetPaymentPlanCostTodayConvenienceFee(msgs, dc);
                paymentTypeFee = checkoutObject.GetPaymentPlanCostTodayCreditCardFee(msgs, dc);
                amount = checkoutObject.PaymentPlan.CostPerPeriod;
                totalPeriods = checkoutObject.PaymentPlan.TotalPeriods;
            }
            else
            {
                convenienceFee = checkoutObject.GetSubscriptionCostTodayConvenienceFee(msgs, dc);
                paymentTypeFee = checkoutObject.GetSubscriptionCostTodayCreditCardFee(msgs, dc);
                amount = checkoutObject.SubscriptionPlan.CostPerPeriod;
            }

            return new PlanFeeObject()
            {
                ConvenienceFee = convenienceFee.Value,
                ConvenienceFeesPaidBy = billingAccount.ConvenienceFeesPaidBy,
                PaymentTypeFee = paymentTypeFee.Value,
                PaymentTypeFeesPaidBy = billingAccount.CreditCardFeesPaidBy,
                Amount = amount.Value,
                TotalPeriods = totalPeriods,
                PaymentType = checkoutObject.PaymentType
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
                criterias: new Hashtable() { { "RegistrantId", registrant.ID.Value } }, 
                msgs: msgs, 
                dc: dc) > 0;
        }

        internal static void SendText(this Registrant registrant, Account sendingAccount, WebUser loggedInUser, ActivityMessage message, string activityName, string subject, string body, string senderName, SystemMessages msgs, DatabaseConnection dc = null)
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

                // ensure this message has not already been sent to this user
                var twilioSms = TwilioSMS.LoadByRegistrantIdByActivityMessageId(registrant.ID.Value, message.ID.Value, msgs, dc);

                if (twilioSms.IsEmpty())
                {
                    if (!registrant.HasReceivedTextMessages(msgs, dc))
                        TwilioHelper.SendSMS(
                            registrant.Phone,
                            $"Welcome to {Helpers.Utilities.AppName()} Alerts! You are receiving this message because you enabled text notifications when you registered your child for activities. \nMsg&data rates may apply. \nReply HELP for help, STOP to cancel",
                            msgs);

                    var resource = ActivityMessageService.SendTextMessage(registrant.Phone, senderName, subject, activityName, message.Uid.ToString(), msgs);

                    if (msgs.HasNoErrors() && resource.IsNotEmpty())
                    {
                        twilioSms = new TwilioSMS()
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
                }
            }
        }

        internal static void SendEmail(this Registrant registrant, Account account, string activityName, string fromName, string replyToEmail, string replyToName, string subject, string body, string utmCampaign = "", SystemMessages msgs = null)
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

        public static void AddUsersWithAccess(this Registrant registrant, List<WebUser> users, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            users.ForEach(user => registrant.AddUserWithAccess(user, loggedInUser, msgs, dc));
        }

        public static void AddUserWithAccess(this Registrant registrant, WebUser user, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                        CreatedBy = loggedInUser?.ID.IfEmpty(0)
                    };
                    msgs.AddRange(registrantWebUser.Save(dc));
                }
            }
        }

        public static void SetProfileAsCompleted(this Registrant registrant, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            registrant.UpdateProfileStatus(RegistrantProfileStatus.Completed, loggedInUser, msgs, dc);
        }

        public static void UpdateProfileStatus(this Registrant registrant, RegistrantProfileStatus profileStatus, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            registrant.ProfileStatusId = profileStatus;
            registrant.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            registrant.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(registrant.Save(dc));
        }

        private static bool HasParticipationAssignment(this Registrant masterRegistrant, ParticipationObject participationObject, List<RegistrationAssignmentType> assignmentTypes, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
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

        public static bool MasterRegistrantHasPlan(this Registrant masterRegistrant, ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var submittedRecords = masterRegistrant.GetSubmittedRecords(participationObject.OwnerAccount.ID.Value, msgs, dc);

            return submittedRecords.Any() && RegistrantSubscriptionPlan.ExistsByParticipationObjectByRegistrants(
                participationObjectId: participationObject.ObjectID.Value,
                participationObjectType: participationObject.ObjectType,
                registrantIds: submittedRecords.Select(item => item.ID.Value).ToList(),
                msgs: msgs,
                dc: dc);
        }

        public static bool HasPlan(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantSubscriptionPlan.ExistsByParticipationObjectByRegistrants(
                    participationObjectId: participationObject.ObjectID.Value,
                    participationObjectType: participationObject.ObjectType,
                    registrantIds: registrant.ID.Value.AsList(),
                    msgs: msgs,
                    dc: dc);
        }

        public static List<RegistrantSubscriptionPlan> GetRegistrantSubscriptionPlans(this Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return RegistrantSubscriptionPlan.LoadByRegistrantId(registrant.ID.Value, msgs, dc);
        }

        public static bool IsAppliedFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return HasParticipationAssignment(masterRegistrant, participationObject, RegistrationAssignmentType.Applied.AsList(), hasParentalConsent, msgs, dc);
        }
        
        public static bool HasConsentFor(this Registrant masterRegistrant, ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return HasParticipationAssignment(masterRegistrant, participationObject, null, true, msgs, dc);
        }

        public static bool IsReservedFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var assignmentTypes = new List<RegistrationAssignmentType>()
                {
                    RegistrationAssignmentType.Reserved,
                    RegistrationAssignmentType.ReservedPendingPayment,
                    RegistrationAssignmentType.ReservedPendingParentalConsent,
                };

            return HasParticipationAssignment(masterRegistrant, participationObject, assignmentTypes, hasParentalConsent, msgs, dc);
        }

        public static bool IsReservedAndPaidFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return HasParticipationAssignment(masterRegistrant, participationObject, RegistrationAssignmentType.Reserved.AsList(), hasParentalConsent, msgs, dc);
        }

        public static bool HasWithdrawn(this Registrant masterRegistrant, ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return HasParticipationAssignment(masterRegistrant, participationObject, RegistrationAssignmentType.Withdrawn.AsList(), null, msgs, dc);
        }

        public static bool HasSubmittedFor(this Registrant masterRegistrant, ParticipationObject participationObject, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var assignmentTypes= new List<RegistrationAssignmentType>()
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
            return registrant.ParentRegistrantId.HasValue ? Registrant.Load(registrant.ParentRegistrantId.Value, msgs, dc) : null;
        }

        public static Registrant SubmitRecordToAccount(this Registrant registrant, Account ownerAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return copy;
        }

        public static List<Registrant> GetSubmittedRecords(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByParentRegistrantId(registrant.ID.Value, msgs, dc);
        }

        public static List<Registrant> GetSubmittedRecords(this Registrant registrant, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByParentRegistrantIdByAccountId(registrant.ID.Value, accountId, msgs, dc);
        }

        public static (Registrant, Registration) GetSubmittedRecord(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var submittedRecords = registrant.GetSubmittedRecords(msgs, dc);

            if (!submittedRecords.Any())
                return (null, null);

            var submittedRecordIds = submittedRecords.Select(child => child.ID.Value).ToList();
            
            var ras = RegistrantActivity.LoadByRegistrantIds(submittedRecordIds, msgs, dc);

            var submittedRecordRA = ras.FirstOrDefault(ra => ra.ParticipationObjectId == participationObject.ObjectID && ra.ParticipationObjectTypeId == participationObject.ObjectType);
            
            var registration = new Registration();

            if (submittedRecordRA.IsNotEmpty())
                return (
                    submittedRecords.First(child => child.ID == submittedRecordRA.RegistrantId), 
                    Registration.Load(submittedRecordRA.RegistrationId.Value, msgs, dc)
                );

            return (null, null);
        }

        public static RegistrantActivity AddOrUpdateParticipationObject(this Registrant registrant, int registrationId, int participationObjectId, ParticipationObjectType participationObjectType, RegistrationAssignmentType assignmentType, int? registrationPeriodId, int? activityGroupId, decimal? amountCharged, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static RegistrantActivity AddOrUpdateParticipationObject(this Registrant registrant, int registrationId, int participationObjectId, ParticipationObjectType participationObjectType, RegistrationAssignmentType assignmentType, int preferenceNumber, int? registrationPeriodId, int? activityGroupId, decimal? amountCharged, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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
                    CreatedBy = loggedInUserId
                };

            ra.LastModifiedDate = DateTime.UtcNow;
            ra.LastModifiedBy = loggedInUserId;
            ra.PreferenceNumber = preferenceNumber;

            if (amountCharged.HasValue)
                ra.AmountCharged = amountCharged;

            msgs.AddRange(ra.Save(dc));

            ra.ChangeSeatAssignment(assignmentType, new WebUser { ID = loggedInUserId }, msgs, dc);

            return ra;
        }

        public static FormQuestionAnswer MapAndAddAnswer(this Registrant registrant, List<FormQuestion> questions, Registrant fromRegistrant, QuestionDataType fromType, QuestionDataType toType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var parentAnswer = fromRegistrant.GetAnswer(fromType, msgs, dc);
            var studentQuestion = questions.Find(question => question.QuestionDataTypeId == toType);
            if (studentQuestion.IsNotEmpty() && parentAnswer.IsNotEmpty())
                return registrant.AddAnswer(parentAnswer.Text, studentQuestion, loggedInUser, msgs, dc);

            return null;
        }

        private static string UploadFile(this Registrant registrant, System.IO.Stream file, string filename, FormQuestion question, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ah = Helpers.Utilities.GetNewAmazonHelper();

            var regEx = new System.Text.RegularExpressions.Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
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

        public static FormQuestionAnswer AddAnswer(this Registrant registrant, System.IO.Stream file, string filename, FormQuestion question, int? registrationId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
            {
                var amazonFilePath = registrant.UploadFile(file, filename, question, msgs, dc);
                
                if (msgs.HasNoErrors())
                    return registrant.AddAnswer(amazonFilePath, question, registrationId, loggedInUser, msgs, dc);
            }

            return null;
        }

        public static FormQuestionAnswer AddAnswer(this Registrant registrant, string answerText, FormQuestion question, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
                return registrant.AddAnswer(answerText, question, null, loggedInUser, msgs, dc);

            return null;
        }

        public static FormQuestionAnswer AddAnswer(this Registrant registrant, string answerText, FormQuestion question, int? registrationId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
                return registrant.AddAnswer(answerText, question.ID, question.QuestionDataTypeId, question.Text, registrationId, loggedInUser, msgs, dc);

            return null;
        }

        public static FormQuestionAnswer AddAnswer(this Registrant registrant, string answerText, int? questionId, QuestionDataType questionDataType, string questionText, int? registrationId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            //check if answer for this one already exists, and where formquestion is null
            FormQuestionAnswer answer = null;

            if (questionDataType == QuestionDataType.NotSet)
                answer = FormQuestionAnswer.LoadByRegistrantIdByFormQuestionIdByRegistrationId(
                    RegistrantId: registrant.ID.Value,
                    FormQuestionId: questionId.Value,
                    RegistrationId: registrationId.Value,
                    msgs: msgs,
                    dc: dc);
            else
                answer = FormQuestionAnswer.LoadByRegistrantIdByQuestionTypeId(registrant.ID.Value, questionDataType, msgs, dc);

            if (answer.IsEmpty())
                answer = new FormQuestionAnswer
                {
                    RegistrantId = registrant.ID,
                    FormQuestionId = questionId.Value,
                    QuestionDataTypeId = questionDataType,
                    QuestionText = questionText,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID.IfEmpty(0)
                };

            answer.RegistrationId = registrationId;
            answer.Text = answerText;
            answer.LastModifiedDate = DateTime.UtcNow;
            answer.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(answer.Save(dc));

            answer.UpdateRegistrantInfo(registrant, msgs, dc);

            return answer;
        }

        public static RegistrantActivity AddInviteToActivity(this Registrant registrant, int activityId, int loggedInUserId, int? activityGroupId, int? registrationPeriodId, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                    ActivityGroupId = activityGroupId
                };

            ra.LastModifiedDate = DateTime.UtcNow;
            ra.LastModifiedBy = loggedInUserId;
            ra.RegistrationPeriodId = registrationPeriodId;
            ra.AssignmentTypeId = RegistrationAssignmentType.Invited;

            msgs.AddRange(ra.Save(dc: dc));

            return ra;
        }

        public static SystemMessages AddInviteToActivities(this Registrant registrant, List<int> activityIds, int loggedInUserId, int? activityGroupId, int? registrationPeriodId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            activityIds.ForEach(activityId =>
                registrant.AddInviteToActivity(activityId, loggedInUserId, activityGroupId, registrationPeriodId, msgs, dc)
            );
            return msgs;
        }

        public static Registrant CreateMasterCopy(this Registrant registrant, List<FormQuestion> questions, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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

        public static Registrant CopyWithAnswers(this Registrant registrant, List<FormQuestion> questions, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var copy = registrant.Copy(loggedInUser, msgs, dc);

            if (questions.IsEmpty())
                questions.ForEach(question =>
                    copy.AddAnswer(question.GetAnswer(registrant.ID.Value, msgs, dc).Text, question, null, loggedInUser, msgs, dc)
                );

            return copy;
        }

        public static void CopyAnswers(this Registrant registrant, Registrant copyingRegistrant, Registration registration, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
              (
                  action: (SystemMessages amsgs, DatabaseConnection adc) =>
                  {
                      // copy all answers
                      var answers = registrant.GetAnswers(amsgs, adc);
                      answers.ForEach(answer =>
                      {
                          copyingRegistrant.AddAnswer(
                              answerText: answer.Text,
                              question: answer.ToFormQuestion(true, amsgs, adc),
                              registrationId: registration?.ID,
                              loggedInUser: loggedInUser,
                              msgs: amsgs,
                              dc: adc);
                      });
                  },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void WithdrawRegistration(this Registrant registrant, int participationObjectId, ParticipationObjectType participationObjectType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var masterRegistrant = registrant.GetMasterRegistrant(msgs, dc);

            if (!loggedInUser.HasAccessToRegistrant(masterRegistrant, msgs, dc))
            {
                msgs.Add("You don't have permission to modify this registration", MessageType.UserError);
                return;
            }

            var ras = registrant.GetRegistrantActivities(msgs, dc);
            var registrantActivity = ras.Find(ra => ra.ParticipationObjectId == participationObjectId && ra.ParticipationObjectTypeId == participationObjectType);

            if (registrantActivity.IsEmpty())
            {
                msgs.Add("Registration not found", MessageType.Notice);
                return;
            }

            registrantActivity.ChangeSeatAssignment(RegistrationAssignmentType.Withdrawn, loggedInUser, msgs, dc);

            // Notify Provider
            if (msgs.HasNoErrors())
            {
                var participationObject = registrantActivity.GetParticipationObject(msgs, dc);
                Helpers.EmailHelper.SendRegistrationWithdrawal(registrant, participationObject, loggedInUser, msgs);
            }
        }

        public static Registration AddRegistration(this Registrant registrant, int ownerAccountId, int? activityGroupId, int? accountActivityGroupId, int? periodId, RegistrationCompletionStatus completionStatus, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                LastUpdated = DateTime.UtcNow
            };
            msgs.AddRange(reg.Save(dc));

            if (registrant.FirstName.IsNotEmpty() && registrant.LastName.IsNotEmpty() && registrant.Email.IsNotEmpty())
                msgs.AddRange(reg.FindAndSetFirstRegistration(registrant.FirstName, registrant.LastName, registrant.Email, dc));

            return reg.Reload(msgs, dc);
        }

        public static Registration AddRegistrationWithESignature(this Registrant registrant, int ownerAccountId, int? activityGroupId, int? accountActivityGroupId, int? periodId, string esignature, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                LastUpdated = DateTime.UtcNow
            };
            msgs.AddRange(reg.Save(dc));

            return reg;
        }

        public static List<Registration> GetRegistrations(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByRegistrantId(registrant.ID.Value, msgs, dc: dc);
        }

        public static List<Registrant> FindRelatedRegistrants(this Registrant registrant, string firstName = null, string lastName = null, string email = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.FindRelatedRegistrants(
                registrant.AccountId.Value,
                firstName ?? registrant.FirstName,
                lastName ?? registrant.LastName,
                email ?? registrant.Email,
                msgs,
                dc);
        }

        public static SystemMessages AddRelatedRegistrant(this Registrant registrant, Registrant otherRegistrant, int loggedInUserId, DatabaseConnection dc = null)
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
                    amsgs.AddRange(registrant.Delete(adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static Registrant GetParentOrGuardian(this Registrant masterRegistrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(masterRegistrant.AccountId.Value, msgs, dc)
                .Where(item => item.IsParentOrGuardian)
                .OrderBy(item => item.ID)
                .FirstOrDefault();
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
            return ActivityPayment.LoadByRegistrantId(registrant.ID.Value, msgs, dc).Sum(payment => payment.Amount.Value);
        }

        public static RegistrantActivity GetRegistrantActivity(this Registrant registrant, ParticipationObject participationObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByParticipationObjectByRegistrant(
                participationObject.ObjectID.Value, 
                participationObject.ObjectType, 
                registrant.ID.Value, 
                msgs, 
                dc);
        }
    }
}
