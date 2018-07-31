using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using BasicLibrary.DataAccess;

namespace AfterSchool.Models
{
    public class BaseController : System.Web.Mvc.Controller
    {
        public BaseController()
        {

        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            Utilities.DetectAfterSchool();

            if (SessionVariables.VisitorSession.IsEmpty())
            {
                var session = new AfterSchoolLibrary.Session();
                session.LastModifiedDate = session.CreateDate = session.LastActivity = DateTime.UtcNow;
                session.LastModifiedBy = session.CreatedBy = 0;
                session.UrlReferrer = Request.UrlReferrer.IsNotEmpty() ? Request.UrlReferrer.ToString() : null;
                session.IPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"]; //Request.UserHostAddress;
                session.UserAgent = Request.UserAgent;
                session.Save();
                SessionVariables.VisitorSession = session;
            }
            else
            {
                SessionVariables.VisitorSession.LastActivity = DateTime.UtcNow;
                SessionVariables.VisitorSession.Save();
            }
                    
        }

        protected void UpdateSession(SessionActionType sat, int? accountId = null, int? activityId = null, int? webUserId = null, int? registrantId = null, int? registrationId = null, int? registrationPeriodId = null, int? activityGroupId = null, DatabaseConnection dc = null)
        {
            if (SessionVariables.VisitorSession.IsNotEmpty())
            {
                new SessionAction
                {
                    ActivityId = activityId,
                    AccountId = accountId,
                    SessionActionTypeId = sat,
                    WebUserId = webUserId,
                    SessionId = SessionVariables.VisitorSession.ID,
                    RegistrantId = registrantId,
                    RegistrationPeriodId = registrationPeriodId,
                    ActivityGroupId = activityGroupId,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = SessionVariables.LoggedInUser.IsNotEmpty() ? SessionVariables.LoggedInUser.ID : 0
                }.Save(dc: dc);
            }
        }

        protected bool IsLoggedIn()
        {
            return !SessionVariables.LoggedInUser.IsEmpty();
        }

        protected void HandleIsLoggedIn(HttpContextBase requestContext)
        {
            if (!IsLoggedIn())
                requestContext.Response.Redirect("/login?url=" + requestContext.Server.UrlEncode(requestContext.Request.Url.PathAndQuery));
        }

        protected void WriteOutput(object obj, ResponseType rt)
        {
            switch (rt)
            {
                case ResponseType.JSON:
                    Response.ContentType = "application/json";
                    JavaScriptSerializer jsserializer = new JavaScriptSerializer();
                    Response.Write(jsserializer.Serialize(obj));
                    break;
                case ResponseType.XML:
                    Response.ContentType = "application/xml";
                    obj.ToXml().Save(Response.Output);
                    break;
            }
        }

        protected void HandleErrors(SystemMessages msgs, ResponseType rt)
        {
            if (msgs.HasErrors())
            {
                List<object> list = new List<object>();

                foreach (SystemMessage msg in msgs)
                {
                    object message = new { text = msg.Text, type = msg.MessageType.ToString() };
                    switch (rt)
                    {
                        case ResponseType.JSON:
                            list.Add(message);
                            break;
                        case ResponseType.XML:
                            list.Add(new { message = message });
                            break;
                    }
                }

                object obj = new { status = "error", messages = list.ToArray() };
                switch (rt)
                {
                    case ResponseType.JSON:
                        JavaScriptSerializer jsserializer = new JavaScriptSerializer();
                        Response.Write(jsserializer.Serialize(obj));
                        break;
                    case ResponseType.XML:
                        Response.Write(obj.ToXml().ToString());
                        break;
                }
            }
        }

