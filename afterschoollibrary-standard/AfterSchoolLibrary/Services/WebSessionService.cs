using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class WebSessionService
    {
        //public static void InitializeWebSession()
        //{
        //    if (SessionVariables.VisitorSession.IsEmpty())
        //    {
        //        var session = new Session();
        //        session.LastModifiedDate = session.CreateDate = session.LastActivity = DateTime.UtcNow;
        //        session.LastModifiedBy = session.CreatedBy = 0;
        //        session.UrlReferrer = Request.UrlReferrer.IsNotEmpty() ? Request.UrlReferrer.ToString() : null;
        //        session.IPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"]; //Request.UserHostAddress;
        //        session.UserAgent = Request.UserAgent;
        //        session.Save();
        //        SessionVariables.VisitorSession = session;
        //    }
        //    else
        //    {
        //        SessionVariables.VisitorSession.LastActivity = DateTime.UtcNow;
        //        SessionVariables.VisitorSession.Save();
        //    }

        //}



        public static void LogActivity(WebUser actor, int? activityId, WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObject = ObjectType.NotSet)
        {
            try
            {
                //.. actor should not be null 
                var action = new WebSessionAction();
                //action.SessionId = SessionVariables.WebSession.ID;
                action.ActorId = actor.ID;
                //action.AccountId = SessionVariables.CurrentAccount.IsNotEmpty() ? SessionVariables.CurrentAccount.ID : null;
                action.ActionTypeId = actionType;
                action.ObjectTypeId = objectType;
                action.ObjectId = null; //not sure what data will be here
                action.IndirectObjectId = null;//not sure what data will be here

                var actionString = $"{actor.FirstName} {actionType}";
                action.ActionStringFormat = actionString;

                if (objectType != ObjectType.NotSet)
                    actionString += $"{objectType}";

                if (indirectObject != ObjectType.NotSet)
                    actionString += $"{indirectObject}";

                action.ActionString = actionString;
                action.Save();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
