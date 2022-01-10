using System;
using BasicLibrary;
using System.IO;
using System.Collections.Specialized;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Linq;
using System.Collections.Generic;
using BasicLibrary.DataAccess;
using System.Web;

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

        public static SystemMessages SendEmailToSupport(string subject, string body)
        {
            var msgs = new SystemMessages();
            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: AfterSchoolLibrary.Helpers.Email.SendGridSupportEmailAddress,
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId()
            );

            msgs.AddRange(mailMsgs);

            return msgs;
        }

        public static SystemMessages SendPaymentReminder(RegistrantActivity ra)
        {
            var msgs = new SystemMessages();
            var registration = ra.GetRegistration();
            var participationObject = ra.GetParticipationObject(msgs);

            msgs.AddRange(SendPaymentReminder(participationObject, registration));

            return msgs;
        }

        public static SystemMessages SendPaymentReminder(ParticipationObject participationObject, Registration registration)
        {
            var msgs = new SystemMessages();
            var registrant = registration.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var parentOrGuardian = masterRegistrant.GetParentOrGuardian(msgs);
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
                { "ProductName", Utilities.AppName() },
                { "StudentName", registrant.FirstName }
            };

            var subject = $"It's time to finalize your registration for {registrant.FirstName}";
            var body = ProcessTemplate(
                        EmailTemplates.Payment_Reminder,
                        model,
                        "Payment_Reminder-1.03",
                        msgs);

            var mailMsgs = SendGridHelper.SendMail(
                            fromEmail: Email.SendGridNoReplyEmailAddress(),
                            toEmail: new SendGrid.Helpers.Mail.EmailAddress(parentOrGuardian.Email),
                            subject: subject,
                            body: body,
                            emailTemplateId: SendGridHelper.GetGenericTemplateId());

            msgs.AddRange(mailMsgs);

            return msgs;
        }


        public static SystemMessages SendSurveyEmail(int surveyResponseId, int registrantId, int surveyRequestId, string code, int codeId, SystemMessages msgs)
        {
            msgs = msgs ?? new SystemMessages();

            var registrant = Registrant.Load(registrantId);
            var survey = SurveyRequest.Load(surveyRequestId);
            var registrantParent = registrant.GetParentOrGuardian(msgs);
            var model = new NameValueCollection()
            {
                { "Name", registrant.Name },
                { "Link", Utilities.SurveyBaseUrl+"/survey/"+ surveyResponseId},
                { "SignUpLink", Utilities.SurveyBaseUrl+"/onboarding/signup"},                
                { "SurveyName", survey.Name}
            };

            if (code.IsNotEmpty())
            {
                model.Add(new NameValueCollection() { { "InviteCode", code } });
                model.Add(new NameValueCollection() { { "AutoSignUpLink", Utilities.SurveyBaseUrl + "/onboarding/signup/" + codeId } });
            }

            var subject = $"You have received a survey request";
            var body = ProcessTemplate(
                        code.IsNotEmpty() ? EmailTemplates.Survey_Code : EmailTemplates.Survey,
                        model,
                        code.IsNotEmpty() ? "survey_code_email" : "survey_code",
                        msgs);

            // check if child and parent has same email
            var isRegistrantEmailSameAsParent = (registrant.Email == registrantParent?.Email);
            if (isRegistrantEmailSameAsParent)
                SendSurveyEmailLastStep(subject, registrant.Email, body, msgs);
            else
            {
                //send both
                SendSurveyEmailLastStep(subject, registrant.Email, body, msgs);
                if (registrantParent.IsNotEmpty())
                    SendSurveyEmailLastStep(subject, registrantParent.Email, body, msgs);
            }

            return msgs;
        }

        public static void SendSurveyEmailLastStep(string subject, string email, string body, SystemMessages msgs)
        {
            var mailMsgs = SendGridHelper.SendMail(
                         fromEmail: Email.SendGridNoReplyEmailAddress(),
                         toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                         subject: subject,
                         body: body,
                         emailTemplateId: SendGridHelper.GetGenericTemplateId());

            msgs.AddRange(mailMsgs);
        }


        public static SystemMessages SendAccountConfirmationEmail(ParticipationObject participationObject, Registration registration)
        {
            var msgs = new SystemMessages();
            var registrant = registration.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var parentOrGuardian = masterRegistrant.GetParentOrGuardian(msgs);
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
                { "ProductName", Utilities.AppName() },
                { "StudentName", registrant.FirstName }
            };

            var subject = $"It's time to finalize your registration for {registrant.FirstName}";
            var body = ProcessTemplate(
                        EmailTemplates.Payment_Reminder,
                        model,
                        "Payment_Reminder-1.03",
                        msgs);

            var mailMsgs = SendGridHelper.SendMail(
                            fromEmail: Email.SendGridNoReplyEmailAddress(),
                            toEmail: new SendGrid.Helpers.Mail.EmailAddress(parentOrGuardian.Email),
                            subject: subject,
                            body: body,
                            emailTemplateId: SendGridHelper.GetGenericTemplateId());

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

        public static void SendContactProviderEmail(Account account, Registrant contact, SystemMessages msgs)
        {
            var claimProfileDetails = "";

            if (!account.IsClaimed)
                claimProfileDetails = $"<p><a href=\"{Utilities.BaseUrl()}\">{Utilities.AppName()}</a> is promoting your program in our online marketplace to enable thousands of parents to quickly find programs for their children. You can view your profile profile here: <a href=\"{account.ShortHomePageUrl}\">{account.ShortHomePageUrl}</a>. <br /><br />Ready to claim your profile? Click the following link: {account.ClaimAccountUrl}</p>";

            var model = new NameValueCollection()
            {
                { "ContactName", contact.Name },
                { "ContactEmail",  contact.Email},
                { "ContactPhone", contact.Phone },
                { "ClaimProfileDetails", claimProfileDetails },
                { "ProductName", Utilities.AppName() }
            };

            var subject = $"{contact.Name} has submitted a contact form from {Utilities.AppName()}";
            var body = ProcessTemplate(
                EmailTemplates.ContactForm_Submitted,
                model,
                "ContactForm_Submitted",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to activity contact
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId()));
        }

        public static void SendStripeHasUnfulFilledRequirementsEmail(Stripe.Account stripeAccount, List<string> requirementList, StripeRequirementStatus requirementStatus, Account account, SystemMessages msgs)
        {
            var unFulfilledRequirements = "<ul>";

            requirementList.ForEach(req =>
            {
                unFulfilledRequirements += $"<li>{req}</li>";
            });

            unFulfilledRequirements += "</ul>";

            var model = new NameValueCollection()
            {
                { "UnfulfilledRequirements", unFulfilledRequirements },
                { "RequirementStatus", requirementStatus.ToString().SplitByUpperCase() },
                { "StripeBusinessName", stripeAccount.BusinessProfile.Name },
                { "StripeAccountEmail", stripeAccount.Email },
                { "AHQAccountName", account.Name },
                { "AHQAccountOwner", account.GetAccountOwner(msgs).DisplayName },
                { "AHQAccountEmail", account.Email },
                { "AccountName", Utilities.AppName() }
            };

            var subject = $"{account.Name} is missing information On it's Stripe Account";
            var body = ProcessTemplate(
                EmailTemplates.Stripe_Has_Unfulfilled_Requirements,
                model,
                "StripeHasUnfulfilledRequirements-1.0",
                msgs);

            var mailMsgs = new SystemMessages();

            mailMsgs.AddRange(
                SendGridHelper.SendMail(
                    fromEmail: Email.AppCustomersEmailAddress,
                    toEmail: Email.AppSupportEmailAddress,
                    subject: subject,
                    body: body,
                    emailTemplateId: SendGridHelper.GetGenericTemplateId()));
        }

        public static void SendClaimEmail(Account account, SystemMessages msgs)
        {
            var claimProfileDetails = $"<p><a href=\"{Utilities.BaseUrl()}\">{Utilities.AppName()}</a> is promoting your program in our online marketplace to enable thousands of parents to quickly find programs for their children. You can view your profile profile here: <a href=\"{account.ShortHomePageUrl}\">{account.ShortHomePageUrl}</a>. <br /><br />Ready to claim your profile? Click the following link: {account.ClaimAccountUrl} or forward this email to a team member to claim your <em>free</em> profile</p>";

            var model = new NameValueCollection()
            {
                { "ClaimProfileDetails", claimProfileDetails },
                { "ProductName", Utilities.AppName() }
            };

            var subject = $"Someone has requested to claim your free profile on {Utilities.AppName()}";
            var body = ProcessTemplate(
                EmailTemplates.ClaimForm_Submitted,
                model,
                "ClaimForm_Submitted",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to activity contact
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId()));
        }

        public static void SendClaimProfileEmail(Account account, Guid userGuid, string email, SystemMessages msgs)
        {
            var subject = $"Thanks for claiming your profile on {Utilities.AppName()}";
            var claimProfileDetails = $"<p>Click following <a href=\"{Utilities.CompleteSignUpUrl(userGuid)}\">Link</a> to claim your profile.<p>";

            var model = new NameValueCollection()
            {
                { "ClaimProfileDetails", claimProfileDetails },
                { "ProductName", Utilities.AppName() }
            };
            
            var body = ProcessTemplate(
                EmailTemplates.ClaimProfile_Send,
                model,
                "ClaimProfile_Send",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to activity contact
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId()));
        }

        public static void SendNonRecognizedProfileClaimEmail(Account account, WebUser requester, SystemMessages msgs)
        {
            var subject = $"{requester.FirstName} {requester.LastName} is requesting to claim your profile on AfterSchool HQ.";
            var profileClaimLink = Utilities.CompleteSignUpUrl(requester.Uid);

            var model = new NameValueCollection()
            {
                { "OwnerFirstName", account.GetAccountOwner()?.FirstName },
                { "RequestorFirstName", requester.FirstName },
                { "RequestorEmail", requester.Email },
                { "ProfileClaimLink", profileClaimLink },
                { "ProductName", Utilities.AppName() }
            };

            var body = ProcessTemplate(
                EmailTemplates.ClaimProfileNonRecognized_Send,
                model,
                "ClaimProfileNonRecognized_Send",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to activity contact
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId()));
        }

        public static void SendProfileClaimedEmail(Account account, string firstName, string lastName, string email, SystemMessages msgs)
        {
            var subject = $"{firstName} {lastName} is requesting access to your account on AfterSchool HQ.";
            var teamPage = $"{Utilities.BaseUrl()}/org/account/team/";

            var model = new NameValueCollection()
            {
                { "OwnerFirstName", account.GetAccountOwner()?.FirstName },
                { "RequestorFirstName", firstName },
                { "RequestorEmail", email },
                { "TeamPage", teamPage },
                { "ProductName", Utilities.AppName() }
            };

            var body = ProcessTemplate(
                EmailTemplates.ClaimProfileRequestAccess_Send,
                model,
                "ClaimProfileRequestAccess_Send",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to activity contact
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
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
            var actionLink = Utilities.LoginUrl();
            var registrationDetails = "Login to your account to view more details.";
            var bccPrepend = $"<p>{masterRegistrant.FirstName} has registered interest for the following activity. Please reach out to them or add them to your mailing list</p><p>------------------</p>";

            if (customMessage.IsEmpty())
            {
                customMessage = interestFormThankYou;

                if (interestFormThankYou.IsNotEmpty())
                    customMessage = "Thank you for your interest. We will be in touch with you soon to follow up.";
            }

            if (!loggedInUser.IsRegistered)
            {
                actionLink = Utilities.CompleteSignUpUrl(loggedInUser.Uid);
                registrationDetails = "Click here to complete your account sign up";
            }

            var model = new NameValueCollection()
            {
                { "FirstName", loggedInUser.FirstName },
                { "ProductName", Utilities.AppName(true) },
                { "SupportEmail", Email.AppSupportEmailAddress },
                { "CustomMessage", customMessage },
                { "ActionLink", actionLink },
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

            // Send BCC
            var emails = new List<string>();

            if (participationObject.ContactEmail.IsEmail() && participationObject.ContactEmail.ToLower().NotIn(emails))
                emails.Add(participationObject.ContactEmail.ToLower());

            emails.ForEach(email =>
                SendGridHelper.SendMail(
                    fromEmail: Email.SendGridNoReplyEmailAddress(true),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                    subject: $"Interest Form CC: {subject}",
                    body: $"{bccPrepend}{body}",
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(true)));

            msgs.AddRange(mailMsgs);
        }

        public static void SendContactFormConfirmation(Account account, Registrant contact, SystemMessages msgs)
        {
            var accountOwner = account.GetAccountOwner(msgs);
            var questionsAndAnswers = "";
            var answers = contact.GetAnswers(msgs).OrderBy(answer => answer.FormQuestionDisplayOrder).ToList();
            var groups = answers.Select(answer => answer.ToFormQuestionGroup()).DistinctBy(group => group.ID.Value).OrderBy(group => group.DisplayOrder).ToList();

            questionsAndAnswers = "<p><table border=\"0\" cellpadding=\"5\">";

            groups.ForEach(group => {

                questionsAndAnswers += $"<tr><td colspan=\"{group.Name}\"></td></tr>";

                var groupAnswers = answers.Where(answer => answer.FormQuestionFormQuestionGroupId == group.ID.Value).ToList();

                groupAnswers.ForEach(answer => {
                    questionsAndAnswers += $"<tr><td>{answer.QuestionText}</td><td>{answer.Text}</td></tr>";
                });
            });

            questionsAndAnswers += "</table></p>";

            var model = new NameValueCollection()
            {
                { "ContactName", contact.Name },
                { "QuestionsAndAnswers", questionsAndAnswers },
                { "ProductName", Utilities.AppName(true) },
            };

            var body = ProcessTemplate(
                        EmailTemplates.ContactFormCustom_Submitted,
                        model,
                        "contact-submitted-custom-1.0.0",
                        msgs);

            var emails = account.Email.AsList();

            if (accountOwner.Email.IsEmail() && accountOwner.Email.ToLower().NotIn(emails))
                emails.Add(accountOwner.Email.ToLower());

            emails.ForEach(email =>
                msgs.AddRange(SendGridHelper.SendMail(
                    fromEmail: Email.SendGridNoReplyEmailAddress(true),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                    subject: $"Contact Form Submitted by {contact.Name}",
                    body: $"{body}",
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(true))));
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
            var postPayPlan = checkoutObject.PostPayPlan;
            var masterRegistrant = checkoutObject.MasterRegistrant;
            var parentOrGuardian = masterRegistrant.GetParentOrGuardian(msgs, dc);
            var canAccountAcceptPayments = billingAccount.CanProcessPayments(msgs, dc);
            var registration = checkoutObject.Registration;
            var submittedRecord = registration.GetRegistrant(msgs, dc);

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

            if (participationObject.CanRegisterPerClass)
                checkoutObject.SelectedDates.ForEach(date => description += $"<br />&nbsp;&bull;&nbsp;{date.ToLongDateTimeString()}");


            var isCompletelyFree = checkoutObject.IsFree;
            var isRegFree = participationObject.IsRegistrationFree;
            var hasPaymentPlan = paymentPlan.IsNotEmpty();
            var hasSubscriptionPlan = subscription.IsNotEmpty();
            var hasPostPayPlan = postPayPlan.IsNotEmpty();
            var splitCostEvenly = subscription?.SplitCostEvenly ?? false;
            var regFinalized = participationObject.RegistrationFinalized;
            string registrationTable = "";
            var totalCost = checkoutObject.GetTotalCostToPurchaseTodayWithFees(msgs, dc);
            var showTable = false;

            if (splitCostEvenly && regFinalized)
                totalCost = checkoutObject.GetSubscriptionCostTodayWithFees(msgs, dc);

            // show this if no subscription plan, if there's a reg fee or there's a payment plan
            if ((isCompletelyFree || !isRegFree || hasPaymentPlan || hasPostPayPlan) && (!splitCostEvenly || !regFinalized))
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
                    var startDate = submittedRecord.GetPlanStartDate(paymentPlan, participationObject, msgs, dc);

                    if (startDate.IsOnOrAfter(today))
                        todayOrLater = $"on {startDate.ToShortDateString()}";

                    additionalDetails = $"{todayOrLater} plus {paymentPlan.GetTotalCostWithConvenienceFee(billingAccount, msgs, dc).ToCurrency()} for {(paymentPlan.TotalPeriods.Value - 1)} {paymentPlan.BillingPeriodTypeId.ToString().ToLower()}(s)";
                }
                else
                {
                    if (checkoutObject.Coupon.IsNotEmpty() &&
                        checkoutObject.Coupon.ID.IsNotEmpty() && 
                        checkoutObject.Coupon.DoesApplyToRegistrationFee())
                    {
                        //for now lets not apply coupon and just get the value of total cost. (to show proper cost per item in email)
                        var amountCC = checkoutObject.GetRegistrationCostWithFeeAndCoupon(applyCoupon: false, msgs: msgs, dc: dc);                        
                        var couponValueCC = checkoutObject.CouponValue(msgs,dc);
                        
                        cost = amountCC.ToCurrency();
                        additionalDetails = "registration fee";

                        // add registration fee row
                        var registrationFeeModel = new NameValueCollection()
                                            {
                                                { "Description", description },
                                                { "Quantity",  checkoutObject.ClassQuantity.ToString() },
                                                { "Cost", cost },
                                                { "AdditionalDetails", additionalDetails }
                                            };

                        receiptRows += ProcessTemplate(
                                        EmailTemplates.Registration_Confirmation_Rows,
                                        registrationFeeModel,
                                        "Registration_Confirmation_Rows",
                                        msgs);

                        // add coupon discount row
                        var couponDiscountModel = new NameValueCollection()
                                            {
                                                { "Description", $"Coupon {checkoutObject.Coupon.Name} Applied" },
                                                { "Quantity",  "1"},
                                                { "Cost", couponValueCC.ToCurrency() },
                                                { "AdditionalDetails", $"{(checkoutObject.Coupon.GetDiscountString())} discount applied." }
                                            };

                        receiptRows += ProcessTemplate(
                                        EmailTemplates.Registration_Confirmation_Rows,
                                        couponDiscountModel,
                                        "Registration_Confirmation_Rows",
                                        msgs);
                    }
                    else
                    {
                        cost = (checkoutObject.GetTotalCostToPurchaseTodayWithConvenienceFee(msgs, dc)).ToCurrency();                        
                        additionalDetails = "registration fee";

                        // add registration fee row
                        var registrationFeeModel = new NameValueCollection()
                                            {
                                                { "Description", description },
                                                { "Quantity",  checkoutObject.ClassQuantity.ToString() },
                                                { "Cost", cost },
                                                { "AdditionalDetails", additionalDetails }
                                            };

                        receiptRows += ProcessTemplate(
                                        EmailTemplates.Registration_Confirmation_Rows,
                                        registrationFeeModel,
                                        "Registration_Confirmation_Rows",
                                        msgs);                        
                    }

                    ccFee = (checkoutObject.GetTotalCreditCardFeesToday(msgs, dc)).ToCurrency();
                    showTable = true;
                }
            }

            // if has subscription
            // and is not finalized if it is split evenly

            // add subscription row
            if (hasSubscriptionPlan && (!splitCostEvenly || regFinalized))
            {
                var startDate = submittedRecord.GetPlanStartDate(subscription, participationObject, msgs, dc);
                description = $"Subscription for {participationObject.Name} - Starting on {startDate.ToShortDateString()}";
                additionalDetails = $"every {subscription.BillingPeriodTypeId.ToString().SplitByUpperCase().ToLower()}";

                cost = checkoutObject.GetSubscriptionCostTodayWithConvenienceFee(msgs, dc).ToCurrency();

                if (isRegFree || (splitCostEvenly && regFinalized))
                    ccFee = checkoutObject.GetSubscriptionCostTodayCreditCardFee(msgs, dc).ToCurrency();
                else
                    ccFee = checkoutObject.GetTotalCreditCardFeesToday(msgs, dc).ToCurrency();

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

                if (subscription.RegFeeAsDownPayment)
                {
                    var credit = participationObject.GetCheckoutCostWithConvenienceFee() * -1;

                    totalCost += credit;

                    receiptRows += ProcessTemplate(
                                    EmailTemplates.Registration_Confirmation_Rows,
                                    new NameValueCollection()
                                    {
                                        { "Description", "One time registration fee credit" },
                                        { "Quantity",  "1"},
                                        { "Cost", credit.ToCurrency() },
                                        { "AdditionalDetails", "" }
                                    },
                                    "Registration_Confirmation_Rows",
                                    msgs);
                }

                showTable = true;
            };

            if (showTable)
                registrationTable = ProcessTemplate(
                                     EmailTemplates.Registration_Confirmation_Table,
                                     new NameValueCollection()
                                        {
                                            { "AmountDueOrPaid", amountDueOrPaid },
                                            { "TotalCost", totalCost.ToCurrency() },
                                            { "BodyRows", receiptRows },
                                            { "CreditCardFee", ccFee },
                                        },
                                     "Registration_Confirmation_Table-1.0",
                                     msgs);

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

                if (customMessage == "Thank you for registering")
                    customMessage += ". ";

                if (isParentCopy || loggedInUser.HasAccessTo(billingAccount))
                {
                    var addClosingP = false;

                    if (customMessage.Contains("<p>"))
                    {
                        customMessage += "<p>";
                        addClosingP = true;
                    }

                    if (isParentCopy)
                        customMessage += $"{masterRegistrant.FirstName} has registered";
                    else if (loggedInUser.HasAccessTo(billingAccount))
                        customMessage += $"{masterRegistrant.FirstName} has been registered by {loggedInUser.DisplayName} at {billingAccount.Name}";

                    if (showTable)
                        customMessage += " for the following:";
                    else
                        customMessage += ". ";

                    if (addClosingP)
                        customMessage += "</p>";
                }
            }

            var virtualInfo = "";
            if (participationObject.ActivityLocationType == ActivityLocationType.Virtual && 
                participationObject.VirtualLocationInfo.IsNotEmpty() &&
                mainRA.AssignmentTypeId.In(Registration.ReservedStatuses))
            {
                virtualInfo += $"<p>Join this activity virtually with the following info: <br /><br />{participationObject.VirtualLocationInfo}</a>.<br /></p>";
            }

            var physicalInfo = "";
            if (participationObject.ActivityLocationType == ActivityLocationType.Physical && 
                participationObject.Location.IsNotEmpty() && 
                mainRA.AssignmentTypeId.In(Registration.ReservedStatuses))
            {
                physicalInfo += $"<p>The location of {participationObject.Name} is at: {participationObject.Location.FormatAddress()}<br /></p>";
            }

            var splitPaymentSubscriptionInfo = "";

            if (subscription.IsNotEmpty() && subscription.SplitCostEvenly && !participationObject.RegistrationFinalized)
            {
                splitPaymentSubscriptionInfo = $"<p>Once all students are registered and registration is finalized for {participationObject.Name}, you will receive an email with the final amount to pay.</p>";
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
                { "CustomMessage", customMessage },
                { "RegistrationDetails", registrationDetails},
                { "VirtualInfo", virtualInfo },
                { "PhysicalInfo", physicalInfo },
                { "SplitPaymentSubscriptionInfo", splitPaymentSubscriptionInfo },
                { "RegistrationTable", registrationTable }
            };
            
            var subject = $"{participationObject.Name} registration for {masterRegistrant.FirstName}";
            var body = ProcessTemplate(
                EmailTemplates.Registration_Confirmation,
                model,
                "Registration_Confirmation-1.2",
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

        public static void SendAttendanceChargeConfirmation(Dictionary<Attendance, List<int>> attendanceRegistrantActivityCounts, Activity activity, decimal costPerClass, decimal couponDiscount, string customMessage, WebUser loggedInUser, bool sendBcc, SystemMessages msgs, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();

            var account = activity.GetAccount(msgs, dc);
            var isAfterSchool = account.IsAfterSchool;

            var receiptRows = "";
            decimal totalAttendanceCharge = 0;
            int totalNumberPresent = 0;

            // prepare registration fee rows
            attendanceRegistrantActivityCounts.ToList().ForEach(attendanceRegistrantActivityCount =>
            {
                var attendance = attendanceRegistrantActivityCount.Key;
                var attendanceCount = attendanceRegistrantActivityCount.Value.Count();
                var prePaidAttendanceRegistrantCount = attendance.GetAttendanceRegistrants(msgs, dc).Count(ar => ar.HasPrePayment(msgs, dc));
                var totalCharge = (attendanceCount - prePaidAttendanceRegistrantCount) * costPerClass;
                var totalDiscount = attendanceCount * couponDiscount;
                
                //.. todo: get coupon discount and prepare calculation

                var attendanceModel = new NameValueCollection() 
                {
                    { "AttendanceDate", attendance.Date.ToShortDateTimeString() },
                    { "NumberPresent",  attendanceCount.ToString() },
                    { "NumberPrePaid", prePaidAttendanceRegistrantCount.ToString() },
                    { "TotalCharge", totalCharge.ToCurrency() },
                    { "CouponDiscount", totalDiscount.ToCurrency() }
                };

                receiptRows += ProcessTemplate(
                        EmailTemplates.Attendance_Charge_Confirmation_Rows,
                        attendanceModel,
                        "Attendance_Charge_Confirmation_Rows-1.2",
                        msgs);

                totalAttendanceCharge += (totalCharge - totalDiscount);
                totalNumberPresent += attendanceCount;
            });

            var attendanceChargeTableModel = new NameValueCollection()
            {
                { "TotalNumberPresent" , totalNumberPresent.ToString()  },
                { "TotalAttendanceCharge", totalAttendanceCharge.ToCurrency() },
                { "BodyRows", receiptRows }
            };

            var attendanceChargeTable = ProcessTemplate(
                EmailTemplates.Attendance_Charge_Confirmation_Table,
                attendanceChargeTableModel,
                "Attendance_Charge_Confirmation_Table-1.2",
                msgs);

            var model = new NameValueCollection()
            {
                { "FirstName", loggedInUser.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "LoginLink", Utilities.LoginUrl() },
                { "AccountEmail", account.Email },
                { "RegistrationTable", attendanceChargeTable },
                { "CustomMessage", customMessage },
                { "RegistrationDetails", "Login to your account to view more details about this payment."},
                { "ActivityName", activity.Name.ToProperCase() },
                { "SupportEmail", Email.AppSupportEmailAddress }
            };

            var subject = $"Attendance Charged for {activity.Name}";
            var body = ProcessTemplate(
                EmailTemplates.Attendance_Charge_Confirmation,
                model,
                "Attendance_Charge_Confirmation-1.1",
                msgs);

            // SEND EMAILS
            var mailMsgs = new SystemMessages();

            // Send email to activity account
            mailMsgs.AddRange(SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(isAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)));

            msgs.AddRange(mailMsgs);
        }

        public static void SendPaymentConfirmation(CheckoutObject checkoutObject, string customMessage, WebUser loggedInUser, bool sendBcc, SystemMessages msgs, bool isAttendanceCharge = false, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();

            var isAfterSchool = checkoutObject.PayingAccount.IsAfterSchool;
            var purchaserAccount = checkoutObject.PayingAccount;
            var purchaser = checkoutObject.PayingAccount.GetAccountOwner(msgs, dc);
            var billingAccount = checkoutObject.BillingAccount;
            var billingAccountOwner = billingAccount.GetAccountOwner(msgs, dc);
            var masterRegistrant = checkoutObject.MasterRegistrant;
            var parentOrGuardian = masterRegistrant?.GetParentOrGuardian(msgs, dc);
            var canAccountAcceptPayments = billingAccount.CanProcessPayments(msgs, dc);
            var registration = checkoutObject.Registration;
            var totalCostWithFees = checkoutObject.GetStripeAmount(msgs, dc);
            var totalCost = totalCostWithFees - checkoutObject.PaymentTypeFeeOverrideAmount.Value;
            var totalCreditCardFees = checkoutObject.PaymentTypeFeeOverrideAmount.Value;
            var quantity = checkoutObject.Quantity.HasValue ? checkoutObject.Quantity.Value.ToString() : "1";
            var description = $"Payment to {billingAccount.Name}";

            if (checkoutObject.Quantity.HasValue && isAttendanceCharge)
            {
                description = $"Payment to {billingAccount.Name} ({(totalCost / checkoutObject.Quantity.Value).ToCurrency()} per {checkoutObject.PostPayPlan.BillingPeriodTypeId.ToString().ToLower()})";
            }

            var receiptRows = "";

            if (checkoutObject.Coupon.IsNotEmpty() && checkoutObject.Coupon.ID.IsNotEmpty() && checkoutObject.Coupon.DoesApplyToRegistrationFee())
            {
                var cost = checkoutObject.ParticipationObject.GetPostPayCostWithConvenienceFee();               
                var couponValue = checkoutObject.Coupon.GetValue(checkoutObject.ParticipationObject.GetPostPayCost());                

                // add registration fee row
                var paymentModel = new NameValueCollection()
                            {
                                { "Description", description },
                                { "Quantity", quantity },
                                { "Cost", cost.ToCurrency() },
                                { "AdditionalDetails", "" }
                            };

                receiptRows += ProcessTemplate(
                        EmailTemplates.Registration_Confirmation_Rows,
                        paymentModel,
                        "Registration_Confirmation_Rows",
                        msgs);

                // add coupon discount row
                var couponDiscountModel = new NameValueCollection()
                                            {
                                                { "Description", $"Coupon {checkoutObject.Coupon.Name} Applied" },
                                                { "Quantity",  "1"},
                                                { "Cost", couponValue.ToCurrency() },
                                                { "AdditionalDetails", $"{(checkoutObject.Coupon.GetDiscountString())} discount applied." }
                                            };

                receiptRows += ProcessTemplate(
                                EmailTemplates.Registration_Confirmation_Rows,
                                couponDiscountModel,
                                "Registration_Confirmation_Rows",
                                msgs);
            }
            else
            {
                // add registration fee row
                var paymentModel = new NameValueCollection()
                            {
                                { "Description", description },
                                { "Quantity", quantity },
                                { "Cost", totalCost.ToCurrency() },
                                { "AdditionalDetails", "" }
                            };

                receiptRows += ProcessTemplate(
                        EmailTemplates.Registration_Confirmation_Rows,
                        paymentModel,
                        "Registration_Confirmation_Rows",
                        msgs);
            }

            var registrationTable = ProcessTemplate(
                                     EmailTemplates.Registration_Confirmation_Table,
                                     new NameValueCollection()
                                        {
                                            { "AmountDueOrPaid", "Amount Paid" },
                                            { "TotalCost", totalCostWithFees.ToCurrency() },
                                            { "BodyRows", receiptRows },
                                            { "CreditCardFee", totalCreditCardFees.ToCurrency() },
                                        },
                                     "Registration_Confirmation_Table-1.0",
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
                { "RegistrationTable", registrationTable },
                { "CustomMessage", customMessage },
                { "CreditCardFee", checkoutObject.PaymentTypeFeeOverrideAmount.ToCurrency() },
                { "RegistrationDetails", "Login to your account to view more details about this payment."},
                { "VirtualInfo", "" },
                { "SplitPaymentSubscriptionInfo", "" }
            };

            var subject = $"Payment receipt for {billingAccount.Name}";
            var body = ProcessTemplate(
                EmailTemplates.Registration_Confirmation,
                model,
                "Registration_Confirmation-1.0",
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
                subject: $"BCC: {subject}",
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

        public static SystemMessages SendAccountConfirmationEmail(WebUser user, string redirectUrl, SystemMessages msgs)
        {
            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "ProductName", Utilities.AppName(true) },
                { "ConfirmLink", Utilities.CompleteSignUpUrl(user.Uid, redirectUrl) }
            };

            var body = ProcessTemplate(
                        EmailTemplates.Account_Confirm,
                        model,
                        "account_confirm-1.0.1",
                        msgs);

            var subject = $"Finalize your account on {Utilities.AppName(true)}";

            var mailMsgs = SendGridHelper.SendMail(
                            fromEmail: Email.SendGridNoReplyEmailAddress(true),
                            toEmail: new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                            subject: subject,
                            body: body,
                            emailTemplateId: SendGridHelper.GetGenericTemplateId(true));

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

        public static SystemMessages SendEmailVerification(WebUser user, string redirectUrl, bool isAfterSchool)
        {
            var msgs = new SystemMessages();            

            var model = new NameValueCollection()
            {
                { "FirstName", user.FirstName },
                { "ProductName", Utilities.AppName(isAfterSchool) },
                { "VerificationLink", $"{Email.BaseUrl()}signup/verify/{user.Uid.ToString()}?redirectUrl={HttpUtility.UrlEncode(redirectUrl)}" },
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

        public static SystemMessages SendWelcomePurchase(string userFirstName, string userEmail, bool IsAccountAfterSchool)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", userFirstName },
                { "ProductName", Utilities.AppName(IsAccountAfterSchool) },
                { "SupportEmail", Email.AppSupportEmailAddress }
            };

            var body = ProcessTemplate(
                EmailTemplates.Welcome_Purchase,
                model,
                "welcomepurchase",
                msgs
                );

            var subject = $"Welcome to the {Utilities.AppName(IsAccountAfterSchool)} Family!";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(IsAccountAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(userEmail),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(IsAccountAfterSchool)
                );

            // send copy to afterschool hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridNoReplyEmailAddress(IsAccountAfterSchool),
                toEmail: Email.SendGridSupportEmailAddress,
                subject: $"CC: {Utilities.AppName(IsAccountAfterSchool)} SIGNUP - {subject}",
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(IsAccountAfterSchool)
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

        public static SystemMessages SendAccessRequestToHost(Account visitorAccount, Account hostAccount, WebUser loggedInUser)
        {
            var msgs = new SystemMessages();
            var hostAccountOwner = hostAccount.GetAccountOwner(msgs);

            // TODO: disucss with darye
            // this call takes too much time to load account owner, we can remvoe this call and use logged in user
            // var visitorAccountOwner = visitorAccount.GetAccountOwner(msgs);

            var model = new NameValueCollection()
            {
                { "HostFirstName", hostAccountOwner.FirstName },
                { "HostAccountName", hostAccount.Name},
                { "VisitorAccountName", visitorAccount.Name },
                { "VisitorUserDisplayName", loggedInUser.DisplayName },
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
