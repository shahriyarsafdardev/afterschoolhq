using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Web.Mvc;

namespace AfterSchool.Models
{
    public class BaseLoggedInController : BaseController
    {
        private List<AccountType> AllowedAccountTypes { get; set; }

        public BaseLoggedInController(List<AccountType> allowedAccountTypes)
        {
            this.AllowedAccountTypes = allowedAccountTypes;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (SessionVariables.LoggedInUser.IsEmpty())
            {
                WebUser user = null;
                Account Account = null;

                // Check cookies
                if (!Request.Cookies["sessionuser"].IsEmpty() && !Request.Cookies["sessionAccount"].IsEmpty())
                {
                    try
                    {
                        var userGuid = Guid.Parse(BasicLibrary.Encryption.DecryptToString(
                            Convert.FromBase64String(Request.Cookies["sessionuser"].Value),
                            AfterSchoolLibrary.Helpers.Utilities.AppAESKey, AfterSchoolLibrary.Helpers.Utilities.AppIV));
                        user = WebUser.LoadByUid(userGuid);

                        var AccountUniqueName = BasicLibrary.Encryption.DecryptToString(
                            Convert.FromBase64String(Request.Cookies["sessionAccount"].Value),
                            AfterSchoolLibrary.Helpers.Utilities.AppAESKey, AfterSchoolLibrary.Helpers.Utilities.AppIV);
                        Account = Account.LoadByUniqueName(AccountUniqueName);
                    }
                    catch
                    { }
                }

                // Auto login
                if (!user.IsEmpty() && !Account.IsEmpty())
                {
                    SessionVariables.LoggedInUser = user;
                    SessionVariables.CurrentAccount = Account;
                    SessionVariables.AccountUserType = user.GetUserType(Account);
                }
            }

            if (SessionVariables.LoggedInUser.IsNotEmpty())
            {
                SessionVariables.LoggedInUser.UpdateLastActivity();

                if (SessionVariables.CurrentAccount.IsEmpty())
                {
                    BaseLoggedInController.CheckAccount(requestContext.HttpContext.Request);
                }
                
                // BaseLoggedInController.CheckAccount(requestContext.HttpContext.Request);
            }
        }

        private bool HasAccess()
        {
            if (SessionVariables.LoggedInUser.IsEmpty())
                return false;

            if (AllowedAccountTypes.IsNotEmpty() &&
                    AllowedAccountTypes.Any() &&
                    SessionVariables.CurrentAccount.AccountTypeId.NotIn(AllowedAccountTypes))
                return false;

            return true;
        }

        protected override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            if (!HasAccess())
            {
                filterContext.Result = RedirectToAction(
                    "index",
                    "login",
                    new
                    {
                        returnurl = filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.Url.PathAndQuery),
                        area = ""
                    }
                );
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }

        protected static SystemMessages CheckAccount(HttpRequestBase Request)
        {
            SystemMessages msgs = new SystemMessages();
            if (Request.QueryString["Account"].IsNumber())
            {
                msgs.Add("Got Account in QS and it is a number: " + Request.QueryString["Account"], MessageType.Notice);

                int AccountId = Request.QueryString["Account"].ToInteger();
                if (SessionVariables.CurrentAccount.IsEmpty() || AccountId != SessionVariables.CurrentAccount.ID.Value)
                {
                    msgs.Add("Checking if a member of this Account: " + AccountId + " " + SessionVariables.LoggedInUser.ID.Value, MessageType.Notice);

                    AccountWebUser gwu = AccountWebUser.LoadByAccountIdByWebUserId(AccountId, SessionVariables.LoggedInUser.ID.Value, null);
                    if (!gwu.IsEmpty())
                    {
                        msgs.Add("GWU is not empty: " + gwu.ID.Value, MessageType.Notice);
                        SessionVariables.CurrentAccount = Account.Load(AccountId, null);

                        if (!SessionVariables.CurrentAccount.IsEmpty())
                        {
                            SessionVariables.AccountUserType = SessionVariables.LoggedInUser.GetUserType(SessionVariables.CurrentAccount);
                            msgs.Add("Account is not empty: " + SessionVariables.CurrentAccount.ID.Value, MessageType.Notice);
                        }
                    }
                }

                //AfterSchoolLibrary.RebornMail.Send(new System.Net.Mail.MailMessage(AfterSchoolLibrary.Helpers.Email.AppEmailAddress, "darye@reborncode.com", "Testing Account", msgs.RenderMessageList(MessageOutputType.Html)));
            }

            return msgs;
        }
    }
}
