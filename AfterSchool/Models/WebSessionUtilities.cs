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
            var user = SessionVariables.CurrentUser;
            var account = SessionVariables.CurrentAccount;
            var session = new WebSession();
            WebSession lastSession = new WebSession();

            if (user.IsNotEmpty())
                lastSession = WebSession.LoadByWebUserId(WebUserId: user.ID.Value).OrderByDescending(m => m.ID).FirstOrDefault();

            if (lastSession.IsEmpty() && account.IsNotEmpty())
                lastSession = WebSession.LoadByAccountId(AccountId: account.ID.Value).OrderByDescending(m => m.ID).FirstOrDefault();


            if (SessionVariables.WebSession.IsEmpty())
            {
                session.CreateDate = DateTime.UtcNow;
                session.CreatedBy = user.IsNotEmpty() && user.ID.IsNotEmpty() ? user.ID : 0;
                session.LastModifiedDate = DateTime.UtcNow;
                session.LastModifiedBy = session.CreatedBy = 0;
                session.UrlReferrer = request.UrlReferrer.IsNotEmpty() ? request.UrlReferrer.ToString() : null;
                session.IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                session.UserAgent = request.UserAgent;
                //.. need to retrieve this both from user location
                session.IPAddressTimeZone = null;
                session.UserLocation = null;
                
                session.WebUserId = user.IsNotEmpty() ? user.ID : null;
                session.AccountId = account.IsNotEmpty() ? account.ID : null;
            }
            else
            {
                session = WebSession.Load(SessionVariables.WebSession.ID.Value);
                session.LastModifiedDate = DateTime.UtcNow;
                session.LastModifiedBy = user.IsNotEmpty() && user.ID.IsNotEmpty() ? user.ID : 0;

                if (session.WebUserId.IsEmpty() && user.IsNotEmpty())
                    session.WebUserId = user.ID;

                if (session.AccountId.IsEmpty() && account.IsNotEmpty())
                    session.AccountId = account.ID;
            }

            if (session.PreviousId.IsEmpty() && lastSession.IsNotEmpty() && lastSession.ID.IsNotEmpty())
                session.PreviousId = lastSession.ID;

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

                case ObjectType.ContactList:
                    actionStringFormat = "{actor} Tried to {action} Account {objecttype}";
                    actionString = $"{actorName} Tried to Contact Account {text}";
                    break;

                case ObjectType.Registration:
                    if (actionType == WebSessionActionType.Interested)
                    {
                        actionStringFormat = "{actor} Has Registered {action} For The Activity {objecttype}";
                        actionString = $"{actorName} Has Registered Interest For The Activity {text}";
                    }                    
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

                case ObjectType.Payout:
                    actionStringFormat = "{actor} {action} {primaryObject} To {indirectObject}";
                    actionString = $"{actorName} {actionType} {objectText} To {indirectObjectText}";
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