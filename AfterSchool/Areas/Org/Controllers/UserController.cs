using System;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.FeatureFlag;

namespace AfterSchool.Areas.Org.Controllers
{
    public class UserController : BaseLoggedInController
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public UserController(IFeatureFlagRepository featureFlagRepository) : base(AccountService.ProviderAccountTypes.ToList())
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        // GET: User
        [HttpGet]
        public ActionResult Index(int? start = null, int? length = null)
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount.GetUsers());
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var msgs = new SystemMessages();

            HandleTempMessages(msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");

            return View(SessionVariables.CurrentUser);
        }

        [HttpPost]
        public ActionResult Edit(string id, string firstname, string lastname, string email, string password1, string password2, string isactive)
        {
            SystemMessages msgs = new SystemMessages();

            if (firstname.IsEmpty())
            {
                msgs.Add("First Name is required", MessageType.UserError);
            }
            if (lastname.IsEmpty())
            {
                msgs.Add("Last Name is required", MessageType.UserError);
            }
            if (email.IsEmpty())
            {
                msgs.Add("Email is required", MessageType.UserError);
            }
            else if (!email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
            {
                msgs.Add("Email must be valid", MessageType.UserError);
            }
            else if (!SessionVariables.CurrentUser.CanEmailBeUsed(email, msgs))
            {
                msgs.Add("Email is already in use by another user", MessageType.UserError);
            }
            if ((!password1.IsEmpty() || !password2.IsEmpty()) && password1 != password2)
            {
                msgs.Add("Passwords must match", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                var user = WebUser.Load(SessionVariables.CurrentUser.ID.Value);
                user.FirstName = firstname;
                user.LastName = lastname;
                user.Email = email;
                user.LastModifiedDate = DateTime.UtcNow;
                user.LastModifiedBy = SessionVariables.CurrentUser.ID;
                // user.IsActive = !isactive.IsEmpty();

                if (!password1.IsEmpty())
                {
                    user.Password = WebUserService.EncryptPassword(password1);
                }

                msgs.AddRange(user.Save());

                if (msgs.HasNoErrors())
                {
                    msgs.Clear();
                    msgs.Add("Save successful", MessageType.Success);
                    SessionVariables.CurrentUser = user;
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");
            return View(SessionVariables.CurrentUser);
        }

        [HttpGet]
        public ActionResult Invite(string returnUrl)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Invite(string usertype, string emails, string message)
        {
            SystemMessages msgs = new SystemMessages();

            if (usertype.IsEmpty())
            {
                msgs.Add("Account type is required", MessageType.UserError);
            }

            if (emails.IsEmpty())
            {
                msgs.Add("Please enter at least one email address", MessageType.UserError);
            }
            else
            {
                foreach (string email in emails.Split(',').Select(x => x.Trim()))
                {
                    if (!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    {
                        msgs.Add(string.Format("{0} is not a valid email address", email), MessageType.UserError);
                    }
                }
            }

            if (msgs.HasNoErrors())
            {
                var a = SessionVariables.CurrentAccount;
                var ut = (UserType)usertype.ToInteger();

                foreach (string email in emails.Split(',').Select(x => x.Trim()))
                {
                    a.InviteUser("", "", email, null, Request.Params["message"], ut, SessionVariables.CurrentUser, msgs);
                }
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add("All invites have been sent", MessageType.Success);

                if (Request["returnurl"].IsNotBlank())
                {
                    return Redirect(Request["returnurl"]);
                }
            }

            ViewBag.Messages = msgs;
            return View();
        }

        [HttpGet]
        public ActionResult DisconnectCleverForCurrentUser()
        {
            var msgs = new SystemMessages();

            if (SessionVariables.CurrentAccount.IsEmpty())
                msgs.Add("Account not found.", MessageType.UserError);

            if (SessionVariables.CurrentUser.IsEmpty())
                msgs.Add("User is not logged In.", MessageType.UserError);            

            if (msgs.HasNoErrors())
            {
                if (SessionVariables.CurrentUser.CleverUserId.IsEmpty())
                    msgs.Add($"Clever is already disconnected for {SessionVariables.CurrentUser.DisplayName}", MessageType.UserError);
                else
                    WebUserService.DisconnectCleverIdentity(SessionVariables.CurrentUser, msgs);               
            }

            SetTempMessage(msgs);
            return RedirectToAction("edit");
        }
    }
}