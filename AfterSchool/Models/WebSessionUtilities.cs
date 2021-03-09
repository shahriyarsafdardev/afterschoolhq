using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;

namespace AfterSchool.Models
{
    public static class WebSessionUtilities
    {
        public static void InitializeWebSession(HttpRequestBase request)
        {
            var session = new WebSession();
            if (SessionVariables.WebSession.IsEmpty())
            {                
                session.LastModifiedDate = DateTime.UtcNow;
                session.LastModifiedBy = session.CreatedBy = 0;
                session.UrlReferrer = request.UrlReferrer.IsNotEmpty() ? request.UrlReferrer.ToString() : null;
                session.IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"]; //Request.UserHostAddress;
                session.UserAgent = request.UserAgent;
                //.. need to retrieve this both from user location
                session.IPAddressTimeZone = null;
                session.UserLocation = null;
                session.PreviousId = null; //not sure what data will be here
                session.WebUserId = SessionVariables.CurrentUser.ID;
                session.AccountId = SessionVariables.CurrentAccount.ID;
            }
            else
            {
                session = WebSession.Load(SessionVariables.WebSession.ID.Value);
                session.LastModifiedDate = DateTime.UtcNow;

                if (SessionVariables.WebSession.WebUserId.IsEmpty() && SessionVariables.CurrentUser.IsNotEmpty())
                    session.WebUserId = SessionVariables.CurrentUser.ID;

                if (SessionVariables.WebSession.AccountId.IsEmpty() && SessionVariables.CurrentAccount.IsNotEmpty())
                    session.AccountId = SessionVariables.CurrentAccount.ID;                
            }

            session.Save();
            SessionVariables.WebSession = session;
        }

        public static void LogActivity(WebUser actor, int? activityId, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObject = ObjectType.NotSet)
        {
            //.. actor should not be null 

            var action = new WebSessionAction();
            action.SessionId = SessionVariables.WebSession.ID;
            action.ActorId = actor.ID;
            action.ActivityId = activityId.HasValue ? activityId.Value : (int?)null;
            action.AccountId = SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null;
            action.ActionTypeId = actionType;
            action.ObjectTypeId = objectType;
            action.ObjectId = null; //not sure what data will be here
            action.IndirectObjectId = ObjectType.NotSet;//not sure what data will be here

            var actionString = $"{actor.FirstName} {actionType}";
            action.ActionStringFormat = actionString;

            if (objectType != ObjectType.NotSet)
                actionString += $"{objectType}";

            if (indirectObject != ObjectType.NotSet)
                actionString += $"{indirectObject}";

            action.ActionString = actionString;
            action.Save();
        }
    }
}