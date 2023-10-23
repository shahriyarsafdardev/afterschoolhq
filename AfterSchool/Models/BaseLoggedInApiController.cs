using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Web.Mvc;

namespace AfterSchool.Models
{
    public class BaseLoggedInApiController : BaseController
    {
        private List<AccountType> AllowedAccountTypes { get; }

        public BaseLoggedInApiController(List<AccountType> allowedAccountTypes)
        {
            AllowedAccountTypes = allowedAccountTypes;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (SessionVariables.CurrentUser.IsEmpty()) return;

            SessionVariables.CurrentUser.UpdateLastActivity(new SystemMessages());
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
            var msgs = new SystemMessages();
            if (!HasAccess())
            {
                msgs.Add("Unauthorized Account", MessageType.UserError);
                filterContext.Result = LogoutAPI(msgs: msgs);
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
