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

        public ActionResult Index()
        {
            var msgs = new SystemMessages();

            if (SessionVariables.OriginalLoggedInUser.IsNotEmpty())
            {
                msgs.AddRange(LoadSessionInfo(SessionVariables.OriginalLoggedInUser));
                SessionVariables.OriginalLoggedInUser = null;
                return LoginRedirect();
            }
            else
            {
                return Logout();
            }
        }

    }
}
