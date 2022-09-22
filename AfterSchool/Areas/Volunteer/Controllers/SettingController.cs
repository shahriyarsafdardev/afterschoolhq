using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchool.Areas.Volunteer.Controllers
{
    public class SettingController : BaseLoggedInController
    {
        public SettingController() : base(new List<AccountType> {AccountType.Volunteer})
        {
        }

        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentUser);
        }

        [HttpPost]
        public ActionResult Edit(string password1, string password2, string email, string phone)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;

            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsMatch(Utilities.EmailRegEx))
                msgs.Add("Email must be valid", MessageType.UserError);
            else if (!currentUser.CanEmailBeUsed(email, msgs))
                msgs.Add("Email is already in use by another user", MessageType.UserError);

            if (phone.IsPhone())
                msgs.Add("Phone Number is incorrect.", MessageType.UserError);
            if (!currentUser.CanPhoneBeUsed(phone, msgs))
                msgs.Add("Phone Number is already in use.", MessageType.UserError);

            if (!password1.IsEmpty() || !password2.IsEmpty())
            {
                if (password1.Length < 8)
                    msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
                if (password1 != password2)
                    msgs.Add("Passwords must match", MessageType.UserError);
            }


            if (msgs.HasNoErrors())
            {
                var user = WebUser.Load(currentUser.ID.Value);
                user.Email = email;
                user.Phone = phone;
                user.LastModifiedDate = DateTime.UtcNow;
                user.LastModifiedBy = currentUser.ID;

                if (!password1.IsEmpty())
                    user.Password = WebUserService.EncryptPassword(password1);

                msgs.AddRange(user.Save());

                if (msgs.HasNoErrors())
                {
                    msgs.Clear();
                    msgs.Add("Login Credentials updated successfully", MessageType.Success);
                    SessionVariables.CurrentUser = user;
                }
            }

            ViewBag.Messages = msgs;
            return View(currentUser);
        }
    }
}