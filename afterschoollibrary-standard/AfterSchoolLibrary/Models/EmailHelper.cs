using System;
using BasicLibrary;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;

namespace AfterSchoolLibrary
{
    public class EmailHelper
    {
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

        public static SystemMessages SendAccessRequestToHost(Account visitorAccount, Account Hostaccount)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", visitorAccount.GetAccountOwner().DisplayName },
                { "ProductName", visitorAccount.Name}
            };

            var body = $"The after school provider {visitorAccount.Name} is  interested in becoming an activity provider for {Hostaccount.Name}," +
                $"hop on ASHQ to accept their request! ";

            var subject = $" {visitorAccount.Name} would like to request access to {Hostaccount.Name}'s after school";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(Hostaccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(Hostaccount.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: Email.SendGridAppAdminEmailAddress,
                subject: $" {visitorAccount.Name} would like to request access to {Hostaccount.Name}'s after school",
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(Hostaccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }


        public static SystemMessages SendApprovalNotificationToVisitor(Account visitorAccount, Account Hostaccount)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", visitorAccount.GetAccountOwner().DisplayName },
                { "ProductName", visitorAccount.Name}
            };

            var body = $"You have been approved by  {Hostaccount.Name} to become an activity provider," +
                $"hop on ASHQ to set up your next after school activity!";

            var subject = $"Approval to access {Hostaccount.Name}'s after school";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(Hostaccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(visitorAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: Email.SendGridAppAdminEmailAddress,
                subject: $"Approval to access {Hostaccount.Name}'s after school",
            body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(Hostaccount.IsAfterSchool)
                );

            msgs.AddRange(mailMsgs);
            return msgs;
        }


        public static SystemMessages SendDenialNotificationToVisitor(Account visitorAccount, Account Hostaccount)
        {
            var msgs = new SystemMessages();

            var model = new NameValueCollection()
            {
                { "FirstName", visitorAccount.GetAccountOwner().DisplayName },
                { "ProductName", visitorAccount.Name}
            };

            var body = $"You have been denied by  {Hostaccount.Name} to become an activity provider.";

            var subject = $"Approval to access {Hostaccount.Name}'s after school was denied";

            var mailMsgs = SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(Hostaccount.IsAfterSchool),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(visitorAccount.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(visitorAccount.IsAfterSchool)
                );

            // send copy to enroll hq team
            SendGridHelper.SendMail(
                fromEmail: Email.SendGridAppEmailAddress(visitorAccount.IsAfterSchool),
                toEmail: Email.SendGridAppAdminEmailAddress,
                subject: $"Approval to access {Hostaccount.Name}'s after school was denied",
            body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(Hostaccount.IsAfterSchool)
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
