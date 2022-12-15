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
        public static List<Registrant> GetChildAndOrParentRegistrants(this ActivityMessageRegistrant activityMessageRegistrant, bool parentsOnly, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrants = new List<Registrant>();
            var childRegistrant = activityMessageRegistrant.GetRegistrant(msgs, dc);
            var parentOrGuardian = childRegistrant.GetParentOrGuardian(msgs, dc);

            if (parentOrGuardian.IsNotEmpty())
                registrants.Add(parentOrGuardian);

            if (parentsOnly)
                return registrants;

            if (childRegistrant.Email.IsNotEmpty() && (parentOrGuardian.IsEmpty() || !childRegistrant.Email.IsSameAs(parentOrGuardian.Email)))
                registrants.Add(childRegistrant);

            return registrants;
        }        

        internal static void SendMessage(this ActivityMessageRegistrant activityMessageRegistrant, ActivityMessage message, Account account, WebUser loggedInUser, string activityName, string fromName, string replyToEmail, string replyToName, string subject, string body, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var allResults = new SystemMessages();

            activityMessageRegistrant.GetChildAndOrParentRegistrants(message.ParentsOnly, allResults, dc)
                .ForEach(registrant =>
                {
                    registrant.SendEmail(account, activityName, fromName, replyToEmail, replyToName, subject, body, "activity_message", msgs);
                    registrant.SendText(account, loggedInUser, message, activityName, subject, body, replyToName, msgs);
                }
            );            

            activityMessageRegistrant.UpdateSendResults(allResults, msgs, dc);

            msgs.AddRange(allResults);
        }

        private static void UpdateSendResults(this ActivityMessageRegistrant activityMessageRegistrant, SystemMessages sendResultMessages, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (sendResultMessages.HasNoErrors())
            {
                activityMessageRegistrant.DateSent = DateTime.UtcNow;
                activityMessageRegistrant.SentEmail = true;
            }
            else
            {
                activityMessageRegistrant.SendErrors = sendResultMessages.RenderMessageList(MessageOutputType.Text).MaxChars(1000);
            }

            msgs.AddRange(activityMessageRegistrant.Save(dc));
        }

    }
}
