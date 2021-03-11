using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary.Services;

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
                session.WebUserId = SessionVariables.CurrentUser?.ID;
                session.AccountId = SessionVariables.CurrentAccount?.ID;
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

        public static void LogActivity(WebUser actor, WebSessionActionType actionType = WebSessionActionType.NotSet)
        {
            SaveLog(actorName: (actor.IsNotEmpty() ? actor.FirstName : "Someone"),
                    actionStringFormat: "{actor} {action}",
                    actionString: $"{actor.FirstName} {actionType}",
                    actorId: actor.IsNotEmpty() ? actor.ID : null,
                    accountId: SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null,
                    actionType: actionType,
                    objectType: ObjectType.NotSet,
                    objectId: null,
                    indirectObjectId: null);
        }

        public static void LogActivity(WebUser actor, int? Id = (int?)null, string text = "", WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet)
        {
            string actionString = null, actionStringFormat = null, actorName = (actor.IsNotEmpty() ? actor.FirstName : "Someone");

            switch (objectType)
            {
                case ObjectType.Activity:
                    actionStringFormat = "{actor} {action} Activity {objecttype}";
                    actionString = $"{actorName} {actionType} Activity {text}";
                    break;

                case ObjectType.Account:
                    actionStringFormat = "{actor} {action} Account {objecttype}";
                    actionString = $"{actorName} {actionType} Account {text}";
                    break;

                case ObjectType.Registrant:
                    actionStringFormat = "{actor} {action} Registrant {objecttype}";
                    actionString = $"{actorName} {actionType} Registrant {text}";
                    break;

                case ObjectType.DirectoryAccount:
                    actionStringFormat = "{actor} {action} Directory Account {objecttype}";
                    actionString = $"{actorName} {actionType} Directory Account {text}";
                    break;

                default:
                    actionStringFormat = "{actor} {action}";
                    actionString = $"{actorName} {actionType}";
                    break;
            }

            SaveLog(actorName: actorName,
                    actionStringFormat: actionStringFormat,
                    actionString: actionString,
                    actorId: actor.IsNotEmpty() ? actor.ID : null,
                    accountId: SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null,
                    actionType: actionType,
                    objectType: objectType,
                    objectId: Id,
                    indirectObjectId: null);
        }

        public static void LogActivity(WebUser actor, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet,
            int? objectId = (int?)null, string objectText = "",
            int? indirectObjectId = (int?)null, string indirectObjectText = "")
        {
            //..actor	action		    Object		            ObjectType	        IndirectObjectId
            //..Bharat  Navigated to    FIRST SAMUEL CHURCH(W)  DirectoryAccount   ISYP

            string actionString = null, actionStringFormat = null, actorName = (actor.IsNotEmpty() ? actor.FirstName : "Someone");

            switch (objectType)
            {
                case ObjectType.DirectoryAccount:
                    actionStringFormat = "{actor} {action} To {primaryObject} FROM {objectType} {indirectObject}";
                    actionString = $"{actorName} {actionType} TO {objectText} FROM {objectType} {indirectObjectText}";
                    break;

                default:
                    actionStringFormat = "{actor} {action}";
                    actionString = $"{actorName} {actionType}";
                    break;
            }

            SaveLog(actorName: (actor.IsNotEmpty() ? actor.FirstName : "Someone"),
                actionStringFormat: actionStringFormat,
                actionString: actionString,
                actorId: actor.IsNotEmpty() ? actor.ID : null,
                accountId: SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null,
                actionType: actionType,
                objectType: objectType,
                objectId: objectId,
                indirectObjectId: indirectObjectId);
        }

        public static void SaveLog(string actorName, string actionStringFormat, string actionString, int? actorId = (int?)null, int? accountId = (int?)null,
            WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet,
            int? objectId = (int?)null, int? indirectObjectId = (int?)null)
        {
            var action = new WebSessionAction();
            action.SessionId = SessionVariables.WebSession.ID;
            action.ActorId = actorId;
            action.AccountId = accountId; //.. logged in account id
            action.ActionTypeId = actionType;
            action.ObjectTypeId = objectType;
            action.ObjectId = objectId;
            action.IndirectObjectId = indirectObjectId;
            action.ActionStringFormat = actionStringFormat;
            action.ActionString = actionString;
            action.Save();
        }
    }
}