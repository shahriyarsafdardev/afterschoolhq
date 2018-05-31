using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary;

namespace AfterSchool.Models
{
    public class BaseLoggedInController : BaseController
    {
        public BaseLoggedInController()
        {
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
                        Guid userGuid = Guid.Parse(BasicLibrary.Encryption.DecryptToString(
                            Convert.FromBase64String(Request.Cookies["sessionuser"].Value),
                            AfterSchoolLibrary.Utilities.AppAESKey, AfterSchoolLibrary.Utilities.AppIV));
                        user = WebUser.LoadByUid(userGuid);

                        string AccountUniqueName = BasicLibrary.Encryption.DecryptToString(
                            Convert.FromBase64String(Request.Cookies["sessionAccount"].Value),
                            AfterSchoolLibrary.Utilities.AppAESKey, AfterSchoolLibrary.Utilities.AppIV);
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
                BaseLoggedInController.CheckAccount(requestContext.HttpContext.Request);
            }
        }

        protected override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            if (SessionVariables.LoggedInUser.IsEmpty())
            {
                filterContext.Result = RedirectToAction(
                                            "index",
                                            "login",
                                            new
                                            {
                                                url = filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.Url.PathAndQuery),
                                                area = ""
                                            }
                                       );
            }
            base.OnActionExecuting(filterContext);
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

                //AfterSchoolLibrary.RebornMail.Send(new System.Net.Mail.MailMessage(AfterSchoolLibrary.Email.AppEmailAddress, "darye@reborncode.com", "Testing Account", msgs.RenderMessageList(MessageOutputType.Html)));
            }

            return msgs;
        }
    }
}
