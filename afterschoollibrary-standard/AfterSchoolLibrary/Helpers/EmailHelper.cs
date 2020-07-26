using System;
using BasicLibrary;
using System.IO;
using System.Collections.Specialized;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Linq;
using System.Collections.Generic;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Helpers
{
    public class EmailHelper
    {
        public static SystemMessages SendEmail(string toEmail, string subject, string body, bool isAfterSchool)
        {
            var msgs = new SystemMessages();
            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(toEmail),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                );

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        public static SystemMessages SendPaymentReminder(RegistrantActivity ra)
        {
            var msgs = new SystemMessages();
            var registration = ra.GetRegistration();

            msgs.AddRange(SendPaymentReminder(ra.GetParticipationObject(msgs), registration));

            return msgs;
        }

        public static SystemMessages SendPaymentReminder(ParticipationObject participationObject, Registration registration)
        {
            var msgs = new SystemMessages();
            var registrant = registration.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var parentOrGuardian = masterRegistrant.GetParentOrGuardian(msgs);
            var isAfterSchool = participationObject.OwnerAccount.IsAfterSchool;
            var registering = "registering";      

            if (participationObject.RegistrationMode == RegistrationMode.Apply)
                registering = "applying";

            var model = new NameValueCollection()
            {
                { "FirstName", parentOrGuardian.FirstName },
                { "Registering", registering },
                { "ActivityName",  participationObject.Name },
                { "PaymentLink", registration.GetPaymentUrl },
                { "BillerAccountName", participationObject.OwnerAccount.Name },
                { "ActivityContactName", participationObject.ContactName },
                { "ActivityContactEmail", participationObject.ContactEmail },
                { "ActivityContactPhone", participationObject.ContactPhone },
                { "ProductName", Utilities.AppName(participationObject.OwnerAccount.IsAfterSchool) }
            };

            var subject = $"You have a payment due for {participationObject.Name}";
            var body = ProcessTemplate(
                EmailTemplates.Payment_Reminder,
                model,
                "Payment_Reminder-1.02",
                msgs);

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(participationObject.OwnerAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(parentOrGuardian.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(participationObject.OwnerAccount.IsAfterSchool));

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        public static void SendRegistrationWithdrawal(Registrant registrant, ParticipationObject participationObject, WebUser loggedInUser, SystemMessages msgs)
        {
            var model = new NameValueCollection()
            {
                { "ActivityContactName", participationObject.ContactName },
                { "RegistrantName",  registrant.Name},
                { "ActivityName", participationObject.Name },
                { "CurrentUserName", loggedInUser.DisplayName },
                { "SupportEmail", Email.AppSupportEmailAddress },
                { "ProductName", Utilities.AppName() }
            };

            var subject = $"{registrant.Name}'s registration for {participationObject.Name} has been withdrawn";
            var body = ProcessTemplate(
                EmailTemplates.Registration_Withdrawal,
                model,
                "Registration_Withdrawal",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to activity contact
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(participationObject.ContactEmail),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId()));
        }

        private static string AddLotteryTable(List<RegistrantActivity> ras, Registrant registrant, string action, SystemMessages msgs, DatabaseConnection dc)
        {
            if (!ras.Any())
                return "";

            var rows = "";

            ras.ForEach(ra => {

                var po = ra.GetParticipationObject(msgs, dc);
                var rowModel = new NameValueCollection()
                    {
                        { "ActivityName", po.Name },
                        { "ActivityDays",  po.Schedule.GetDaysOfTheWeek(msgs, dc).Select(x => x.ToString().MaxChars(3)).ToStringList(", ")},
                    };

                rows += ProcessTemplate(
                    EmailTemplates.Registration_LotteryResults_Rows,
                    rowModel,
                    "Registration_LotteryResults_Rows-1.0",
                    msgs);

            });

            var tableModel = new NameValueCollection()
                {
                    { "StudentName", registrant.FirstName },
                    { "Action",  action },
                    { "ActivityRows", rows }
                };

            return ProcessTemplate(
                EmailTemplates.Registration_LotteryResults_Table,
                tableModel,
                "Registration_LotteryResults_Table-1.0",
                msgs);
        }

        public static void SendLotteryResults(Registration registration, Account billingAccount, ParticipationObject session, WebUser loggedInUser, bool sendBcc, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrant = registration.GetRegistrant(msgs, dc);
            var parent = registrant.GetMasterRegistrant(msgs, dc)?.GetParentOrGuardian(msgs, dc);
            var registrantActivities = registration.GetRegistrantActivities(msgs, dc).Where(ra => ra.ParticipationObjectTypeId == ParticipationObjectType.Activity);
            var reservedRas = registrantActivities.Where(ra => ra.AssignmentTypeId.In(Registration.ReservedStatuses)).ToList();
            var waitingListRas = registrantActivities.Where(ra => ra.AssignmentTypeId == RegistrationAssignmentType.WaitingList).ToList();
            var appliedRas = registrantActivities.Where(ra => ra.AssignmentTypeId == RegistrationAssignmentType.Applied).ToList();

            var resultsModel = new NameValueCollection()
                {
                    { "FirstName", parent?.FirstName ?? registrant.GetAnswer(QuestionDataType.Parent1FirstName, msgs, dc).Text },
                    { "ReservedTable", AddLotteryTable(reservedRas, registrant, "has been placed in", msgs, dc) },
                    { "WaitingListTable", AddLotteryTable(waitingListRas, registrant, "has been placed on the <em>waiting list</em> for", msgs, dc) },
                    { "AppliedTable", AddLotteryTable(appliedRas, registrant, "has <em>not</em> been placed in", msgs, dc) },
                    { "ProductName", Utilities.AppName() },
                    { "LoginLink", Utilities.LoginUrl() },
                    { "ActivityContactEmail", session.ContactEmail.IfBlank(billingAccount.Email)},
                    { "ActivityContactPhone", session.ContactPhone.IfBlank(billingAccount.Phone)},
                    { "ActivityContactName", session.ContactName.IfBlank(billingAccount.Name)},
                    { "BillerAccountName", billingAccount.Name },
                    { "BillerAccountLink", billingAccount.HomePageUrl },
                };

            var subject = $"{session.Name} Lottery results for {registrant.Name}";
            var body =  ProcessTemplate(
                EmailTemplates.Registration_LotteryResults,
                resultsModel,
                "Registration_LotteryResults-1.0",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();
            var toEmail = parent?.Email ?? registrant.GetAnswer(QuestionDataType.Parent1Email, msgs, dc).Text ?? registrant.Email;
            
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(toEmail),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId()));
            
            if (sendBcc)
            {
                var emails = new List<string>();

                if (session.ContactEmail.IsEmail() && session.ContactEmail.ToLower().NotIn(emails))
                    emails.Add(session.ContactEmail.ToLower());

                if (!emails.Any())
                    emails.Add(billingAccount.Email.ToLower());

                emails.ForEach(email =>
                    SendGridHelper.SendMail(
                        fromEmail: Email.SendGridNoReplyEmailAddress(true),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                        subject: $"Lottery CC: {subject}",
                        body: body,
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(true)));
            }

            msgs.AddRange(mailMsgs);
        }

        public static void SendInterestConfirmation(CheckoutObject checkoutObject, string customMessage, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registration = checkoutObject.Registration;
            var participationObject = checkoutObject.ParticipationObject;
            var registrantActivities = registration.GetRegistrantActivities(msgs, dc);
            var mainRA = registrantActivities.First(ra => 
                            ra.ParticipationObjectId == participationObject.ObjectID && 
                            ra.ParticipationObjectTypeId == participationObject.ObjectType);
            var interestFormThankYou = participationObject.GetRegistrationThankYou(mainRA, msgs, dc);
            var masterRegistrant = checkoutObject.MasterRegistrant;
            var billingAccount = checkoutObject.BillingAccount;
            var registrationDetails = "Login to your account to view more details.";
            var loginLink = Utilities.LoginUrl();

            if (customMessage.IsEmpty())
            {
                customMessage = interestFormThankYou;

                if (loggedInUser.HasAccessTo(billingAccount))
                    customMessage = $"{masterRegistrant.FirstName} has registered interest for the following activity:";
            }

            var model = new NameValueCollection()
            {
                { "FirstName", loggedInUser.FirstName },
                { "ProductName", Utilities.AppName(true) },
                { "SupportEmail", Email.AppSupportEmailAddress },
                { "CustomMessage", customMessage },
                { "LoginLink", loginLink },
                { "ActivityContactEmail", participationObject.ContactEmail.IfBlank(billingAccount.Email)},
                { "ActivityContactPhone", participationObject.ContactPhone.IfBlank(billingAccount.Phone)},
                { "ActivityContactName", participationObject.ContactName.IfBlank(billingAccount.Name)},
                { "RegistrationDetails", registrationDetails},
                { "BillerAccountName", billingAccount.Name },
                { "BillerAccountLink", billingAccount.HomePageUrl },
            };

            var body = ProcessTemplate(
                EmailTemplates.Interest_Confirmation,
                model,
                "interest-confirmation-1.0.1",
                msgs);

            var subject = $"Thanks for sharing your interest in {participationObject.Name}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(loggedInUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true));

            msgs.AddRange(mailMsgs);
        }

        public static SystemMessages SendRegistrationConfirmed(CheckoutObject checkoutObject, string customMessage, WebUser loggedInUser, bool isParentCopy, bool sendBcc, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var isAfterSchool = checkoutObject.PayingAccount.IsAfterSchool;
            var purchaserAccount = checkoutObject.PayingAccount;
            var purchaser = checkoutObject.PayingAccount.GetAccountOwner(msgs, dc);
            var participationObject = checkoutObject.ParticipationObject;
            var billingAccount = checkoutObject.ParticipationObject.OwnerAccount;
            var billingAccountOwner = billingAccount.GetAccountOwner(msgs, dc);
            var paymentPlan = checkoutObject.PaymentPlan;
            var subscription = checkoutObject.SubscriptionPlan;
            var masterRegistrant = checkoutObject.MasterRegistrant;
            var parentOrGuardian = masterRegistrant.GetParentOrGuardian(msgs, dc);
            var canAccountAcceptPayments = billingAccount.CanProcessPayments(msgs, dc);
            var registration = checkoutObject.Registration;

            var receiptRows = "";
            var regStatus = "";
            var registrantActivities = registration.GetRegistrantActivities(msgs, dc);
            var mainRA = registrantActivities.First(ra => ra.ParticipationObjectId == participationObject.ObjectID && ra.ParticipationObjectTypeId == participationObject.ObjectType);

            var otherRAs = new List<RegistrantActivity>();

            if (mainRA.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
                otherRAs = registrantActivities.Except(mainRA.AsList()).ToList();

            var registrationThankYou = participationObject.GetRegistrationThankYou(mainRA, msgs, dc);
            var cost = "";
            var ccFee = "";
            var additionalDetails = "";

            switch (mainRA.AssignmentTypeId)
            {
                case RegistrationAssignmentType.Reserved:
                case RegistrationAssignmentType.ReservedPendingPayment:
                case RegistrationAssignmentType.ReservedPendingParentalConsent:
                    regStatus = "Reserved";
                    break;
                case RegistrationAssignmentType.WaitingList:
                    regStatus = "Waiting List";
                    break;
                case RegistrationAssignmentType.Applied:
                    regStatus = "Applied";
                    break;
            }

            var amountDueOrPaid = "";
            var appliedOrWaitingList = new RegistrationAssignmentType[] { RegistrationAssignmentType.Applied, RegistrationAssignmentType.WaitingList };

            // if it's in apply mode, but the student could have been moved to registered even during apply mode registration
            // so we should treat them as registered
            if (mainRA.AssignmentTypeId.In(appliedOrWaitingList))
                amountDueOrPaid = "Amount Due (If Confirmed)";
            else if (mainRA.AssignmentTypeId == RegistrationAssignmentType.ReservedPendingParentalConsent)
                amountDueOrPaid = "Amount Due (If Parent Approved)";
            else if (!canAccountAcceptPayments || mainRA.AssignmentTypeId == RegistrationAssignmentType.ReservedPendingPayment)
                amountDueOrPaid = "Amount Due";
            else
                amountDueOrPaid = "Amount Paid";

            var description = $"{participationObject.Name} - {regStatus}";

            otherRAs.OrderBy(ra => ra.AssignmentTypeId).ToList().ForEach(ra => {
                description += $"<br />&nbsp;&bull;&nbsp;{ra.GetParticipationObject(msgs, dc).Name} - {ra.AssignmentTypeId.ToString().SplitByUpperCase()}";
            });

            // show this if no subscription plan, if there's a reg fee or there's a payment plan
            if (checkoutObject.IsFree || participationObject.Cost > 0.ToDecimal() || paymentPlan.IsNotEmpty())
            {
                if (checkoutObject.IsFree)
                {
                    cost = "FREE";
                    ccFee = "$0";
                }
                else if (checkoutObject.PaymentPlan.IsNotEmpty())
                {                    
                    cost = checkoutObject.GetPaymentPlanCostTodayWithConvenienceFee(msgs, dc).ToCurrency();
                    ccFee = checkoutObject.GetPaymentPlanCostTodayCreditCardFee(msgs, dc).ToCurrency();

                    var todayOrLater = "today";
                    var today = participationObject.OwnerAccount.ConvertToAccountTimeZone(DateTime.UtcNow);

                    if (checkoutObject.PaymentPlan.PaymentStartTypeId == PlanPaymentStartType.WhenActivityStarts && participationObject.StartDate.Value.IsOnOrAfter(today))
                        todayOrLater = $"on {participationObject.StartDate.ToShortDateString()}";

                    additionalDetails = $"{todayOrLater} plus {paymentPlan.GetTotalCostWithConvenienceFee(billingAccount, msgs, dc).ToCurrency()} for {(paymentPlan.TotalPeriods.Value - 1)} {paymentPlan.BillingPeriodTypeId.ToString().ToLower()}(s)";
                }
                else
                {
                    cost = (checkoutObject.RegistrationCostNoFees + checkoutObject.GetRegistrationConvenienceFee(msgs, dc)).ToCurrency();
                    ccFee = checkoutObject.GetRegistrationCreditCardFee(msgs, dc).ToCurrency();

                    additionalDetails = "registration fee";
                }

                // add registration fee row
                var registrationFeeModel = new NameValueCollection()
                {
                    { "Description", description },
                    { "Quantity",  "1"},
                    { "Cost", cost },
                    { "AdditionalDetails", additionalDetails }
                };

                receiptRows += ProcessTemplate(
                    EmailTemplates.Registration_Confirmation_Rows,
                    registrationFeeModel,
                    "Registration_Confirmation_Rows",
                    msgs);
            }

            // add subscription row
            if (subscription.IsNotEmpty())
            {
                description = $"Subscription for {participationObject.Name}";
                additionalDetails = $"per {subscription.BillingPeriodTypeId.ToString().ToLower()}";

                cost = checkoutObject.GetSubscriptionCostTodayWithConvenienceFee(msgs, dc).ToCurrency();
                ccFee = checkoutObject.GetSubscriptionCostTodayCreditCardFee(msgs, dc).ToCurrency();

                // add registration fee row
                var subscriptionFeeModel = new NameValueCollection()
                {
                    { "Description", description },
                    { "Quantity",  "1"},
                    { "Cost", cost },
                    { "AdditionalDetails", additionalDetails }
                };

                receiptRows += ProcessTemplate(
                    EmailTemplates.Registration_Confirmation_Rows,
                    subscriptionFeeModel,
                    "Registration_Confirmation_Rows",
                    msgs);
            };

            var registrationDetails = "Login to your account to view more details about this registration.";
            var loginLink = Utilities.LoginUrl();

            if (isParentCopy)
            {
                var parentUser = parentOrGuardian.GetOwner(msgs, dc);
                if (parentUser.IsNotEmpty() && !parentUser.IsRegistered)
                {
                    registrationDetails = "Create your account to review this registration.";
                    loginLink = parentUser.GetInviteUrl(purchaserAccount);
                }
            }

            if (customMessage.IsEmpty())
            {
                customMessage = registrationThankYou;

                if (isParentCopy)
                    customMessage = $"{masterRegistrant.FirstName} has registered for the following activity:";
                else if (loggedInUser.HasAccessTo(billingAccount))
                    customMessage = $"{masterRegistrant.FirstName} has been registered by {loggedInUser.DisplayName} at {billingAccount.Name} for the following activity:";
            }

            var model = new NameValueCollection()
            {
                { "FirstName", isParentCopy ? purchaser.FirstName : loggedInUser.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "LoginLink", loginLink },
                { "ActivityContactEmail", participationObject.ContactEmail.IfBlank(billingAccount.Email)},
                { "ActivityContactPhone", participationObject.ContactPhone.IfBlank(billingAccount.Phone)},
                { "ActivityContactName", participationObject.ContactName.IfBlank(billingAccount.Name)},
                { "BillerAccountName", billingAccount.Name },
                { "BillerAccountLink", billingAccount.HomePageUrl },
                { "AmountDueOrPaid", amountDueOrPaid },
                { "TotalCost", checkoutObject.GetTotalCostToPurchaseTodayWithFees(msgs, dc).ToCurrency() },
                { "BodyRows", receiptRows },
                { "CustomMessage", customMessage },
                { "CreditCardFee", ccFee },
                { "RegistrationDetails", registrationDetails}
            };
            
            var subject = $"{participationObject.Name} registration for {masterRegistrant.FirstName}";
            var body = ProcessTemplate(
                EmailTemplates.Registration_Confirmation,
                model,
                "Registration_Confirmation",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to registerer
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(loggedInUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)));

            // send bcc to activity contact
            if (sendBcc)
            {
                var emails = new List<string>();

                if (participationObject.ContactEmail.IsEmail() && participationObject.ContactEmail.ToLower().NotIn(emails))
                    emails.Add(participationObject.ContactEmail.ToLower());

                emails.ForEach(email =>
                    SendGridHelper.SendMail(
                        fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                        subject: $"Registration CC: {subject}",
                        body: body,
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)));
            }

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        public static void SendPaymentConfirmation(CheckoutObject checkoutObject, string customMessage, WebUser loggedInUser, bool sendBcc, SystemMessages msgs, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();
            
            var isAfterSchool = checkoutObject.PayingAccount.IsAfterSchool;
            var purchaserAccount = checkoutObject.PayingAccount;
            var purchaser = checkoutObject.PayingAccount.GetAccountOwner(msgs, dc);
            var billingAccount = checkoutObject.BillingAccount;
            var billingAccountOwner = billingAccount.GetAccountOwner(msgs, dc);
            var masterRegistrant = checkoutObject.MasterRegistrant;
            var parentOrGuardian = masterRegistrant.GetParentOrGuardian(msgs, dc);
            var canAccountAcceptPayments = billingAccount.CanProcessPayments(msgs, dc);
            var registration = checkoutObject.Registration;
            var totalCostWithFees = checkoutObject.GetStripeAmount(msgs, dc);
            var totalCost = totalCostWithFees - checkoutObject.PaymentTypeFeeOverrideAmount.Value;

            var receiptRows = "";

            // add registration fee row
            var subscriptionFeeModel = new NameValueCollection()
                {
                    { "Description", $"Payment to {billingAccount.Name}" },
                    { "Quantity",  "1"},
                    { "Cost", totalCost.ToCurrency() },
                    { "AdditionalDetails", "" }
                };

            receiptRows += ProcessTemplate(
                    EmailTemplates.Registration_Confirmation_Rows,
                    subscriptionFeeModel,
                    "Registration_Confirmation_Rows",
                    msgs);

            var model = new NameValueCollection()
            {
                { "FirstName", loggedInUser.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "LoginLink", Utilities.LoginUrl() },
                { "ActivityContactEmail", billingAccount.Email },
                { "ActivityContactPhone", billingAccount.Phone },
                { "ActivityContactName", billingAccount.Name },
                { "BillerAccountName", billingAccount.Name },
                { "BillerAccountLink", billingAccount.HomePageUrl },
                { "AmountDueOrPaid", "Amount Paid" },
                { "TotalCost", totalCostWithFees.ToCurrency() },
                { "BodyRows", receiptRows },
                { "CustomMessage", customMessage },
                { "CreditCardFee", checkoutObject.PaymentTypeFeeOverrideAmount.ToCurrency() },
                { "RegistrationDetails", "Login to your account to view more details about this payment."}
            };

            var subject = $"Payment receipt for {billingAccount.Name}";
            var body = ProcessTemplate(
                EmailTemplates.Registration_Confirmation,
                model,
                "Registration_Confirmation",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to registerer
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(loggedInUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)));

            // send bcc to activity contact
            if (sendBcc)
            {
                var emails = new List<string>() { billingAccount.Email };

                emails.ForEach(email =>
                    SendGridHelper.SendMail(
                        fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                        subject: $"Payment CC: {subject}",
                        body: body,
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)));
            }

            msgs.AddRange(mailMsgs);
        }

        public static SystemMessages SendPasswordReset(WebUser user, PasswordResetToken token, bool isAfterSchool)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "ResetPasswordLink", $"{Email.BaseUrl()}login/changepassword/?token={token.Uid.ToString()}" },
                { "ExpiresInDays", (token.ExpireDate.Value.Date - DateTime.UtcNow.Date).Days.ToString() },
                { "SupportEmail", Email.AppSupportEmailAddress },
            };

            var body = ProcessTemplate(
                EmailTemplates.Password_Reset,
                model,
                "passwordreset-1.0.1",
                msgs
                );

            var subject = $"Password reset request from {Utilities.AppName(isAfterSchool)}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static void SendRefundRequestToProvider(RefundRequest request, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var transaction = request.GetTransaction(msgs);
            var billingAccountOwner = transaction.GetBillerAccount(msgs).GetAccountOwner(msgs);
            var firstName = billingAccountOwner.FirstName;

            var model = new NameValueCollection()
            {
                { "FirstName", firstName },
                { "ProductName", Utilities.AppName(true) },
                { "RequesterFirstName", loggedInUser.FirstName },
                { "RequestAmount", request.Amount.Value.ToCurrency() },
                { "RequestReason", request.RequestReason },
                { "RequestViewUrl", $"{Utilities.BaseUrl()}/org/accounting/refundrequests/" },
            };

            var body = ProcessTemplate(
                EmailTemplates.Refund_Requested,
                model,
                "refund_requested-1.0.1",
                msgs
                );

            var subject = $"A refund has been requested by ${loggedInUser.DisplayName}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(billingAccountOwner.Email),              
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true)
                );

            // CC Support Team
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: $"CC: {subject}",
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true)
                ));

            Utilities.SystemErrorLog(mailMsgs);
        }

        public static void SendRefundRequestToSupport(RefundRequest request, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var model = new NameValueCollection()
            {
                { "FirstName", "Support" },
                { "ProductName", Utilities.AppName(true) },
                { "RequesterFirstName", loggedInUser.FirstName },
                { "RequestAmount", request.Amount.Value.ToCurrency() },
                { "RequestReason", request.RequestReason },
                { "RequestViewUrl", $"{Utilities.BaseUrl()}/admin/transaction/refundrequests/" },
            };

            var body = ProcessTemplate(
                EmailTemplates.Refund_Requested,
                model,
                "refund_requested-1.0.1",
                msgs
                );

            var subject = $"A refund has been requested/approved by {loggedInUser.DisplayName}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true)
                );

            Utilities.SystemErrorLog(mailMsgs);
        }

        public static void SendRefundRequestProcessed(RefundRequest request, Transaction refund, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var requester = request.GetCreator(msgs, dc);

            var model = new NameValueCollection()
            {
                { "FirstName",  requester.FirstName},
                { "ProductName", Utilities.AppName(true) },
                { "RefundAmount", refund.Amount.ToCurrency() },
                { "SupportEmail", Email.AppSupportEmailAddress },
            };

            var body = ProcessTemplate(
                EmailTemplates.Refund_Processed,
                model,
                "refund_processed-1.0.1",
                msgs
                );

            var subject = $"Your refund request has been approved and processed";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true));

            Utilities.SystemErrorLog(mailMsgs);
        }

        public static void SendRefundRequestDeniedByProvider(RefundRequest request, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var transaction = request.GetTransaction(msgs, dc);
            var providerAccount = transaction.GetBillerAccount(msgs, dc);
            var requester = request.GetCreator(msgs, dc);

            var model = new NameValueCollection()
            {
                { "FirstName", requester.FirstName },
                { "ProductName", Utilities.AppName(true) },
                { "ProviderAccountName", providerAccount.Name },
                { "RequestAmount", request.Amount.Value.ToCurrency() },
                { "DeniedReason", request.DeniedReason },
                { "SupportEmail", Email.AppSupportEmailAddress },
            };

            var body = ProcessTemplate(
                EmailTemplates.Refund_Denied,
                model,
                "refund_denied-1.0.1",
                msgs
                );

            var subject = $"Your refund request has been denied";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(requester.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true)
                );

            // CC Support Team
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: $"CC: {subject}",
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true)
                ));

            Utilities.SystemErrorLog(mailMsgs);
        }

        public static SystemMessages SendAttachedCoupon(AttachedCoupon attachedCoupon)
        {
            var msgs = new SystemMessages();
            var receivingAccountOwner = attachedCoupon.GetAccount(msgs).GetAccountOwner(msgs);
            var sendingAccount = attachedCoupon.GetCoupon(msgs).GetAccount(msgs);
            var sendingAccountOwner = sendingAccount.GetAccountOwner(msgs);
            var isAfterSchool = sendingAccount.IsAfterSchool;
            var coupon = attachedCoupon.GetCoupon(msgs);
            var purchaseableObjectCoupons = coupon.GetPurchaseableObjectCoupons(msgs);
            var purchaseableObject = purchaseableObjectCoupons.Select(purchaseableObjectCoupon => purchaseableObjectCoupon.GetPurchaseableObject(msgs)).First();

            var model = new NameValueCollection()
            {
                { "ReceivingFirstName", receivingAccountOwner.FirstName },
                { "SendingAccountName", sendingAccount.Name },
                { "CouponDiscount", coupon.GetAmountAsString() },
                { "PurchaseableObjectName", purchaseableObject.Name },
                { "CouponFeeType", coupon.AppliesTo.ToString().SplitByUpperCase() },
                { "CouponCode", coupon.CouponCode },
                { "RegistrationUrl", sendingAccount.HomePageUrl },
                { "GoodFor", $"{coupon.MaxNumberOfUsesPerPerson} use{(coupon.MaxNumberOfUsesPerPerson > 1 ? "s" : "")}" },
                { "ExpirationDate", coupon.ExpirationDate.ToShortDateString() },
                { "ContactName", sendingAccountOwner.DisplayName },
                { "ContactEmail", sendingAccount.Email },
                { "ProductName", sendingAccount.Name },
            };

            var body = ProcessTemplate(
                EmailTemplates.Coupon_Attached,
                model,
                "couponattached-1.0.1",
                msgs
                );

            var subject = $"You've received a coupon from {sendingAccount.Name}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(receivingAccountOwner.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendStripeAccountVerified(Account account, WebUser user, bool isAfterSchool)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "AccountName", account.Name },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "LoginLink", $"{Email.BaseUrl()}login" }
            };

            var body = ProcessTemplate(
                EmailTemplates.StripeAccount_Verified,
                model,
                "stripeaccountverified-1.0.0",
                msgs
                );

            var subject = $"You can now accept payments on {Utilities.AppName(isAfterSchool)}!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendSubscriptionPaymentFailed(Account account, WebUser user, SubscriptionPlan plan, bool isAfterSchool)
        {
            var msgs = new SystemMessages();

            var activityName = "";

            if (plan.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                activityName = plan.GetActivity().Name;
            else
                activityName = plan.GetActivityGroup().Name;

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "AccountName", account.Name },
                { "Period", $"{plan.BillingPeriodTypeId.ToString().ToLower()}ly" },
                { "PaymentAmount", plan.CostPerPeriod.ToCurrency() },
                { "ActivityName", activityName },
                { "LoginLink", $"{Email.BaseUrl()}login" }
            };

            var body = ProcessTemplate(
                EmailTemplates.SubscriptionPayment_Failed,
                model,
                "subscriptionpaymentfailed-1.0.0",
                msgs
                );

            var subject = $"Your payment failed for ${activityName}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendSubscriptionPaymentSucceeded(WebUser payingAccountUser, RegistrantSubscriptionPlan rsp, SubscriptionPlan plan)
        {
            var msgs = new SystemMessages();
            var participationObject = plan.GetParticipationObject(msgs);
            var billingPeriodTypeId = rsp.BillingPeriodTypeId;

            if (billingPeriodTypeId == BillingPeriodType.NotSet)
                billingPeriodTypeId = rsp.BillingPeriodTypeId;
            
            var model = new NameValueCollection()
            {
                { "FirstName", payingAccountUser.FirstName },
                { "AccountName", participationObject.OwnerAccount.Name },
                { "Period", $"{billingPeriodTypeId.ToString().ToLower()}ly" },
                { "PaymentAmount", rsp.TotalPaidWithFees.ToCurrency() },
                { "ActivityName", participationObject.Name },
                { "LoginLink", Utilities.LoginUrl() }
            };

            var subject = $"Your payment succeeded for {participationObject.Name}";

            var body = ProcessTemplate(
                EmailTemplates.SubscriptionPayment_Succeeded,
                model,
                "subscriptionpaymentsucceeded-1.0.1",
                msgs
                );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(payingAccountUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendEmailVerification(WebUser user, bool isAfterSchool)
        {
            var msgs = new SystemMessages();            

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "VerificationLink", $"{Email.BaseUrl()}signup/verify/{user.Uid.ToString()}" },
                { "SupportEmail", Email.AppSupportEmailAddress },
            };

            var body = ProcessTemplate(
                EmailTemplates.Email_Verification,
                model,
                "emailverification-1.0.1",
                msgs
                );

            var subject = $"{Utilities.AppName(isAfterSchool)}: Please verify your email address";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendWelcomeTrial(WebUser user, Account account)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "ProductName", Utilities.AppName(account.IsAfterSchool) }
            };

            var body = ProcessTemplate(
                EmailTemplates.Welcome_Trial,
                model,
                "welcometrial-1.0.1",
                msgs
                );

            var subject = $"Welcome to {Utilities.AppName(account.IsAfterSchool)}!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(account.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(account.IsAfterSchool),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: $"CC: {Utilities.AppName(account.IsAfterSchool)} TRIAL - {subject}",
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendWelcomePurchase(WebUser user, Account account)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "ProductName", Utilities.AppName(account.IsAfterSchool) },
                { "SupportEmail", Email.AppSupportEmailAddress }
            };

            var body = ProcessTemplate(
                EmailTemplates.Welcome_Purchase,
                model,
                "welcomepurchase",
                msgs
                );

            var subject = $"Welcome to the {Utilities.AppName(account.IsAfterSchool)} Family!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(account.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );

            // send copy to afterschool hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(account.IsAfterSchool),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: $"CC: {Utilities.AppName(account.IsAfterSchool)} SIGNUP - {subject}",
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendAccountInvitedEmail(WebUser invitedUser, Account invitedAccount, WebUser loggedInUser, string customMessage = null)
        {
            var msgs = new SystemMessages();

            if (customMessage.IsNotEmpty())
                customMessage = $"<p>{customMessage.Replace("\n", "<br />")}</p><hr />";

            var model = new NameValueCollection()
            {
                { "FirstName", invitedUser.FirstName },
                { "ProductName", Utilities.AppName() },
                { "SupportEmail", Email.AppSupportEmailAddress },
                { "InviteUrl", invitedUser.GetInviteUrl(invitedAccount) },
                { "CustomMessage", customMessage }
            };

            var body = ProcessTemplate(
                EmailTemplates.Account_Invited,
                model,
                "accountinvited",
                msgs
                );

            var subject = $"{loggedInUser.DisplayName} has invited you to use {Utilities.AppName(invitedAccount.IsAfterSchool)}!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(invitedAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(invitedUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(invitedAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static void SendSessionInviteEmail(Registrant contact, Account invitingAccount, ActivityGroup session, RegistrantActivityGroup rag, RegistrationPeriod period, WebUser loggedInUser, string customMessage = null, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            if (customMessage.IsNotEmpty())
                customMessage = $"<p>{customMessage.Replace("\n", "<br />")}</p><hr />";
            
            var acceptUrl = rag.GetInviteUrl();
            var declineUrl = rag.GetDeclineUrl();
            var regOpenText = "";

            if (period.IsNotEmpty())
            {
                if (period.IsOpen())
                    regOpenText = "Registration is open now.";
                else if (period.WillOpen())
                    regOpenText = $" Registration will open {period.StartDateTime.ToShortDateString()}.";
            }

            var model = new NameValueCollection()
            {
                { "FirstName", contact.FirstName },
                { "ProductName", Utilities.AppName(invitingAccount.IsAfterSchool) },
                { "SupportEmail", Email.AppSupportEmailAddress },
                { "AcceptLink", acceptUrl },
                { "DeclineLink", declineUrl },
                { "CustomMessage", customMessage },
                { "SenderDisplayName", loggedInUser.DisplayName },
                { "AccountName", invitingAccount.Name },
                { "SessionName", session.Name },
                { "RegistrationOpenText", regOpenText }
            };

            var body = ProcessTemplate(
                EmailTemplates.Session_Invite,
                model,
                "session_invite_1",
                msgs
                );

            var subject = $"You have been invited to register for {session.Name}!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(contact.Email),
                subject: subject,
                body: body,
                replyTo: new SendGrid.Helpers.Mail.EmailAddress(invitingAccount.Email, invitingAccount.Name),
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true));

            msgs.AddRange(mailMsgs);
        }

        public static void SendActivityInviteEmail(Registrant contact, Account invitingAccount, Activity activity, RegistrationPeriod period, WebUser loggedInUser, string customMessage = null, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            if (customMessage.IsNotEmpty())
                customMessage = $"<p>{customMessage.Replace("\n", "<br />")}</p><hr />";

            var regOpenText = "";
            var greetingName = contact.FirstName.IsNotEmpty() ? $" {contact.FirstName}" : "";

            if (period.IsNotEmpty())
            {
                if (period.IsOpen())
                    regOpenText = "Registration is open now.";
                else if (period.WillOpen())
                    regOpenText = $" Registration will open {period.StartDateTime.ToShortDateString()}.";
            }

            var model = new NameValueCollection()
            {
                { "Greeting", $"Hello{greetingName}" },
                { "ProductName", Utilities.AppName(invitingAccount.IsAfterSchool) },
                { "SupportEmail", Email.AppSupportEmailAddress },
                { "ViewLink", activity.ShortHomePageUrl },
                { "CustomMessage", customMessage },
                { "SenderDisplayName", loggedInUser.DisplayName },
                { "AccountName", invitingAccount.Name },
                { "ActivityName", activity.Name },
                { "RegistrationOpenText", regOpenText }
            };

            var body = ProcessTemplate(
                EmailTemplates.Activity_Invite,
                model,
                "activity_invite_1",
                msgs);

            var subject = $"You have been invited to register for {activity.Name}!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(true),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(contact.Email),
                subject: subject,
                body: body,
                replyTo: new SendGrid.Helpers.Mail.EmailAddress(invitingAccount.Email, invitingAccount.Name),
                emailTemplateId: SendGridHelper.GetGenericTemplateId(true));

            msgs.AddRange(mailMsgs);
        }

        public static SystemMessages SendLeagueAccountInvitedEmail(Account invitedAccount, ActivityGroup activityGroup, WebUser sendingUser, string customMessage = null)
        {
            var msgs = new SystemMessages();
            var invitedUser = invitedAccount.GetAccountOwner(msgs);
            var sendingAccount = activityGroup.GetAccount(msgs);

            if (customMessage.IsNotEmpty())
                customMessage = $"<p>{customMessage.Replace("\n", "<br />")}</p><hr />";

            var model = new NameValueCollection()
            {
                { "FirstName", invitedUser.FirstName },
                { "ProductName", Utilities.AppName(invitedAccount.IsAfterSchool) },
                { "SenderDisplayName", sendingUser.DisplayName },
                { "LoginLink", $"{Email.BaseUrl()}login" },
                { "CustomMessage", customMessage },
                { "RegistrationStartDate", activityGroup.GetCurrentOrNextRegistrationPeriod(msgs).StartDateTime.ToLongDateTimeString() },
                { "RegistrationEndDate", activityGroup.GetCurrentOrNextRegistrationPeriod(msgs).EndDateTime.ToLongDateTimeString() },
                { "ActivityHomePage", invitedAccount.HomePageUrl },
                { "LeagueAccountName", sendingAccount.Name },
                { "LeagueName", activityGroup.Name }
            };

            var subject = $"{sendingUser.DisplayName} has invited you to join their league in {Utilities.AppName(invitedAccount.IsAfterSchool)}!";
            var body = ProcessTemplate(
                EmailTemplates.LeagueSchool_Invited,
                model,
                "leagueschoolinvited",
                msgs
                );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(invitedAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(invitedUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(invitedAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendInviteStatusUpdatedEmail(Account invitedAccount, ActivityGroup activityGroup, WebUser sendingUser, InviteStatus inviteStatus)
        {
            var msgs = new SystemMessages();
            var invitedUser = invitedAccount.GetAccountOwner(msgs); 
            var sendingAccount = activityGroup.GetAccount(msgs);
            var leagueOwner = WebUser.Load(sendingAccount.OwnerId.Value);

            var model = new NameValueCollection()
            {
                { "FirstName", invitedUser.FirstName },
                { "ProductName", Utilities.AppName(invitedAccount.IsAfterSchool) },
                { "SenderDisplayName", sendingUser.DisplayName },
                { "RegistrationStartDate", activityGroup.GetCurrentOrNextRegistrationPeriod(msgs).StartDateTime.ToLongDateTimeString() },
                { "RegistrationEndDate", activityGroup.GetCurrentOrNextRegistrationPeriod(msgs).EndDateTime.ToLongDateTimeString() },
                { "LeagueAccountName", sendingAccount.Name },
                { "LeagueName", activityGroup.Name },
                { "InviteStatus", inviteStatus.ToString() }
            };

            var subject = $"{sendingUser.DisplayName} has {inviteStatus} your invitation to {activityGroup.Name} in {Utilities.AppName(invitedAccount.IsAfterSchool)}";
            var body = ProcessTemplate(
                EmailTemplates.LeagueInviteStatusUpdate_Notification,
                model,
                "leagueinvitestatusupdatenotification",
                msgs
                );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(invitedAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(leagueOwner.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(invitedAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendAccessRequestToHost(Account visitorAccount, Account hostAccount)
        {
            var msgs = new SystemMessages();
            var hostAccountOwner = hostAccount.GetAccountOwner(msgs);
            var visitorAccountOwner = visitorAccount.GetAccountOwner(msgs);

            var model = new NameValueCollection()
            {
                { "HostFirstName", hostAccountOwner.FirstName },
                { "HostAccountName", hostAccount.Name},
                { "VisitorAccountName", visitorAccount.Name },
                { "VisitorUserDisplayName", visitorAccountOwner.DisplayName },
                { "ProductName", Utilities.AppName(hostAccount.IsAfterSchool) },
                { "LoginLink", $"{Email.BaseUrl()}login" }
            };

            var subject = $"{visitorAccount.Name} is requesting permission to list after school activities at your location";
            var body = ProcessTemplate(
               EmailTemplates.VisitorAccount_Requested,
               model,
               "visitoraccountrequested",
               msgs
               );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(hostAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(hostAccount.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(hostAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendApprovalNotificationToVisitor(Account visitorAccount, Account hostAccount, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", visitorAccount.GetAccountOwner(msgs).DisplayName },
                { "HostAccountName", hostAccount.Name},
                { "VisitorAccountName", visitorAccount.Name },
                { "ProductName", Utilities.AppName(hostAccount.IsAfterSchool) },
                { "LoginLink", $"{Email.BaseUrl()}login" }
            };            

            var subject = $"You have been approved to list activities at {hostAccount.Name}";
            var body = ProcessTemplate(
               EmailTemplates.VisitorAccount_Approved,
               model,
               "visitoraccountapproved",
               msgs
               );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(hostAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(visitorAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendDenialNotificationToVisitor(Account visitorAccount, Account hostAccount)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", visitorAccount.GetAccountOwner(msgs).DisplayName },
                { "HostAccountName", hostAccount.Name},
                { "VisitorAccountName", visitorAccount.Name },
                { "ProductName", Utilities.AppName(hostAccount.IsAfterSchool) },
                { "LoginLink", $"{Email.BaseUrl()}login" }
            };

            var subject = $"You have been denied to list activities at {hostAccount.Name}";
            var body = ProcessTemplate(
               EmailTemplates.VisitorAccount_Denied,
               model,
               "visitoraccountdenied",
               msgs
               );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(hostAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(visitorAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendTeamSubmmittedToDistrictNotifitication(Account visitorAccount, Account hostAccount, Activity activity, ActivityGroup activityGroup, bool isSubmitting)
        {
            var msgs = new SystemMessages();
            var verb = isSubmitting ? "submitted" : "withdrawn";
            var preposition = isSubmitting ? "to" : "from";

            var model = new NameValueCollection()
            {
                { "OrgName", visitorAccount.Name},
                { "ActivityName", activity.Name },
                { "LeagueName", activityGroup.Name },
                {"Verb", verb },
                {"Preposition", preposition },

            };

            var subject = $"{activity.Name} has been {verb} {preposition} {activityGroup.Name}";
            var body = ProcessTemplate(
               EmailTemplates.TeamSubmittedToDistrict_Notification,
               model,
               "teamsubmittedtodistrict",
               msgs
               );

            var mailMsgsHost = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(hostAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(hostAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );
            var mailMsgsVisitor = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(hostAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(visitorAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgsHost);
            msgs.AddRange(mailMsgsVisitor);
            return msgs;
        }

        public static SystemMessages SendProviderNewActivityNotification(Account hostAccount, Account visitorAccount, WebUser visitorAccountOwner, Activity activity, WebUser loggedInUser)
        {
            var msgs = new SystemMessages();
            var getStartedLink = $"{Email.BaseUrl()}login";
            var getStartedText = "Login to view your account";

            if (!visitorAccount.IsActive)
            {
                getStartedLink = $"{visitorAccountOwner.GetInviteUrl(visitorAccount)}";
                getStartedText = "Let us know how to pay you";
            }

            var model = new NameValueCollection()
            {
                { "FirstName", visitorAccountOwner.DisplayName },
                { "HostAccountName", hostAccount.Name},
                { "AmountCharged", activity.CostPerRegistrant.ToCurrency() },
                { "ProductName", Utilities.AppName(hostAccount.IsAfterSchool) },
                { "GetStartedLink", getStartedLink },
                { "GetStartedText", getStartedText },
                { "HomePageUrl", hostAccount.HomePageUrl },
                { "ActivityHomePageUrl", activity.HomePageUrl },
                { "ActivityName", activity.Name },
                { "SupportEmail", Email.AppSupportEmailAddress },
                { "InviterFirstName", loggedInUser.FirstName }
            };

            var subject = $"You have been designated as the provider for {activity.Name} at {hostAccount.Name}";
            var body = ProcessTemplate(
               EmailTemplates.ProviderAccount_Added,
               model,
               "provideraccountadded",
               msgs);

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(hostAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(visitorAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendPayoutRequestNotification(Payout payout, Account toAccount)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "AccountName", toAccount.Name },
                { "PayoutAmount", payout.Amount.ToCurrency() },
                { "LoginLink", Utilities.LoginUrl($"{Utilities.BaseUrl()}/admin/payout/") },
            };

            var subject = $"A new payout has been requested by {toAccount.Name}";
            var body = ProcessTemplate(
                EmailTemplates.Payout_Request,
                model,
                "Payout_Request-1.00",
                msgs);

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId());

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        public static SystemMessages SendPublishRequestNotification(Activity activity, Account locationOwner, WebUser accountOwner, WebUser loggedInUser)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "AccountOwnerName", accountOwner.FirstName },
                { "CurrentUserName", loggedInUser.DisplayName },
                { "ActivityName", activity.Name },
                { "AccountName", locationOwner.Name },
                { "LoginLink", Utilities.LoginUrl(activity.PreviewUrl) },
                { "ProductName", Utilities.AppName() },
            };

            var subject = $"{loggedInUser.DisplayName} has requested to publish an activity";
            var body = ProcessTemplate(
                EmailTemplates.Publish_Request,
                model,
                "Publish_Request-1.00",
                msgs);

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(accountOwner.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId());

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        internal static string ProcessTemplate(byte[] emailTemplate, NameValueCollection model, string cacheId, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            try
            {                   
                using (Stream stream = new MemoryStream(emailTemplate))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string template = reader.ReadToEnd(); 
                    
                        foreach (string key in model.Keys)
                            template = template.Replace($"@Model.{key}", model[key]);

                        return template;
                    }
            }
            catch(Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
            }

            return "";
        }
    }
}
