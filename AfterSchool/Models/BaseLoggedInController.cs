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
        private List<AccountType> AllowedAccountTypes { get; }

        public BaseLoggedInController(List<AccountType> allowedAccountTypes)
        {
            AllowedAccountTypes = allowedAccountTypes;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (SessionVariables.CurrentUser.IsEmpty()) return;

            SessionVariables.CurrentUser.UpdateLastActivity(new SystemMessages());

            if (SessionVariables.CurrentAccount.IsEmpty())
                CheckAccount(requestContext.HttpContext.Request);
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

        protected static SystemMessages CheckAccount(HttpRequestBase request)
        {
            var msgs = new SystemMessages();
            if (!request.QueryString["Account"].IsNumber()) return msgs;

            msgs.Add("Got Account in QS and it is a number: " + request.QueryString["Account"], MessageType.Notice);

            var accountId = request.QueryString["Account"].ToInteger();

            if (!SessionVariables.CurrentAccount.IsEmpty() && accountId == SessionVariables.CurrentAccount.ID.Value) return msgs;

            msgs.Add("Checking if a member of this Account: " + accountId + " " + SessionVariables.CurrentUser.ID.Value, MessageType.Notice);

            var gwu = AccountWebUser.LoadByAccountIdByWebUserId(accountId, SessionVariables.CurrentUser.ID.Value, null);

            if (gwu.IsEmpty()) return msgs;

            msgs.Add("GWU is not empty: " + gwu.ID.Value, MessageType.Notice);
            SessionVariables.CurrentAccount = Account.Load(accountId, null);

            if (SessionVariables.CurrentAccount.IsEmpty()) return msgs;

            SessionVariables.AccountUserType = SessionVariables.CurrentUser.GetUserType(SessionVariables.CurrentAccount);
            msgs.Add("Account is not empty: " + SessionVariables.CurrentAccount.ID.Value, MessageType.Notice);

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
