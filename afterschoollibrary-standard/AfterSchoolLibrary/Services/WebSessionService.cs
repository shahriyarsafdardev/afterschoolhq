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
    }
}
