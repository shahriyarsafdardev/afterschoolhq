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
            if (webSession.IsNotEmpty() && SessionVariables.CurrentAccount.IsNotEmpty())
            {
                SessionVariables.CurrentAccount.LogActivity(
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
    }
}