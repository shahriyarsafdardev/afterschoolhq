using AfterSchoolLibrary.Models;
using System;
using System.Linq;
using System.Web;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

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

        public static void LogActivity(this WebSession webSession, HttpRequestBase request, WebUser actor, Account account, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObjectType = ObjectType.NotSet,
             int? objectId = null, string objectText = "",
             int? indirectObjectId = null, string indirectObjectText = "")
        {
            //if webSession is null then try to create it here
            if (webSession.IsEmpty())
            {
                webSession = InitializeWebSession(request, actor, account);
                SessionVariables.WebSession = webSession;
            }

            //LogActivity with websession id
            if (webSession.IsNotEmpty())
                LogActivity(
                    webSession,
                    account,
                    actor,
                    actionType,
                    objectType,
                    indirectObjectType,
                    objectId,
                    objectText,
                    indirectObjectId,
                    indirectObjectText);
        }

        public static void LogActivity(this WebSession webSession, Account account,
            WebUser actor, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObjectType = ObjectType.NotSet,
            int? objectId = null, string objectText = "",
            int? indirectObjectId = null, string indirectObjectText = "",
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            LogActivity(webSession, account?.ID, actor, actionType, objectType, indirectObjectType, objectId, objectText, indirectObjectId, indirectObjectText, msgs, dc);
        }

        public static void LogActivity(this WebSession webSession, int? accountId, WebUser actor, WebSessionActionType actionType,
            ObjectType objectType, ObjectType indirectObjectType, int? objectId, string objectText, int? indirectObjectId,
            string indirectObjectText, SystemMessages msgs, DatabaseConnection dc)
        {
            //..actor	action		    Object		            ObjectType	        IndirectObjectId
            //..Bharat  Navigated to    FIRST SAMUEL CHURCH(W)  DirectoryAccount   ISYP

            string actorName = (actor.IsNotEmpty() ? actor.DisplayName : "Someone");

            var actionStringDict = AfterSchoolLibrary.Helpers.WebSessionHelper.GetActionString(objectType, actorName,
                actionType, objectText, indirectObjectType, indirectObjectText);

            SaveLog(actor: actor,
                actionStringFormat: actionStringDict.SingleOrDefault(kp => kp.Key == "ActionStringFormat").Value,
                actionString: actionStringDict.SingleOrDefault(kp => kp.Key == "ActionString").Value,
                webSessionId: webSession.ID.Value,
                accountId: accountId,
                actionType: actionType,
                objectType: objectType,
                objectId: objectId,
                indirectObjectId: indirectObjectId,
                msgs: msgs,
                dc: dc);
        }

        public static void SaveLog(WebUser actor, int webSessionId, string actionStringFormat, string actionString, int? accountId = null,
            WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObjectType = ObjectType.NotSet,
            int? objectId = null, int? indirectObjectId = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var action = new WebSessionAction();
            action.SessionId = webSessionId;
            action.AccountId = accountId;
            action.ActionTypeId = actionType;
            action.ObjectTypeId = objectType;
            action.ObjectId = objectId;
            action.IndirectObjectId = indirectObjectId;
            action.IndirectObjectTypeId = indirectObjectType;
            action.ActionStringFormat = actionStringFormat;
            action.ActionString = actionString.Truncate(1000);
            action.CreateDate = DateTime.UtcNow;
            action.CreatedBy = actor?.ID.Value;

            msgs.AddRange(action.Save(dc));
        }
        
        public static void LogRefundCompleted(this WebSession webSession, Account currentAccount, WebUser currentUser, WebSessionActionType webSessionActionType, string amount, Transaction transaction, SystemMessages msgs)
        {
            var activityId = ActivityPayment.LoadByTransactionIdByIsDeleted(transaction.ID.Value, false, msgs)?.FirstOrDefault()?.ParticipationObjectId.Value;
            var activity = Activity.Load(activityId.Value, msgs);

            webSession.LogActivity(
                account: currentAccount,
                actor: currentUser,
                actionType: webSessionActionType,
                objectType: ObjectType.Transaction,
                objectId: transaction.ID.Value,
                objectText: amount,
                indirectObjectId: activity.ID.Value,
                indirectObjectText: activity.Name,
                indirectObjectType: ObjectType.Activity
            );
        }

        public static void LogRefundRequestUpdate(this WebSession webSession, Account currentAccount,
            WebUser currentUser, WebSessionActionType webSessionActionType, string amount, RefundRequest refundRequest,
            SystemMessages msgs)
        {
            webSession.LogActivity(
                account: currentAccount,
                actor: currentUser,
                actionType: webSessionActionType,
                objectType: ObjectType.RefundRequest,
                objectId: refundRequest.ID.Value,
                objectText: amount
            );
        }
    }
}