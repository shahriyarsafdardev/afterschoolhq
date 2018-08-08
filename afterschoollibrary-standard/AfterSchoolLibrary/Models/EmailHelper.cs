using System;
using BasicLibrary;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;
using AfterSchoolLibrary.Services;
using System.Linq;

namespace AfterSchoolLibrary
{
    public class EmailHelper
    {
        public static SystemMessages SendEmail(string toEmail, string subject, string body, bool isAfterSchool)
        {
            var msgs = new SystemMessages();
            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
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
                { "PaymentLink", $"{Utilities.BaseUrl(isAfterSchool)}/account/makepayment/{registration.Uid}" },
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
                fromEmail: Email.SendGridAppEmailAddress(participationObject.OwnerAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(parentOrGuardian.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(participationObject.OwnerAccount.IsAfterSchool));

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        public static SystemMessages SendRegistrationConfirmed(CheckoutObject checkoutObject, bool sendBcc)
        {
            var msgs = new SystemMessages();
            var isAfterSchool = checkoutObject.Purchaser.IsAfterSchool;
            var purchaserAccount = checkoutObject.Purchaser;
            var purchaser = checkoutObject.Purchaser.GetAccountOwner(msgs);
            var participationObject = checkoutObject.ParticipationObject;
            var billingAccount = checkoutObject.ParticipationObject.OwnerAccount;
            var billingAccountOwner = billingAccount.GetAccountOwner(msgs);
            var paymentPlan = checkoutObject.PaymentPlan;
            var subscription = checkoutObject.SubscriptionPlan;
            var registrant = checkoutObject.Registrant;
            var parentOrGuardian = registrant.GetParentOrGuardian(msgs);
            var stripeAccount = billingAccount.GetStripeAccount(msgs);
            var canAccountAcceptPayments = stripeAccount.IsNotEmpty() && stripeAccount.IsVerified;

            var receiptRows = "";
            var regStatus = "";
            var registeredOrApplied = "registered";
            var cost = "";
            var registrantActivity = checkoutObject.Registration.GetRegistrantActivities().First();
            var registrationThankYou = participationObject.GetRegistrationThankYou(registrantActivity, msgs);

            switch(registrantActivity.AssignmentTypeId)
            {
                case RegistrationAssignmentType.Reserved:
                    regStatus = "Reserved";
                    break;
                case RegistrationAssignmentType.WaitingList:
                    regStatus = "Waiting List";
                    break;
                case RegistrationAssignmentType.Applied:
                    regStatus = "Applied";
                    registeredOrApplied = "applied";
                    break;
            }

            var description = $"{participationObject.Name} - {regStatus}";
            var amountDueOrPaid = "";

            // if it's in apply mode, but the student could have been moved to registered even during apply mode registration
            // so we should treat them as registered
            if (registrantActivity.AssignmentTypeId == RegistrationAssignmentType.Applied)
                amountDueOrPaid = "Amount Due (If Confirmed)";
            else if (!canAccountAcceptPayments)
                amountDueOrPaid = "Amount Due";
            else
                amountDueOrPaid = "Amount Paid";

            // show this if no subscription plan, if there's a reg fee or there's a payment plan
            if (checkoutObject.IsFree || participationObject.Cost > 0.ToDecimal() || paymentPlan.IsNotEmpty())
            {
                if (checkoutObject.IsFree)
                    cost = "FREE";
                else if (checkoutObject.PaymentPlan.IsNotEmpty())
                    cost = $"{checkoutObject.PaymentPlanCostToday.ToCurrency()} today plus {paymentPlan.CostPerPeriod.ToCurrency()} for {(paymentPlan.TotalPeriods.Value - 1)} {paymentPlan.BillingPeriodTypeId.ToString().ToLower()}(s)";
                else
                    cost = $"{participationObject.Cost.ToCurrency()} - Registration fee";

                // add registration fee row
                var registrationFeeModel = new NameValueCollection()
                {
                    { "Description", description },
                    { "Quantity",  "1"},
                    { "Cost", cost }
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
                cost = $"{subscription.CostPerPeriod.ToCurrency()} per {subscription.BillingPeriodTypeId.ToString().ToLower()}";

                // add registration fee row
                var subscriptionFeeModel = new NameValueCollection()
                {
                    { "Description", description },
                    { "Quantity",  "1"},
                    { "Cost", cost }
                };

                receiptRows += ProcessTemplate(
                    EmailTemplates.Registration_Confirmation_Rows,
                    subscriptionFeeModel,
                    "Registration_Confirmation_Rows",
                    msgs);
            };

            var model = new NameValueCollection()
            {
                { "FirstName", purchaser.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "LoginLink", $"{Utilities.LoginUrl(isAfterSchool)}" },
                { "ActivityContactEmail",  participationObject.ContactEmail},
                { "ActivityContactPhone",  participationObject.ContactPhone},
                { "ActivityContactName",  participationObject.ContactName},
                { "BillerAccountName", billingAccount.Name },
                { "BillerAccountLink", billingAccount.HomePageUrl },
                { "AmountDueOrPaid", amountDueOrPaid },
                { "TotalCost", checkoutObject.TotalCostToPurchaseToday.ToCurrency() },
                { "BodyRows", receiptRows },
                { "CustomMessage", registrationThankYou }
            };

            var subject = $"{registrant.FirstName} has {registeredOrApplied} for {participationObject.Name}!";
            var body = ProcessTemplate(
                EmailTemplates.Registration_Confirmation,
                model,
                "Registration_Confirmation",
                msgs);

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(parentOrGuardian.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool));

            // send bcc
            if (sendBcc)
                SendGridHelper.SendMail(
                    fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(billingAccountOwner.Email),
                    subject: $"New Registration CC: {subject}",
                    body: body,
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool));

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        public static SystemMessages SendPasswordReset(WebUser user, PasswordResetToken token, bool isAfterSchool)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "ResetPasswordLink", $"{Email.BaseUrl(isAfterSchool)}login/changepassword/?token={token.Uid.ToString()}" },
                { "ExpiresInDays", (token.ExpireDate.Value.Date - DateTime.UtcNow.Date).Days.ToString() },
                { "SupportEmail", Email.AppSupportEmailAddress(isAfterSchool) },
            };

