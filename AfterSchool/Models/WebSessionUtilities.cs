using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Helpers;

namespace AfterSchool.Models
{
    public static class WebSessionUtilities
    {
        public static WebSession InitializeWebSession(HttpRequestBase request, WebUser user = null, Account account = null)
        {
            var session = new WebSession();
            WebSession lastSession = new WebSession();

            if (user.IsNotEmpty())
                lastSession = WebSession.LoadMostRecentByUserId(webUserId: user.ID.Value);

            if (lastSession.IsEmpty() && account.IsNotEmpty())
                lastSession = WebSession.LoadMostRecentByAccountId(accountId: account.ID.Value);


            if (SessionVariables.WebSession.IsEmpty())
            {
                session.CreateDate = DateTime.UtcNow;
                session.LastModifiedDate = DateTime.UtcNow;
                session.LastModifiedBy = session.CreatedBy = user?.ID;
                session.UrlReferrer = request.UrlReferrer.IsNotEmpty() ? request.UrlReferrer.ToString() : null;
                session.IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                session.UserAgent = request.UserAgent;
                //.. need to retrieve this both from user location
                session.IPAddressTimeZone = null;
                session.UserLocation = null;
                
                session.WebUserId = user?.ID;
                session.AccountId = account?.ID;
            }
            else
            {
                session = WebSession.Load(SessionVariables.WebSession.ID.Value);
                session.LastModifiedDate = DateTime.UtcNow;
                session.LastModifiedBy = user?.ID;

                if (session.WebUserId.IsEmpty())
                    session.WebUserId = user?.ID;

                if (session.AccountId.IsEmpty())
                    session.AccountId = account?.ID;
            }

            if (session.PreviousId.IsEmpty() && lastSession.IsNotEmpty() && lastSession.ID.IsNotEmpty())
                session.PreviousId = lastSession.ID;

            session.Save();
            return session;
        }

        public static void LogActivity(WebUser actor, int webSessionId, WebSessionActionType actionType = WebSessionActionType.NotSet)
        {
            SaveLog(actorName: (actor.IsNotEmpty() ? actor.FirstName  : "Someone"),
                    actionStringFormat: "{actor} {action}",
                    actionString: $"{actor.FirstName} {actionType}",
                    webSessionId: webSessionId,
                    actorId: actor.IsNotEmpty() ? actor.ID : null,
                    accountId: SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null,
                    actionType: actionType,
                    objectType: ObjectType.NotSet,
                    objectId: null,
                    indirectObjectId: null);
        }

        public static void LogActivity(WebUser actor, int webSessionId, int? Id = (int?)null, string text = "", WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet)
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

                case ObjectType.WebUser:
                    actionStringFormat = "{actor} Signed up";
                    actionString = $"{actorName} Signed up";
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
                    webSessionId: webSessionId,
                    actorId: actor.IsNotEmpty() ? actor.ID : null,
                    accountId: SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null,
                    actionType: actionType,
                    objectType: objectType,
                    objectId: Id,
                    indirectObjectId: null);
        }

        public static void LogActivity(WebUser actor, int webSessionId, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet,
            int? objectId = (int?)null, string objectText = "",
            int? indirectObjectId = (int?)null, string indirectObjectText = "")
        {
            //..actor	action		    Object		            ObjectType	        IndirectObjectId
            //..Bharat  Navigated to    FIRST SAMUEL CHURCH(W)  DirectoryAccount   ISYP

            string actionString = null, actionStringFormat = null, actorName = (actor.IsNotEmpty() ? actor.FirstName : "Someone");

            var actionStringObject = WebSessionHelper.GetActionString()

            SaveLog(actorName: (actor.IsNotEmpty() ? actor.FirstName : "Someone"),
                actionStringFormat: actionStringFormat,
                actionString: actionString,
                webSessionId: webSessionId, 
                actorId: actor.IsNotEmpty() ? actor.ID : null,
                accountId: SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null,
                actionType: actionType,
                objectType: objectType,
                objectId: objectId,
                indirectObjectId: indirectObjectId);
        }

        public static void SaveLog(string actorName, string actionStringFormat, string actionString, int webSessionId, int? actorId = (int?)null, int? accountId = (int?)null,
            WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet,
            int? objectId = (int?)null, int? indirectObjectId = (int?)null)
        {
            var action = new WebSessionAction();
            action.SessionId = webSessionId;
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