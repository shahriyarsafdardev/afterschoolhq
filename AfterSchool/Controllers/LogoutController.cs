using System.Web.Mvc;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class LogoutController : BaseController
    {
        //
        // GET: /Logout/

        public ActionResult Index(string returnurl)
        {
            var msgs = new SystemMessages();

            if (SessionVariables.CurrentUser.IsNotEmpty())
            {
                // LOG: WebUserName LoggedOut
                SessionVariables.CurrentAccount.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: SessionVariables.CurrentUser,
                    actionType: WebSessionActionType.LoggedOut

                );
            }

            if (SessionVariables.OriginalLoggedInUser.IsNotEmpty())
            {
                var remember = CookieVariables.CurrentUser.IsGuid();
                var user = SessionVariables.OriginalLoggedInUser;                    

                ClearCookies();
                LoadSessionInfo(user, null, remember, msgs);
                ClearOriginalLoggedInUser();

                return LoginRedirect();
            }

            return Logout(returnurl);
        }

        public ActionResult Switch(string id, string redirectUrl)
        {
            return SwitchAccount(redirectUrl, id);
        }

    }
}
