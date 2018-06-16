using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;

namespace AfterSchool.Areas.User.Controllers
{
    public class ProfileController : Controller
    {
        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.LoggedInUser);
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
            else if (WebUser.IsEmailInUseByAnotherUser(email, SessionVariables.LoggedInUser.ID.Value))
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
            return View(SessionVariables.LoggedInUser);
        }
    }
}