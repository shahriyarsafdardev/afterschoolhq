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

        internal static SystemMessages SendMessage(this ActivityMessageRegistrant activityMessageRegistrant, Account account, string fromName, string replyToEmail, string replyToName, string subject, string body, DatabaseConnection dc = null)
        {
            var allResults = new SystemMessages();

            activityMessageRegistrant.GetChildAndParentRegistrants(allResults, dc)
                .ForEach(registrant =>
                {
                    allResults.AddRange(registrant.SendEmail(account.IsAfterSchool, fromName, replyToEmail, replyToName, subject, body, "activity_message"));
                    allResults.AddRange(registrant.SendText(subject, body));
                }
            );            

            return activityMessageRegistrant.UpdateSendResults(allResults, dc);
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
