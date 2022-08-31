using System;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using System.Collections.Generic;
using AfterSchoolLibrary;

namespace AfterSchool.Areas.User.Controllers
{
    public class ProfileController : BaseLoggedInController
    {
        public ProfileController() : base(new List<AccountType>() { AccountType.Registrant, AccountType.Volunteer })
        {

        }

        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentUser);
        }

        [HttpPost]
        public ActionResult Edit(string id, string firstname, string lastname, string email, string password1, string password2, string isactive)
        {
            var msgs = new SystemMessages();

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
            else if (!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
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
            if (password1.Length < 8)
            {
                msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                var user = WebUser.Load(SessionVariables.CurrentUser.ID.Value);
                user.FirstName = firstname;
                user.LastName = lastname;
                user.Email = email;
                user.LastModifiedDate = DateTime.UtcNow;
                user.LastModifiedBy = SessionVariables.CurrentUser.ID;

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
            return View(SessionVariables.CurrentUser);
        }
    }
}