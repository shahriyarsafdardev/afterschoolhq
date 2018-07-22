using System;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class LoginController : BaseController
    {
        //
        // GET: /Login/
        [HttpGet]
        public ActionResult Index(string regaccount)
        {
            var msgs = new SystemMessages();
            if (!SessionVariables.LoggedInUser.IsEmpty())
            {
                LoginRedirect();
            }
            else if (!Request["Account"].IsEmpty())
            {
                Account Account = AfterSchoolLibrary.Account.Load(Request["Account"].ToInteger());
                if (!Account.IsEmpty() && !Account.UniqueName.IsEmpty())
                {
                    Response.Redirect(Account.UniqueName);
                }
            }

            ViewBag.LoginType = "Internal";
            ViewBag.Messages = msgs;
            ViewBag.RegistrationAccount = regaccount;
            return View();
        }

        [HttpPost]
        public ActionResult Index(string username, string password, string regaccount)
        {
            var msgs = new SystemMessages();

            if (username.IsEmpty())
                msgs.Add("Username is required", MessageType.UserError);

            if (password.IsEmpty())
                msgs.Add("Password is required", MessageType.UserError);            

            if (msgs.HasNoErrors())
            {
                WebUser user = null;

                if (username.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    user = WebUser.LoginByEmail(username, password);
                else if (username.IsMatch(BasicLibrary.Utilities.PhoneRegEx))
                    user = WebUser.LoginByPhone(username, password);
                else
                    user = WebUser.LoginByUsername(username, password);

                if (user.IsEmpty())
                    msgs.Add("Invalid username and/or password", MessageType.UserError);
                else
                    msgs.AddRange(LoadSessionInfo(user, !Request.Params["remember"].IsEmpty()));
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                var registrationAccount = regaccount.IsGuid() ? Account.LoadByUid(regaccount.ToGuid()) : null;
                return LoginRedirect(registrationAccount);
            }
            else
                return View();
        }        

        [HttpGet]
        public ActionResult ResetPassword()
        {
            ViewBag.Messages = new SystemMessages();
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string email)
        {
            var msgs = new SystemMessages();
            WebUser user = null;

            if (!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
            {
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                user = WebUser.LoadByEmail(email);
                if (user.IsNotEmpty())
                {
                    msgs.AddRange(user.SendPasswordResetToken(SessionVariables.IsAfterSchool.Value));
                }
                
                msgs.Add("If an account exists for your email address, you will receive an email momentarily to reset your password", MessageType.Success);
            }

            ViewBag.Messages = msgs;
            return View();
        }

        [HttpGet]
        public ActionResult ChangePassword(string token)
        {
            var msgs = new SystemMessages();

            if (!PasswordResetToken.ExistsAndIsValidByUid(token.ToGuid()))
            {
                msgs.Add("Invalid reset token", MessageType.UserError);
            }

            ViewBag.Messages = msgs;
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string token, string password1, string password2)
        {
            var msgs = new SystemMessages();
            WebUser user = null;

            if (!PasswordResetToken.ExistsAndIsValidByUid(token.ToGuid()))
            {
                msgs.Add("Invalid reset token", MessageType.UserError);
            }
            else
            {
                if (password1.IsEmpty() || password2.IsEmpty() || (password1 != password2))
                {
                    msgs.Add("Passwords are required and must match", MessageType.UserError);
                }
                else if (password1.Length < 8)
                {
                    msgs.Add("Password must be at least 8 characters", MessageType.UserError);
                }
            }

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var prt = PasswordResetToken.LoadValidByUid(token.ToGuid(), dc: adc);
                        user = prt.GetWebUser(adc);
                        amsgs.AddRange(user.ChangePassword(password1, adc));
                        amsgs.AddRange(prt.Use(adc));
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );              
            }

            ViewBag.Messages = msgs;

            return View(user);
        }

        //public ActionResult Account(string id)
        //{
        //    string url = ControllerContext.RouteData.Values["url"].ToString();
        //    if (url != "favicon.ico")
        //    {
        //        bool redirectToLogin = true;
        //        if (SessionVariables.LoggedInUser.IsEmpty() && !url.ToString().Contains("/"))
        //        {
        //            SessionVariables.CurrentAccount = AfterSchoolLibrary.Account.LoadByUniqueName(url);
        //            if (!SessionVariables.CurrentAccount.IsEmpty())
        //            {
        //                redirectToLogin = false;
        //            }
        //        }
        //        if(redirectToLogin) 
        //        {
        //            Response.Redirect("/");
        //        }
        //    }

        //    return View();
        //}

    }
}
