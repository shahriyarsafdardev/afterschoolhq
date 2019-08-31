using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twilio.Rest.Api.V2010.Account;

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

            am.SendActivityMessage(account, loggedInUser, activity.Name, activity.ContactName, activity.ContactEmail, activity.ContactName, msgs, dc);

            if (msgs.HasNoErrors() && am.SendToStaff)
                am.SendMessageToTeamMembers(activity, account, loggedInUser, msgs, dc);

        }

        private static void SendMessageToTeamMembers(this ActivityMessage activityMessage, Activity activity, Account account, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var members = activity.GetTeamMembers(msgs, dc).DistinctBy(member => member.WebUserId).ToList();

            activityMessage.SendMessageToTeamMembers(
                members,
                activity.Name,
                activity.ContactEmail.IfBlank(account.Email),
                activity.ContactName.IfBlank(account.Name),
                account,
                loggedInUser,
                msgs,
                dc);
        }

        private static void SendMessageToTeamMembers(this ActivityMessage activityMessage, ActivityGroup group, Account account, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var members = new List<ActivityWebUser>();
            var activities = group.GetActivities(msgs, dc);

            activities.ForEach(activity => {
                members.AddRange(activity.GetTeamMembers(msgs, dc).DistinctBy(member => member.WebUserId));
            });

            members = members.DistinctBy(member => member.WebUserId).ToList();

            activityMessage.SendMessageToTeamMembers(
                members,
                group.Name,
                group.ContactEmail.IfBlank(account.Email),
                group.ContactName.IfBlank(account.Name),
                account,
                loggedInUser,
                msgs,
                dc);
        }

        internal static void SendMessageToTeamMembers(this ActivityMessage activityMessage, List<ActivityWebUser> members, string activityName, string contactEmail, string contactName, Account account, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            members.ForEach(member => {

                var body = SearchAndReplaceNameInText(
                    activityMessage.Body,
                    member.WebUserFirstName.IfBlank(""),
                    member.WebUserLastName.IfBlank(""),
                    member.WebUserDisplayName.IfBlank(""));

                ActivityMessageService.SendEmailMessage(
                    account,
                    activityName,
                    loggedInUser.DisplayName,
                    member.WebUserEmail,
                    member.WebUserDisplayName,
                    contactEmail,
                    contactName,
                    activityMessage.Subject,
                    body,
                    "activity_message",
                    msgs);

                ActivityMessageService.SendTextMessage(
                    member.WebUserPhone,
                    loggedInUser.DisplayName,
                    activityMessage.Subject,
                    activityName,
                    activityMessage.Uid.ToString(),
                    null,
                    msgs);
            });
        }

        internal static MessageResource SendTextMessage(string phone, string senderName, string subject, string activityName, string messageUid, string registrantUid, SystemMessages msgs)
        {
            var aOrAn = subject[0].In("aeiouAEIOU") ? "an" : "a";

            return TwilioHelper.SendSMS(
                phone,
                $"{senderName} has sent {aOrAn} {subject} for {activityName}\nPlease review your message: {Helpers.Utilities.BaseUrl()}/msg/{messageUid}",
                messageUid,
                registrantUid,
                msgs);
        }

        internal static void SendEmailMessage(Account account, string activityName, string fromName, string toEmail, string toName, string replyToEmail, string replyToName, string subject, string body, string utmCampaign, SystemMessages msgs)
        {
            var aOrAn = subject[0].In("aeiouAEIOU") ? "an" : "a";
            var header = $"<h2 style=\"margin-left: 10px;\">{subject} for {activityName}</h2>";
            var sentBy = $"<p>&nbsp;</p><p>Sent by <strong>{replyToName}</strong> at <strong>{account.ConvertToAccountTimeZone(DateTime.UtcNow).ToLongDateTimeString()}</strong></p>";

            body = $"<p>{body.Replace("\n", "<br />")}</p>";

            msgs.AddRange(SendGridHelper.SendMail(
                fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                    Helpers.Email.AppNoReplyEmailAddress,
                    $"{fromName} via {Helpers.Utilities.AppName()}"),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(toEmail, toName),
                subject: $"{replyToName} has sent {aOrAn} {subject} regarding {activityName}",
                body: $"{header}{body}{sentBy}{Helpers.Email.GetAppSignature(isAfterSchool: true, utm_campaign: utmCampaign)}",
                replyTo: new SendGrid.Helpers.Mail.EmailAddress(replyToEmail, replyToName),
                emailTemplateId: SendGridHelper.GetGenericTemplateId()
            ));
        }

        private static void SendForGroup(ActivityMessage am, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var group = am.GetActivityGroup(msgs, dc);
            var account = group.GetAccount(msgs, dc);
            var loggedInUser = WebUser.Load(loggedInUserId, msgs, dc);

            am.SendActivityMessage(account, loggedInUser, group.Name, account.Name, account.Email, account.Name, msgs, dc);

            if (msgs.HasNoErrors() && am.SendToStaff)
                am.SendMessageToTeamMembers(group, account, loggedInUser, msgs, dc);
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

        private static void SendActivityMessage(this ActivityMessage activityMessage, Account account, WebUser loggedInUser, string activityName, string fromName, string replyToEmail, string replyToName, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var amrs = activityMessage.GetActivityMessageRegistrants(msgs, dc);

            amrs.ForEach(activityMessageRegistrant => {
                activityMessageRegistrant.SendMessage(
                    activityMessage,
                    account, 
                    loggedInUser,
                    activityName,
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

        public static WebUser GetCreatedBy(this ActivityMessage activityMessage, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activityMessage.CreatedBy.HasValue)
                return WebUser.Load(activityMessage.CreatedBy.Value, msgs, dc);

            return null;
        }

        public static ParticipationObject GetParticipationObject(this ActivityMessage activityMessage, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activityMessage.ActivityId.HasValue)
                return new ParticipationObject(activityMessage.ActivityId.Value, ParticipationObjectType.Activity, msgs, dc);

            if (activityMessage.ActivityGroupId.HasValue)
                return new ParticipationObject(activityMessage.ActivityGroupId.Value, ParticipationObjectType.Activity, msgs, dc);

            return null;
        }
    }
}
