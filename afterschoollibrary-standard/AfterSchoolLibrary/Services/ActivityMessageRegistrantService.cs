using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityMessageRegistrantService
    {
        public static List<Registrant> GetChildAndParentRegistrants(this ActivityMessageRegistrant activityMessageRegistrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrants = new List<Registrant>();
            var childRegistrant = activityMessageRegistrant.GetRegistrant(msgs, dc);
            var parentOrGuardian = childRegistrant.GetMasterRegistrant(msgs, dc)?.GetParentOrGuardian(msgs, dc);

            if (parentOrGuardian.IsNotEmpty())
                registrants.Add(parentOrGuardian);

            if (childRegistrant.Email.IsNotEmpty() && !childRegistrant.Email.IsSameAs(parentOrGuardian.Email))
                registrants.Add(childRegistrant);

            return registrants;
        }

        internal static SystemMessages SendEmail(this ActivityMessageRegistrant activityMessageRegistrant, Account account, string fromName, string replyToEmail, string replyToName, string subject, string body, DatabaseConnection dc = null)
        {
            var sendResultMessages = new SystemMessages();

            activityMessageRegistrant.GetChildAndParentRegistrants(sendResultMessages, dc)
                .ForEach(registrant =>
                {
                    sendResultMessages.AddRange(SendGridHelper.SendMail(
                        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                            Helpers.Email.AppEmailAddress,
                            $"{fromName} via {Helpers.Utilities.AppName(account.IsAfterSchool)}"),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(registrant.Email, registrant.DisplayName),
                        subject: subject,
                        body: $"{body}{(Helpers.Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activity_message"))}",
                        replyTo: new SendGrid.Helpers.Mail.EmailAddress(replyToEmail, replyToName),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    ));
                }
            );            

            return activityMessageRegistrant.UpdateSendResults(sendResultMessages, dc);
        }

        private static SystemMessages UpdateSendResults(this ActivityMessageRegistrant activityMessageRegistrant, SystemMessages sendResultMessages, DatabaseConnection dc = null)
        {
            if (sendResultMessages.HasNoErrors())
                activityMessageRegistrant.DateSent = DateTime.UtcNow;
            else
                activityMessageRegistrant.SendErrors = sendResultMessages.RenderMessageList(MessageOutputType.Text).MaxChars(1000);

            return activityMessageRegistrant.Save(dc);
        }

    }
}
