using System;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
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
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var loggedInWithAnAccount = currentUser.IsNotEmpty() && currentAccount.IsNotEmpty();

            if (loggedInWithAnAccount && (regaccount.IsEmpty() || currentAccount.AccountTypeId == AccountType.Registrant))
            {
                if (!currentUser.IsRegistered)
                    return Redirect(AfterSchoolLibrary.Helpers.Utilities.CompleteSignUpUrl(currentUser.Uid, Request["returnurl"]));

                if (Request["returnurl"].IsNotEmpty())
                    return HandleRedirectAfterLogin(Request["returnurl"]);

                return LoginRedirect();
            }

            if (!Request["Account"].IsEmpty())
            {
                var account = Account.Load(Request["Account"].ToInteger());

                if (!account.IsEmpty() && !account.UniqueName.IsEmpty())
                    return Redirect($"/{account.UniqueName}");
            }

            ViewBag.LoginType = "Internal";
            ViewBag.Messages = msgs;
            ViewBag.RegistrationAccount = regaccount;
            return View();
        }

        [HttpPost]
        public ActionResult Index(string username, string password, string remember, string returnurl)
        {
            var msgs = new SystemMessages();

            if (username.IsEmpty())
                msgs.Add("Username is required", MessageType.UserError);

            if (password.IsEmpty())
                msgs.Add("Password is required", MessageType.UserError);

            WebUser user = null;
            
            if (msgs.HasNoErrors())
            {
                if (username.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    user = WebUserService.LoginByEmail(username, password);
                else if (username.IsMatch(BasicLibrary.Utilities.PhoneRegEx))
                    user = WebUserService.LoginByPhone(username, password);
                else
                    user = WebUserService.LoginByUsername(username, password);

                if (user.IsEmpty())
                    msgs.Add("Invalid username and/or password", MessageType.UserError);
                else
                    LoadSessionInfo(user, null, remember.IsBoolean(), msgs);
            }


            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                var currentAccount = SessionVariables.CurrentAccount;

                // select account
                if (currentAccount.IsEmpty())
                    return RedirectToAction("select", "login", new {  returnurl });
                
                if (user.IsNotEmpty() && currentAccount.IsNotEmpty())
                    // LOG: WebUserName LoggedIn
                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: user,
                        actionType: WebSessionActionType.LoggedIn
                    );

                if (returnurl.IsNotEmpty())
                    return HandleRedirectAfterLogin(returnurl);

                return LoginRedirect();
            }

            return View();
        }

        [HttpGet]
        public ActionResult Select(string id, string returnurl)
        {
            var msgs = new SystemMessages();
            Account account = null;

            if (id.IsGuid())
                account = Account.LoadByUid(id.ToGuid(), msgs);

            if (account.IsNotEmpty())
            {
                var remember = CookieVariables.CurrentUser.IsGuid();

                if (account.AccountTypeId == AccountType.AfterSchool)
                    remember = false;

                ClearCookies();
                LoadSessionInfo(SessionVariables.CurrentUser, account, remember);

                if (returnurl.IsNotEmpty())
                    return Redirect(returnurl);

                return LoginRedirect();
            }

            if (SessionVariables.CurrentUser.IsEmpty())
                return RedirectToAction("Index");

            var accounts = SessionVariables.CurrentUser.GetAccounts(msgs);

            ViewBag.ReturnUrl = returnurl;
            ViewBag.Messages = msgs;
            return View(accounts);
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
                msgs.Add("Please enter a valid email address", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                user = WebUser.LoadByEmail(email, msgs);

                if (user.IsNotEmpty())
                    msgs.AddRange(user.SendPasswordResetToken(SessionVariables.IsAfterSchool.Value));
                
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
                msgs.Add("Invalid reset token", MessageType.UserError);

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

        [HttpPost]
        public ActionResult Authorize(string username, string password, string remember, string source)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var isNewParentAccount = false;

            if (username.IsEmpty())
                msgs.Add("Username is required", MessageType.UserError);

            if (password.IsEmpty())
                msgs.Add("Password is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                WebUser user = null;

                if (username.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    user = WebUserService.LoginByEmail(username, password);
                else if (username.IsMatch(BasicLibrary.Utilities.PhoneRegEx))
                    user = WebUserService.LoginByPhone(username, password);
                else
                    user = WebUserService.LoginByUsername(username, password);

                if (user.IsEmpty())
                    msgs.Add("Invalid username and/or password", MessageType.UserError);
                else
                    LoadSessionInfo(user, null, remember.ToBoolean(), source, msgs);
            }

            if (msgs.HasNoErrors())
                return Json(new { status = "ok", isNewParentAccount }, JsonRequestBehavior.DenyGet);

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.DenyGet);
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
