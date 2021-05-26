using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Helpers;
using AfterSchoolLibrary.Helpers;

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


            if (SessionVariables.WebSession.IsEmpty() && request.IsNotEmpty())
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


        public static void LogActivity(HttpRequestBase request, WebUser actor, WebSession webSession, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObjectType = ObjectType.NotSet,
             int? objectId = (int?)null, string objectText = "",
             int? indirectObjectId = (int?)null, string indirectObjectText = "")
        {
            //if webSession is null then try to create it here
            if (webSession.IsEmpty())
            {
                webSession = InitializeWebSession(request, SessionVariables.LoggedInUser, SessionVariables.CurrentAccount);
                SessionVariables.WebSession = webSession;
            }

            //LogActivity with websession id
            if (webSession.IsNotEmpty())
            {
                LogActivity(
                    webSession.ID.Value,
                    actor,
                    actionType,
                    objectType,
                    indirectObjectType,
                    objectId,
                    objectText,
                    indirectObjectId,
                    indirectObjectText);
            }
        }

        public static void LogActivity(int webSessionId, WebUser actor, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObjectType = ObjectType.NotSet,
            int? objectId = (int?)null, string objectText = "",
            int? indirectObjectId = (int?)null, string indirectObjectText = "")
        {
            //..actor	action		    Object		            ObjectType	        IndirectObjectId
            //..Bharat  Navigated to    FIRST SAMUEL CHURCH(W)  DirectoryAccount   ISYP

            string actorName = (actor.IsNotEmpty() ? actor.FullName : "Someone");

            var actionStringDict = AfterSchoolLibrary.Helpers.WebSessionHelper.GetActionString(objectType, actorName, actionType, objectText, indirectObjectType, indirectObjectText);

            SaveLog(actor: actor,
                actionStringFormat: actionStringDict.SingleOrDefault(kp => kp.Key == "ActionStringFormat").Value,
                actionString: actionStringDict.SingleOrDefault(kp => kp.Key == "ActionString").Value,
                webSessionId: webSessionId,
                actorId: actor.IsNotEmpty() ? actor.ID : null,
                accountId: SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null,
                actionType: actionType,
                objectType: objectType,
                objectId: objectId,
                indirectObjectId: indirectObjectId);
        }

        public static void SaveLog(WebUser actor, int webSessionId, string actionStringFormat, string actionString, int? actorId = (int?)null, int? accountId = (int?)null,
            WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObjectType = ObjectType.NotSet,
            int? objectId = (int?)null, int? indirectObjectId = (int?)null)
        {
            var msgs = new SystemMessages();
            var action = new WebSessionAction();
            action.SessionId = webSessionId;
            action.AccountId = accountId; //.. logged in account id
            action.ActionTypeId = actionType;
            action.ObjectTypeId = objectType;
            action.ObjectId = objectId;
            action.IndirectObjectId = indirectObjectId;
            action.IndirectObjectTypeId = indirectObjectType;
            action.ActionStringFormat = actionStringFormat;
            action.ActionString = actionString;
            action.CreateDate = DateTime.UtcNow;
            action.CreatedBy = actor?.ID.Value;
            msgs.AddRange(action.Save());
        }
    }
}