        protected ActionResult LoginRedirect(Account registrationAccount = null)
        {
            if (SessionVariables.LoggedInUser.HasAccessTo(SessionVariables.CurrentAccount))
                switch (SessionVariables.CurrentAccount.AccountTypeId)
                {
                    case AccountType.AfterSchool:
                        return RedirectToAction("index", "account", new { area = "admin" });

                    case AccountType.SchoolDistrict:
                        if (SessionVariables.AccountPackage.IsEmpty())
                            return RedirectToAction("newbilling", "account", new { area = "district" });

                        return RedirectToAction("index", "home", new { area = "district" });

                    case AccountType.Registrant:
                        if (SessionVariables.Cart.IsNotEmpty())
                            return RedirectToAction("select", "account");
                        else if (registrationAccount.IsNotEmpty())
                            return Redirect(registrationAccount.HomePageUrl);
                        else
                            return RedirectToAction("index", "home", new { area = "user" });

                    default:
                        if (SessionVariables.AccountPackage.IsEmpty())
                            return RedirectToAction("newbilling", "account", new { area = "org" });

                        return RedirectToAction("index", "home", new { area = "org" });
                }

            return RedirectToAction("index", "login", new { area = "" });
        }

        protected SystemMessages LoadSessionInfo(WebUser user, bool doRememberMe = false)
        {
            var msgs = new SystemMessages();

            SessionVariables.LoggedInUser = user;
            var awus = AccountWebUser.LoadByWebUserIdByIsActive(user.ID.Value, true);

            if (awus.Any())
            {
                if (awus.Where(x => x.IsDefault).Any())
                {
                    SessionVariables.CurrentAccount = awus.Where(x => x.IsDefault).First().ToAccount(true);
                    SessionVariables.AccountUserType = awus.Where(x => x.IsDefault).First().UserTypeId;
                }
                else
                {
                    SessionVariables.CurrentAccount = awus.First().ToAccount(true);
                    SessionVariables.AccountUserType = awus.First().UserTypeId;
                }

                SessionVariables.AccountPackage = SessionVariables.CurrentAccount.GetActiveAccountPackage();                            
            }

            //set login cookies
            if (doRememberMe)
            {
                var cookieUser = new HttpCookie("sessionuser", Convert.ToBase64String(Encryption.Encrypt(
                    SessionVariables.LoggedInUser.Uid.ToString(), AfterSchoolLibrary.Utilities.AppAESKey, AfterSchoolLibrary.Utilities.AppIV)));
                cookieUser.Expires = DateTime.Now.AddMonths(1);
                Response.Cookies.Add(cookieUser);

                if (!SessionVariables.CurrentAccount.IsEmpty())
                {
                    var cookieAccount = new HttpCookie("sessionAccount", Convert.ToBase64String(Encryption.Encrypt(
                        SessionVariables.CurrentAccount.UniqueName, AfterSchoolLibrary.Utilities.AppAESKey, AfterSchoolLibrary.Utilities.AppIV)));
                    cookieAccount.Expires = DateTime.Now.AddMonths(1);
                    Response.Cookies.Add(cookieAccount);
                }
            }

            return msgs;
        }

        internal ActionResult Logout(string returnUrl = null)
        {
            SessionVariables.LoggedInUser = null;
            SessionVariables.OriginalLoggedInUser = null;
            SessionVariables.CurrentAccount = null;
            SessionVariables.CurrentRegistrant = null;

            // Clear login cookies
            if (!Request.Cookies["sessionuser"].IsEmpty())
            {
                HttpCookie cookieUser = Request.Cookies["sessionuser"];
                Response.Cookies.Remove("sessionuser");
                cookieUser.Expires = DateTime.Now.AddDays(-1);
                cookieUser.Value = null;
                Response.SetCookie(cookieUser);
            }
            if (!Request.Cookies["sessionAccount"].IsEmpty())
            {
                HttpCookie cookieUser = Request.Cookies["sessionAccount"];
                Response.Cookies.Remove("sessionAccount");
                cookieUser.Expires = DateTime.Now.AddDays(-1);
                cookieUser.Value = null;
                Response.SetCookie(cookieUser);
            }

            if (returnUrl.IsNotEmpty())
                return Redirect(returnUrl);

            return RedirectToAction("index", "Login");
        }
    }
}
