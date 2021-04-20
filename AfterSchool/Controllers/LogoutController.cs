using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary;

namespace AfterSchool.Controllers
{
    public class LogoutController : BaseController
    {
        //
        // GET: /Logout/

        public ActionResult Index(string returnurl)
        {
            var msgs = new SystemMessages();

            if (SessionVariables.OriginalLoggedInUser.IsNotEmpty())
            {
                var remember = CookieVariables.CurrentUser.IsGuid();
                var user = SessionVariables.OriginalLoggedInUser;                    

                ClearCookies();
                LoadSessionInfo(user, null, remember, msgs);
                ClearOriginalLoggedInUser();

                return LoginRedirect();
            }

            if (SessionVariables.CurrentUser.IsNotEmpty())
                WebSessionUtilities.LogActivity(actor: SessionVariables.CurrentUser, webSessionId: SessionVariables.WebSession.ID.Value, actionType: WebSessionActionType.LoggedOut);

            return Logout(returnurl);
        }

        public ActionResult Switch(string id, string redirectUrl)
        {
            return SwitchAccount(redirectUrl, id);
        }

    }
}
