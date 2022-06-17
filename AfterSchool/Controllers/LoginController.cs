using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Controllers
{
    public class LoginController : BaseController
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public LoginController(IFeatureFlagRepository featureFlagRepository)
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        [HttpGet]
        public ActionResult Index(string regaccount, string invite)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var loggedInWithAnAccount = currentUser.IsNotEmpty() && currentAccount.IsNotEmpty();

            HandleTempMessages(msgs);

            if (loggedInWithAnAccount && (regaccount.IsEmpty() || currentAccount.AccountTypeId == AccountType.Registrant))
            {
                if (!currentUser.IsRegistered)
                    return Redirect(Utilities.CompleteSignUpUrl(currentUser.Uid, Request["returnurl"]));

                if (invite.IsGuid())
                {
                    if (currentUser.HasMultipleAccounts())
                        return RedirectToAction("select", "login", new {returnurl = Request["returnurl"], invite});

                    if (currentAccount.AccountTypeId == AccountType.Registrant)
                        return RedirectToAction("welcome", "signup", new {returnurl = Request["returnurl"], invite});
                }

                HandleInvite(invite, currentAccount, currentUser, msgs);

                if (Request["returnurl"].IsNotEmpty())
                    return HandleRedirectAfterLogin(Request["returnurl"], msgs);

                return LoginRedirect(msgs: msgs);
            }

            if (!Request["Account"].IsEmpty())
            {
                var account = Account.Load(Request["Account"].ToInteger());

                if (!account.IsEmpty() && !account.UniqueName.IsEmpty())
                    return Redirect($"/{account.UniqueName}");
            }

            if (invite.IsGuid())
                msgs.Add("Login to accept your invite", MessageType.Notice);

            ViewBag.LoginType = "Internal";
            ViewBag.Messages = msgs;
            ViewBag.RegistrationAccount = regaccount;
            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");
            return View();
        }

        [HttpPost]
        public ActionResult Index(string username, string password, string remember, string returnurl, string invite)
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

            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");
            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                var currentAccount = SessionVariables.CurrentAccount;

                // select account
                if (currentAccount.IsEmpty())
                    return RedirectToAction("select", "login", new {returnurl, invite});

                HandleInvite(invite, currentAccount, user, msgs);

                if (user.IsNotEmpty() && currentAccount.IsNotEmpty())
                    // LOG: WebUserName LoggedIn
                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: user,
                        actionType: WebSessionActionType.LoggedIn
                    );

                if (returnurl.IsNotEmpty())
                    return HandleRedirectAfterLogin(returnurl, msgs);

                return LoginRedirect(msgs: msgs);
            }

            return View();
        }

        [HttpGet]
        public ActionResult LoginUser(string id)
        {
            var msgs = new SystemMessages();

            if (id.IsEmpty())
            {
                msgs.Add("Identifier expected", MessageType.UserError);
                LoadTempMsgAndLogErrors(msgs);
                return RedirectToAction("Index");
            }

            var user = WebUser.Load(id.ToInteger(), msgs);

            if (user.IsEmpty())
            {
                msgs.Add("Invalid username and/or password", MessageType.UserError);
                LoadTempMsgAndLogErrors(msgs);
                return RedirectToAction("Index");
            }
            else
                LoadSessionInfo(user, null, true, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                var currentAccount = SessionVariables.CurrentAccount;

                // select account
                if (currentAccount.IsEmpty())
                    return RedirectToAction("select", "login");

                if (user.IsNotEmpty() && currentAccount.IsNotEmpty())
                    // LOG: WebUserName LoggedIn
                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: user,
                        actionType: WebSessionActionType.LoggedIn
                    );

                return LoginRedirect(msgs: msgs);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Select(string id, string returnurl, string invite)
        {
            var msgs = new SystemMessages();
            Account account = null;

            if (id.IsGuid())
                account = Account.LoadByUid(id.ToGuid(), msgs);

            if (account.IsNotEmpty())
            {
                var remember = CookieVariables.CurrentUser.IsGuid();
                var currentUser = SessionVariables.CurrentUser;

                if (account.AccountTypeId == AccountType.AfterSchool)
                    remember = false;

                ClearCookies();
                LoadSessionInfo(SessionVariables.CurrentUser, account, remember);

                HandleInvite(invite, account, currentUser, msgs);

                if (returnurl.IsNotEmpty())
                    return HandleRedirectAfterLogin(returnurl, msgs);

                return LoginRedirect(msgs: msgs);
            }

            if (SessionVariables.CurrentUser.IsEmpty())
                return RedirectToAction("Index");

            var accounts = SessionVariables.CurrentUser.GetAccounts(msgs);
            //We should not show deactivated or tests accounts.
            accounts = accounts.Where(x => x.IsActive == true && x.IsTest == false).ToList();

            if (invite.IsGuid())
                accounts = accounts.Where(a => a.AccountTypeId.In(FormConstants.ProviderAccountTypes)).ToList();

            ViewBag.Invite = invite;
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

        [HttpPost]
        public JsonResult ResetPasswordJson(string email)
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

            if (msgs.HasNoErrors())
                return Json(new {success = true, status = "ok"}, JsonRequestBehavior.AllowGet);

            //.. something went wrong, error occured
            return Json(new {success = false, status = "error", data = msgs.ToList().Select(msg => new {msg.Text})}, JsonRequestBehavior.AllowGet);
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
                    onSuccess: (SystemMessages smsgs) => { msgs.AddRange(smsgs); },
                    onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); }
                );
            }

            ViewBag.Messages = msgs;

            return View(user);
        }

        [HttpPost]
        public ActionResult Authorize(string username, string password, string remember, string source)
        {
            if (!IsRequestFromApp())
                return Json(new {status = "error", messages = new SystemMessages()}, JsonRequestBehavior.AllowGet);

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
                return Json(new {status = "ok", isNewParentAccount}, JsonRequestBehavior.DenyGet);

            return Json(new {status = "error", messages = msgs}, JsonRequestBehavior.DenyGet);
        }

        [HttpGet]
        public ActionResult AuthorizeUser(string id, string source, string redirectUrl)
        {
            var msgs = new SystemMessages();

            if (id.IsEmpty())
            {
                msgs.Add("Identifier expected", MessageType.UserError);
                LoadTempMsgAndLogErrors(msgs);
                return RedirectToAction("Index");
            }

            var user = WebUser.Load(id.ToInteger(), msgs);

            if (msgs.HasNoErrors() && user.IsNotEmpty())
            {
                LoadSessionInfo(user, null, true, source, msgs);
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index");
        }
    }
}