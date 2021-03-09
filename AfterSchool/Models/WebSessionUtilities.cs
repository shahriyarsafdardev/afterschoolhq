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
            if (SessionVariables.WebSession.IsEmpty())
            {
                var session = new WebSession();
                session.LastModifiedDate = DateTime.UtcNow;
                session.LastModifiedBy = session.CreatedBy = 0;
                session.UrlReferrer = request.UrlReferrer.IsNotEmpty() ? request.UrlReferrer.ToString() : null;
                session.IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"]; //Request.UserHostAddress;
                session.UserAgent = request.UserAgent;
                session.Save();
                SessionVariables.WebSession = session;
            }
            else
            {
                SessionVariables.WebSession.LastModifiedDate = DateTime.UtcNow;
                SessionVariables.WebSession.Save();
            }
        }
    }
}