using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityMessageService
    {
        public static SystemMessages UpdateDateSent(this ActivityMessage activityMessage, DatabaseConnection dc = null)
        {
            activityMessage.DateSent = DateTime.UtcNow;
            return activityMessage.Save(dc);
        }

        private static SystemMessages SendForActivity(ActivityMessage am, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activity = am.GetActivity(dc: dc);
            var account = activity.GetAccount(dc: dc);

            msgs.AddRange(am.SendActivityMessage(account, activity.ContactName, activity.ContactEmail, activity.ContactName, dc: dc));

            if (msgs.HasNoErrors())
                msgs.AddRange(am.SendMessageToTeamMembers(activity, account, dc));

            return msgs;
        }

        private static SystemMessages SendMessageToTeamMembers(this ActivityMessage activityMessage, Activity activity, Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var members = activity.GetTeamMembers(msgs, dc).DistinctBy(member => member.WebUserId).ToList();

            members.ForEach(member => {
                var body = SearchAndReplaceNameInText(
                    activityMessage.Body,
                    member.WebUserFirstName.IfBlank(""),
                    member.WebUserLastName.IfBlank(""),
                    member.WebUserDisplayName.IfBlank(""));

                msgs.AddRange(SendGridHelper.SendMail(
                    fromEmail: Email.SendGridAppEmailAddress(account.IsAfterSchool),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(member.WebUserEmail),
                    subject: activityMessage.Subject,
                    body: $"{body}{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activity_message"))}",
                    replyTo: new SendGrid.Helpers.Mail.EmailAddress(
                        activity.ContactEmail,
                        activity.ContactName),
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                ));
            });

            return msgs;
        }

        private static SystemMessages SendForGroup(ActivityMessage am, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var group = am.GetActivityGroup(dc: dc);
            var account = group.GetAccount(dc: dc);

            msgs.AddRange(am.SendActivityMessage(account, account.Name, account.Email, account.Name, dc: dc));

            if (msgs.HasNoErrors())
            {
                // send to account admin
                SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                        Email.AppEmailAddress,
                        $"CC: {account.Name} via {Utilities.AppName(account.IsAfterSchool)}"),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                    subject: am.Subject,
                    body: $"{am.Body}{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activitygroup_message"))}",
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );
            }

            return msgs;
        }

        private static string SearchAndReplaceNameInBodyText(this ActivityMessage activityMessage, ActivityMessageRegistrant amr)
        {
            var body = activityMessage.Body;
            //a person
            if (amr.RegistrantFirstName.IsNotBlank() || amr.RegistrantLastName.IsNotBlank())
                body = SearchAndReplaceNameInText(body, amr.RegistrantFirstName, amr.RegistrantLastName, amr.RegistrantDisplayName);
            //an organization
            else if (amr.RegistrantCompany.IsNotBlank())
                body = SearchAndReplaceNameInText(body, amr.RegistrantCompany, "", amr.RegistrantCompany);

            return body;
        }

        private static string SearchAndReplaceNameInText(string text, string firstName, string lastName, string name)
        {
            text = text.Replace("[firstname]", firstName);
            text = text.Replace("[lastname]", lastName);
            text = text.Replace("[name]", name);

            return text;
        }

        private static SystemMessages SendActivityMessage(this ActivityMessage activityMessage, Account account, string fromName, string replyToEmail, string replyToName, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var amrs = activityMessage.GetActivityMessageRegistrants(msgs, dc);

            amrs.ForEach(activityMessageRegistrant => {
                var body = activityMessage.SearchAndReplaceNameInBodyText(activityMessageRegistrant);
                var sendMsgs = activityMessageRegistrant.SendEmail(account, fromName, replyToEmail, replyToName, activityMessage.Subject, body, dc);

                if (sendMsgs.HasNoErrors())
                    activityMessageRegistrant.DateSent = DateTime.UtcNow;
                else
                    activityMessageRegistrant.SendErrors = sendMsgs.RenderMessageList(MessageOutputType.Text).MaxChars(1000);

                msgs.AddRange(activityMessageRegistrant.Save(dc: dc));
            });

            if (msgs.HasNoErrors())
                msgs.AddRange(activityMessage.UpdateDateSent(dc));
            else
                Utilities.SystemErrorLog(msgs);

            return msgs;
        }

        public static SystemMessages Send(int id, DatabaseConnection dc = null)
        {
            var am = ActivityMessage.Load(id, dc: dc);

            if (am.ActivityId.HasValue)
                return ActivityMessageService.SendForActivity(am, dc: dc);
            else if (am.ActivityGroupId.HasValue)
                return ActivityMessageService.SendForGroup(am, dc: dc);

            return new SystemMessages() { new SystemMessage() { Text = "Error", MessageType = MessageType.Error } };
        }

        public static ActivityMessageRegistrant AddRegistrant(this ActivityMessage activityMessage, int registrantId, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var amr = new ActivityMessageRegistrant
            {
                RegistrantId = registrantId,
                ActivityMessageId = activityMessage.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId
            };

            msgs.AddRange(amr.Save(dc: dc));

            return amr;
        }
    }
}
