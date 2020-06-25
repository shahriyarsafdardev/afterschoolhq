using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.District.Controllers
{
    public class UserController : BaseLoggedInController
    {
        public UserController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: User
        [HttpGet]
        public ActionResult Index(string pn, string rpp)
        {
            var pageNumber = 1;
            var recordsPerPage = 10;
            var msgs = new SystemMessages();

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int userCount = SessionVariables.CurrentAccount.GetMemberCount(msgs);
            var users = SessionVariables.CurrentAccount.GetUsers(start, length, msgs);
            var totalPages = Math.Ceiling(userCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = userCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(users);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.ReturnUrl = Url.Action("index", "user");

            if (Request["returnurl"].IsNotEmpty())
                ViewBag.ReturnUrl = Request["returnurl"];

            if (id.IsEmpty())
                return View(SessionVariables.CurrentUser);

            return View(WebUser.Load(id.ToInteger()));
        }

        [HttpPost]
        public ActionResult Edit(string id, string firstname, string lastname, string username, string password1, string password2, string isactive)
        {
            var msgs = new SystemMessages();
            var user = WebUser.Load(id.ToInteger());

            if (password1 != password2)
            {
                msgs.Add("Password do not match", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                user.FirstName = firstname;
                user.LastName = lastname;

                if (!user.ID.HasValue)
                {
                    user.CreateDate = DateTime.UtcNow;
                    user.CreatedBy = SessionVariables.CurrentUser.ID;
                }
                user.LastModifiedDate = DateTime.UtcNow;
                user.LastModifiedBy = SessionVariables.CurrentUser.ID;

                if (password1.IsNotEmpty() && password2.IsNotEmpty())
                {
                    user.Password = WebUserService.EncryptPassword(password1);
                }

                user.IsActive = !isactive.IsEmpty();
                msgs.AddRange(user.Save());

                if (msgs.HasNoErrors())
                {
                    msgs.Add("Save successful", MessageType.Success);
                }
            }

            ViewBag.Messages = msgs;
            return View(user);
        }

        [HttpGet]
        public ActionResult Invite(string id, string returnUrl)
        {
            ViewBag.AccountId = id;
            ViewBag.Messages = new SystemMessages();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Invite(string id, string emails, string message)
        {
            SystemMessages msgs = new SystemMessages();

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
                Account a = null;
                UserType ut = UserType.NotSet;

                if (!id.IsNumber())
                {
                    a = SessionVariables.CurrentAccount;
                    ut = UserType.SuperAdmin;
                }
                else
                {
                    a = Account.Load(id.ToInteger());
                    ut = UserType.Administrator;
                }

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
                    Response.Redirect(Request["returnurl"], true);
                }
            }

            ViewBag.Messages = msgs;
            return View();
        }


        [HttpGet]
        public ActionResult Impersonate(string id, string uid)
        {
            var msgs = new SystemMessages();
            WebUser user = null;

            if (uid.IsEmpty() && id.IsNumber())
                user = Account.Load(id.ToInteger()).GetAccountOwner();
            else if (uid.IsGuid())
                user = WebUser.LoadByUid(uid.ToGuid());
            else
                msgs.Add("Invalid params", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                SessionVariables.OriginalLoggedInUser = SessionVariables.CurrentUser;
                LoadSessionInfo(user, null, false, msgs);
            }

            if (msgs.HasNoErrors())
                return LoginRedirect();

            SessionVariables.OriginalLoggedInUser = null;
            ViewBag.Messages = msgs;

            if (id.IsNotEmpty())
                return RedirectToAction("Team", "Account", new { id = id });

            return RedirectToAction("Index", "User");
        }

    }
}