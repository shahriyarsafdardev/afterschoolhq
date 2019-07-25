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
using AfterSchoolLibrary.Models;

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
                var session = new Session();
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
                    CreatedBy = SessionVariables.CurrentUser.IsNotEmpty() ? SessionVariables.CurrentUser.ID : 0
                }.Save(dc: dc);
            }
        }

        protected bool IsLoggedIn()
        {
            return !SessionVariables.CurrentUser.IsEmpty();
        }

        protected void HandleIsLoggedIn(HttpContextBase requestContext)
        {
            if (!IsLoggedIn())
                requestContext.Response.Redirect($"/login?returnurl={requestContext.Request.Url.PathAndQuery}");
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
            if (SessionVariables.CurrentAccount.IsEmpty())
            {
                // take to account creation screen
                return RedirectToAction("createaccount", "signup");
            }
            else if (SessionVariables.CurrentUser.HasAccessTo(SessionVariables.CurrentAccount))
            {
                switch (SessionVariables.CurrentAccount.AccountTypeId)
                {
                    case AccountType.AfterSchool:
                        return RedirectToAction("index", "account", new { area = "admin" });

                    case AccountType.SchoolDistrict:
                        return RedirectToAction("index", "home", new { area = "district" });

                    case AccountType.Registrant:
                        if (SessionVariables.Cart.IsNotEmpty())
                            return RedirectToAction("select", "account", new { area = "" });
                        else if (registrationAccount.IsNotEmpty())
                            return Redirect(registrationAccount.HomePageUrl);
                        else
                            return RedirectToAction("index", "home", new { area = "user" });

                    default:
                        return RedirectToAction("index", "home", new { area = "org" });
                }
            }

            return RedirectToAction("index", "login", new { area = "" });
        }

        protected void LoadSessionInfo(WebUser user, SystemMessages msgs = null)
        {
            LoadSessionInfo(user, null, false, null, msgs);
        }

        protected void LoadSessionInfo(WebUser user, Account account, SystemMessages msgs = null)
        {
            LoadSessionInfo(user, account, false, null, msgs);
        }

        protected void LoadSessionInfo(WebUser user, Account account, bool doRememberMe, SystemMessages msgs = null)
        {
            LoadSessionInfo(user, account, doRememberMe, null, msgs);
        }

        protected void LoadSessionInfo(WebUser user, Account account, bool doRememberMe, string source, SystemMessages msgs = null)
        {
            SessionVariables.CurrentAccount = null;
            SessionVariables.CurrentUser = user;

            if (account.IsNotEmpty())
            {
                SessionVariables.CurrentAccount = account;
                SessionVariables.AccountUserType = SessionVariables.CurrentUser.GetUserType(account);
            }
            else
            {
                var awus = AccountWebUser.LoadByWebUserIdByIsActive(user.ID.Value, true);

                if (awus.Count == 1)
                {
                    SessionVariables.CurrentAccount = awus.First().ToAccount(true);
                    SessionVariables.AccountUserType = awus.First().UserTypeId;
                }
                else if (source == "parent-login")
                {
                    var awu = awus.FirstOrDefault(item => item.AccountAccountTypeId == AccountType.Registrant);

                    if (awu.IsNotEmpty())
                    {
                        SessionVariables.CurrentAccount = awu.ToAccount(true);
                        SessionVariables.AccountUserType = awu.UserTypeId;
                    }
                }
            }

            if (SessionVariables.CurrentAccount.IsNotEmpty())
                SessionVariables.AccountPackage = SessionVariables.CurrentAccount.GetActiveAccountPackage();

            //set login cookies
            if (doRememberMe)
            {
                // TO DO: RE-ADD Remember me code
            }
        }

        internal ActionResult Logout(string returnUrl = null, bool goToLoginScreenBeforeReturn = false)
        {
            SessionVariables.CurrentUser = null;
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

            if (returnUrl.IsNotEmpty() && !goToLoginScreenBeforeReturn)
                return Redirect(returnUrl);

            return RedirectToAction("index", "Login", new { returnurl = returnUrl });
        }
    }
}
