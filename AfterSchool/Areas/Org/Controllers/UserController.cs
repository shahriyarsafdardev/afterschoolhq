using System;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using System.Collections.Generic;

namespace AfterSchool.Areas.Org.Controllers
{
    public class UserController : BaseLoggedInController
    {
        public UserController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        // GET: User
        [HttpGet]
        public ActionResult Index(int? start = null, int? length = null)
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount.GetMembers().Select(x=>x.ToWebUser()));
        }

        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            return View(SessionVariables.LoggedInUser);
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
            else if(!SessionVariables.LoggedInUser.CanEmailBeUsed(email))
            {
                msgs.Add("Email is already in use by another user", MessageType.UserError); 
            }
            if ((!password1.IsEmpty() || !password2.IsEmpty()) && password1 != password2)
            {
                msgs.Add("Passwords must match", MessageType.UserError);
            }
            
            if (msgs.HasNoErrors())
            {
                var user = WebUser.Load(SessionVariables.LoggedInUser.ID.Value);
                user.FirstName = firstname;
                user.LastName = lastname;
                user.Email = email;
                user.LastModifiedDate = DateTime.UtcNow;
                user.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                user.IsActive = !isactive.IsEmpty();

                if (!password1.IsEmpty())
                {
                    user.Password = WebUserService.EncryptPassword(password1);
                }

                msgs.AddRange(user.Save());

                if (msgs.HasNoErrors())
                {
                    msgs.Clear();
                    msgs.Add("Save successful", MessageType.Success);
                    SessionVariables.LoggedInUser = user;
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            return View(SessionVariables.LoggedInUser);
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

            if(usertype.IsEmpty())
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
                    msgs.AddRange(a.InviteUser("", "", email, Request.Params["message"], ut, SessionVariables.LoggedInUser));
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
    }
}