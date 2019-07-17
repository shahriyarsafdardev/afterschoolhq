using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
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
        public static void UpdateDateSent(this ActivityMessage activityMessage, SystemMessages msgs, DatabaseConnection dc = null)
        {
            activityMessage.DateSent = DateTime.UtcNow;
            msgs.AddRange(activityMessage.Save(dc));
        }

        private static void SendForActivity(ActivityMessage am, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var activity = am.GetActivity(msgs, dc);
            var account = activity.GetAccount(msgs, dc);
            var loggedInUser = WebUser.Load(loggedInUserId, msgs, dc);

            am.SendActivityMessage(account, loggedInUser, activity.ContactName, activity.ContactEmail, activity.ContactName, msgs, dc);

            if (msgs.HasNoErrors())
                msgs.AddRange(am.SendMessageToTeamMembers(activity, account, dc));

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
                    fromEmail: Helpers.Email.SendGridNoReplyEmailAddress(account.IsAfterSchool),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(member.WebUserEmail),
                    subject: activityMessage.Subject,
                    body: $"{body}{(Helpers.Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activity_message"))}",
                    replyTo: new SendGrid.Helpers.Mail.EmailAddress(
                        activity.ContactEmail,
                        activity.ContactName),
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                ));

                TwilioHelper.SendSMS(
                    member.WebUserPhone,
                    $"{activityMessage.Subject}\n{body}",
                    msgs
                );
            });

            return msgs;
        }

        private static void SendForGroup(ActivityMessage am, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var group = am.GetActivityGroup(dc: dc);
            var account = group.GetAccount(dc: dc);

            am.SendActivityMessage(account, account.Name, account.Email, account.Name, msgs, dc: dc);

            if (msgs.HasNoErrors())
            {
                // send to account admin
                SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                        Helpers.Email.AppNoReplyEmailAddress,
                        $"CC: {account.Name} via {Helpers.Utilities.AppName(account.IsAfterSchool)}"),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                    subject: am.Subject,
                    body: $"{am.Body}{(Helpers.Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activitygroup_message"))}",
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );
            }
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

        private static void SendActivityMessage(this ActivityMessage activityMessage, Account account, WebUser loggedInUser, string fromName, string replyToEmail, string replyToName, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var amrs = activityMessage.GetActivityMessageRegistrants(msgs, dc);

            amrs.ForEach(activityMessageRegistrant => {
                activityMessageRegistrant.SendMessage(
                    account, 
                    loggedInUser,
                    fromName, 
                    replyToEmail, 
                    replyToName, 
                    activityMessage.Subject,
                    activityMessage.SearchAndReplaceNameInBodyText(activityMessageRegistrant), 
                    msgs,
                    dc);
            });

            activityMessage.UpdateDateSent(msgs, dc);

            if (msgs.HasErrors())
                Helpers.Utilities.SystemErrorLog(msgs);
        }

        public static void Send(int id, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var am = ActivityMessage.Load(id, msgs, dc);

            if (am.ActivityId.HasValue)
                ActivityMessageService.SendForActivity(am, loggedInUserId, msgs, dc);
            else if (am.ActivityGroupId.HasValue)
                ActivityMessageService.SendForGroup(am, loggedInUserId, msgs, dc);
            else
                msgs.Add("Error sending Messages", MessageType.Error);
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

            msgs.AddRange(amr.Save(dc));

            return amr;
        }
    }
}
