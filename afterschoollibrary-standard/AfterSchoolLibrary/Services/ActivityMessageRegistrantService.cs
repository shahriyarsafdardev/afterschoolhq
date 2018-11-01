using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

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
            var msgs = new SystemMessages();

            activityMessageRegistrant.GetChildAndParentRegistrants(msgs, dc)
                .ForEach(registrant => {
                    msgs.AddRange(SendGridHelper.SendMail(
                        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                            Email.AppEmailAddress,
                            $"{fromName} via {Utilities.AppName(account.IsAfterSchool)}"),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(registrant.Email, registrant.DisplayName),
                        subject: subject,
                        body: $"{body}{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activity_message"))}",
                        replyTo: new SendGrid.Helpers.Mail.EmailAddress(replyToEmail, replyToName),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    ));
                }
            );

            return msgs;
        }

    }
}
