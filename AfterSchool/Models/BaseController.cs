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
using Amazon.DynamoDBv2;

namespace AfterSchool.Models
{
    public class BaseController : System.Web.Mvc.Controller
    {
        public BaseController()
        {

        }

        protected bool IsRequestFromApp()
        {
            if (Request.Headers["Origin"].IsNotEmpty() || Request.Headers["Referer"].IsNotEmpty())
            {
                var origin = (Request.Headers["Origin"] ?? Request.Headers["Referer"]).ToLower();

                if (origin.StartsWith("https://afterschoolhq.com") ||
                    origin.StartsWith("https://qa.afterschoolhq.com") ||
                    origin.StartsWith("https://ci.afterschoolhq.com") ||
                    origin.StartsWith("https://staging.afterschoolhq.com") ||
                    origin.StartsWith("https://directory.afterschoolhq.com") ||
                    origin.StartsWith("https://qa.afterschoolfoundation.us") ||
                    origin.StartsWith("https://afterschoolfoundation.us") ||
                    origin.StartsWith("https://riseproject.afterschoolhq.com") ||
                    origin.StartsWith("http://localhost:54849") ||
                    Account.LoadAllSubdomains().Contains(origin)
                    )
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            Utilities.DetectAfterSchool();

            //check if db exisit then create session
            var DoesTableExist = WebSession.DoesTableExists();
            if (DoesTableExist)
                SessionVariables.WebSession = WebSessionUtilities.InitializeWebSession(Request, SessionVariables.LoggedInUser, SessionVariables.CurrentAccount);
        }

        protected bool IsLoggedIn()
        {
            return !SessionVariables.CurrentUser.IsEmpty() && SessionVariables.CurrentUser.IsActive;
        }

        protected void HandleIsLoggedIn(HttpContextBase requestContext)
        {
            if (!IsLoggedIn())
                requestContext.Response.Redirect($"/login?returnurl={requestContext.Request.Url.PathAndQuery}");
        }

        protected void HandleSubdomainRedirect()
        {
            var subdomainAccount = Account.LoadBySubdomain(Request.Url.Host);
            if (subdomainAccount.IsNotEmpty() && Request.Url.AbsolutePath.IsEmpty())
               Response.Redirect($"https://{Request.Url.Host}/{subdomainAccount.UniqueName}");
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

        protected ActionResult LoginRedirect(Account registrationAccount = null, SystemMessages msgs = null)
        {
            if (msgs.IsNotEmpty())
                TempData["msgs"] = msgs;

            if (SessionVariables.CurrentAccount.IsEmpty())
            {
                return RedirectToAction("select", "login");
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
                SessionVariables.AccountUserType = user.GetUserType(account);
                SessionVariables.AccountPackage = account.GetActiveAccountPackage();
            }
            else
            {
                var awus = AccountWebUser.LoadByWebUserId(user.ID.Value, true, false, false, msgs);

                if (awus.Count == 1)
                {
                    SessionVariables.CurrentAccount = awus.First().ToAccount(true);
                }
                else if (source == "parent-login")
                {
                    var awu = awus.FirstOrDefault(item => item.AccountAccountTypeId == AccountType.Registrant);

                    if (awu.IsNotEmpty())
                        SessionVariables.CurrentAccount = awu.ToAccount(true);
                }

                if (SessionVariables.CurrentAccount.IsNotEmpty())
                {
                    SessionVariables.AccountUserType = user.GetUserType(SessionVariables.CurrentAccount);
                    SessionVariables.AccountPackage = SessionVariables.CurrentAccount.GetActiveAccountPackage();
                }
            }

            //set login cookies
            if (doRememberMe && SessionVariables.CurrentUser.IsNotEmpty())
            {
                // TO DO: RE-ADD Remember me code
                CookieVariables.CurrentUser = SessionVariables.CurrentUser.Uid.ToString();
                CookieVariables.CurrentAccount = SessionVariables.CurrentAccount?.Uid.ToString();
            }
        }

        internal ActionResult SwitchAccount(string redirectUrl = null, string newAccountUid = null)
        {
            SessionVariables.CurrentAccount = null;
            SessionVariables.CurrentRegistrant = null;

            if (!newAccountUid.IsGuid())
            {
                if (redirectUrl.IsNotEmpty())
                    return Redirect(redirectUrl);

                return RedirectToAction("select", "login");
            }

            var account = Account.LoadByUid(newAccountUid.ToGuid());

            if (SessionVariables.LoggedInUser.HasAccessTo(account))
            {
                LoadSessionInfo(SessionVariables.CurrentUser, account);

                if (redirectUrl.IsNotEmpty())
                    return Redirect(redirectUrl);

                return LoginRedirect();
            }

            return Logout();
        }

        internal void ClearOriginalLoggedInUser()
        {
            SessionVariables.OriginalLoggedInUser = null;
        }

        internal void ClearCookies()
        {
            CookieVariables.CurrentUser = null;
            CookieVariables.CurrentAccount = null;
            CookieVariables.ParentAccount = null;
        }

        internal ActionResult Logout(string returnUrl = null, bool goToLoginScreenBeforeReturn = false)
        {
            SessionVariables.CurrentUser = null;
            SessionVariables.OriginalLoggedInUser = null;
            SessionVariables.CurrentAccount = null;
            SessionVariables.CurrentRegistrant = null;
            SessionVariables.ParentAccount = null;
            ClearCookies();

            if (returnUrl.IsNotEmpty() && !goToLoginScreenBeforeReturn)
                return Redirect(returnUrl);

            return RedirectToAction("index", "Login", new { returnurl = returnUrl, area = "" });
        }

        internal bool HandleInvite(string invite, Account currentAccount, WebUser currentUser, SystemMessages msgs)
        {
            // if there is no invite then who cares and move on
            if (invite.IsNotEmpty())
            {
                if (!invite.IsGuid())
                    msgs.Add("Invalid invitation code", MessageType.UserError);

                if (currentAccount.IsEmpty())
                    msgs.Add("Invalid account", MessageType.UserError);

                Account invitingAccount = null;

                if (msgs.HasNoErrors())
                {
                    invitingAccount = Account.LoadByInviteCodeToShare(invite.ToGuid(), msgs);

                    if (invitingAccount.IsEmpty())
                        msgs.Add("Invalid invitation code", MessageType.UserError);
                }

                if (msgs.HasNoErrors())
                    invitingAccount.ApproveAccessToAccount(currentAccount, currentUser, msgs);

                if (msgs.HasNoErrors())
                    msgs.Add($"You can now publish activities to {invitingAccount.Name}", MessageType.Success);
            }

            return msgs.HasNoErrors();
        }

        internal void HandleTempMessages(SystemMessages msgs)
        {
            if (TempData.IsNotEmpty() && TempData["msgs"].IsNotEmpty())
                msgs.AddRange((SystemMessages)TempData["msgs"]);
        }

        internal void SetTempMessage(SystemMessages msgs) => TempData["msgs"] = msgs;

        internal ActionResult HandleRedirectAfterLogin(string returnUrl, SystemMessages msgs = null)
        {
            ActionResult actionResult = LoginRedirect(msgs: msgs);
            var account = SessionVariables.CurrentAccount;
            const string adminPath = "/admin/";
            const string registrantPath = "/user/";
            const string districtPath = "/district/";
            const string orgPath = "/org/";

            if(returnUrl.IsNotEmpty())
            {
                switch (account.AccountTypeId)
                {
                    case AccountType.AfterSchool:
                        if (!returnUrl.ToLower().Contains(new string[] { orgPath, registrantPath, districtPath }))
                            actionResult = Redirect(returnUrl);
                        break;
                    case AccountType.Registrant:
                        if (!returnUrl.ToLower().Contains(new string[] { orgPath, adminPath, districtPath }))
                            actionResult = Redirect(returnUrl);
                        break;
                    case AccountType.SchoolDistrict:
                        if (!returnUrl.ToLower().Contains(new string[] { orgPath, adminPath, registrantPath }))
                            actionResult = Redirect(returnUrl);
                        break;
                    default:
                        if (account.AccountTypeId.In(AccountService.ProviderAccountTypes) && !returnUrl.ToLower().Contains(new string[] { adminPath, registrantPath, districtPath }))
                            actionResult = Redirect(returnUrl);
                        break;

                }
            }

            return actionResult;
        }

        internal bool isUnsupportedBrowser()
        {
            if (Request.Browser.Browser == "IE" || Request.Browser.Browser == "InternetExplorer")
                return true;

            return false;
        }

        private const string UnsupportedBrowserPage = "/unsupportedbrowser.html";

        internal ActionResult RedirectUnsupportedBrowser()
        {
            return Redirect(UnsupportedBrowserPage);
        }

        public void LoadTempMsgAndLogErrors(SystemMessages msgs = null)
        {
            // store temp msgs
            SetTempMessage(msgs: msgs);
            
            if (msgs.HasErrors())
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(msgs); // log all error msgs 
        }
    }
}
