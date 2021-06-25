using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore.Update;

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

            var msgs = new SystemMessages();

            if (SessionVariables.CurrentUser.IsEmpty())
            {
                WebUser user = null;
                Account Account = null;

                // Auto login
                if (!user.IsEmpty() && !Account.IsEmpty())
                {
                    SessionVariables.CurrentUser = user;
                    SessionVariables.CurrentAccount = Account;
                    SessionVariables.AccountUserType = user.GetUserType(Account);
                }
            }

            if (SessionVariables.CurrentUser.IsNotEmpty())
            {
                SessionVariables.CurrentUser.UpdateLastActivity(msgs);

                if (SessionVariables.CurrentAccount.IsEmpty())
                    BaseLoggedInController.CheckAccount(requestContext.HttpContext.Request);
            }
        }

        private bool HasAccess()
        {
            if (!IsLoggedIn())
                return false;

            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (account.IsNotEmpty() && 
                !account.IsDeleted && 
                currentUser.HasAccessTo(account) && 
                AllowedAccountTypes.IsNotEmpty() && 
                AllowedAccountTypes.Any() && 
                account.AccountTypeId.In(AllowedAccountTypes))
            {
                LoadSessionInfo(currentUser, account);
                return true;
            } 

            return false;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!HasAccess())
                filterContext.Result = Logout();

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
                    msgs.Add("Checking if a member of this Account: " + AccountId + " " + SessionVariables.CurrentUser.ID.Value, MessageType.Notice);

                    AccountWebUser gwu = AccountWebUser.LoadByAccountIdByWebUserId(AccountId, SessionVariables.CurrentUser.ID.Value, null);
                    if (!gwu.IsEmpty())
                    {
                        msgs.Add("GWU is not empty: " + gwu.ID.Value, MessageType.Notice);
                        SessionVariables.CurrentAccount = Account.Load(AccountId, null);

                        if (!SessionVariables.CurrentAccount.IsEmpty())
                        {
                            SessionVariables.AccountUserType = SessionVariables.CurrentUser.GetUserType(SessionVariables.CurrentAccount);
                            msgs.Add("Account is not empty: " + SessionVariables.CurrentAccount.ID.Value, MessageType.Notice);
                        }
                    }
                }

                //AfterSchoolLibrary.RebornMail.Send(new System.Net.Mail.MailMessage(AfterSchoolLibrary.Helpers.Email.AppEmailAddress, "darye@reborncode.com", "Testing Account", msgs.RenderMessageList(MessageOutputType.Html)));
            }

            return msgs;
        }

        protected ActionResult HandleOnboardingRedirect()
        {
            var account = SessionVariables.CurrentAccount;

            switch(account.OnboardingStepDone)
            {
                case OnboardingStepType.AccountType:
                    return RedirectToAction("programinfo", "onboarding");
                case OnboardingStepType.ProgramInfo:
                    return RedirectToAction("additionalinfo", "onboarding");
                case OnboardingStepType.AdditionalInfo:
                    return RedirectToAction("contactinfo", "onboarding");
                case OnboardingStepType.ContactInfo:
                    return RedirectToAction("logo", "onboarding");
                case OnboardingStepType.LogoAndHeader:
                    return RedirectToAction("servicesoffered", "onboarding");
                case OnboardingStepType.ServicesOffered:
                    return RedirectToAction("links", "onboarding");
                case OnboardingStepType.SocialLinks:
                    return RedirectToAction("done", "onboarding");
            }

            return RedirectToAction("index", "onboarding");
        }
    }
}