            var body = ProcessTemplate(
                EmailTemplates.Password_Reset,
                model,
                "passwordreset-1.0.1",
                msgs
                );

            var subject = $"Password reset request from {Utilities.AppName(isAfterSchool)}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
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
                { "LoginLink", $"{Email.BaseUrl(isAfterSchool)}login" }
            };

            var body = ProcessTemplate(
                EmailTemplates.StripeAccount_Verified,
                model,
                "stripeaccountverified-1.0.0",
                msgs
                );

            var subject = $"You can now accept payments on {Utilities.AppName(isAfterSchool)}!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
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
                { "LoginLink", $"{Email.BaseUrl(isAfterSchool)}login" }
            };

            var body = ProcessTemplate(
                EmailTemplates.SubscriptionPayment_Failed,
                model,
                "subscriptionpaymentfailed-1.0.0",
                msgs
                );

            var subject = $"Your payment failed for ${activityName}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendSubscriptionPaymentSucceeded(Account account, WebUser user, SubscriptionPlan plan, bool isAfterSchool)
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
                { "LoginLink", $"{Email.BaseUrl(isAfterSchool)}login" }
            };

            var body = ProcessTemplate(
                EmailTemplates.SubscriptionPayment_Succeeded,
                model,
                "subscriptionpaymentsucceeded-1.0.0",
                msgs
                );

            var subject = $"Your payment succeeded for ${activityName}";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
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
                { "VerificationLink", $"{Email.BaseUrl(isAfterSchool)}signup/verify/{user.Uid.ToString()}" },
                { "SupportEmail", Email.AppSupportEmailAddress(isAfterSchool) },
            };

            var body = ProcessTemplate(
                EmailTemplates.Email_Verification,
                model,
                "emailverification-1.0.1",
                msgs
                );

            var subject = $"{Utilities.AppName(isAfterSchool)}: Please verify your email address";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(isAfterSchool),
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
                fromEmail: Email.SendGridAppEmailAddress(account.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(account.IsAfterSchool),
                toEmail: Email.SendGridAppAdminEmailAddress,
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
                { "SupportEmail", Email.AppSupportEmailAddress(account.IsAfterSchool) }
            };

            var body = ProcessTemplate(
                EmailTemplates.Welcome_Purchase,
                model,
                "welcomepurchase",
                msgs
                );

            var subject = $"Welcome to the {Utilities.AppName(account.IsAfterSchool)} Family!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(account.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(account.IsAfterSchool),
                toEmail: Email.SendGridAppAdminEmailAddress,
                subject: $"CC: {Utilities.AppName(account.IsAfterSchool)} PURCHASE - {subject}",
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
                { "ProductName", Utilities.AppName(invitedAccount.IsAfterSchool) },
                { "SupportEmail", Email.AppSupportEmailAddress(invitedAccount.IsAfterSchool) },
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
                fromEmail: Email.SendGridAppEmailAddress(invitedAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(invitedUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(invitedAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
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
                { "LoginLink", $"{Email.BaseUrl(invitedAccount.IsAfterSchool)}login" },
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
                fromEmail: Email.SendGridAppEmailAddress(invitedAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(invitedUser.Email),
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
                { "LoginLink", $"{Email.BaseUrl(hostAccount.IsAfterSchool)}login" }
            };

            var subject = $"{visitorAccount.Name} is requesting permission to list after school activities at your location";
            var body = ProcessTemplate(
               EmailTemplates.VisitorAccount_Requested,
               model,
               "visitoraccountrequested",
               msgs
               );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(hostAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(hostAccount.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: Email.SendGridAppAdminEmailAddress,
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(hostAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        public static SystemMessages SendApprovalNotificationToVisitor(Account visitorAccount, Account hostAccount)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", visitorAccount.GetAccountOwner(msgs).DisplayName },
                { "HostAccountName", hostAccount.Name},
                { "VisitorAccountName", visitorAccount.Name },
                { "ProductName", Utilities.AppName(hostAccount.IsAfterSchool) },
                { "LoginLink", $"{Email.BaseUrl(hostAccount.IsAfterSchool)}login" }
            };            

            var subject = $"You have been approved to list activities at {hostAccount.Name}";
            var body = ProcessTemplate(
               EmailTemplates.VisitorAccount_Approved,
               model,
               "visitoraccountapproved",
               msgs
               );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(hostAccount.IsAfterSchool),
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
                { "LoginLink", $"{Email.BaseUrl(hostAccount.IsAfterSchool)}login" }
            };

            var subject = $"You have been denied to list activities at {hostAccount.Name}";
            var body = ProcessTemplate(
               EmailTemplates.VisitorAccount_Denied,
               model,
               "visitoraccountdenied",
               msgs
               );

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(hostAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(visitorAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }

        private static string ProcessTemplate(byte[] emailTemplate, NameValueCollection model, string cacheId, SystemMessages msgs = null)
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
