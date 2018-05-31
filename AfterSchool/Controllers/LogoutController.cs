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
                Session.Clear();

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

                //Response.Redirect("/login");   
                return RedirectToAction("index", "Login");
            }
        }

    }
}
