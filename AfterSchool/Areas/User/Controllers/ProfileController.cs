using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchool.Areas.User.Controllers
{
    public class ProfileController : BaseLoggedInController
    {
        public ProfileController() : base(new List<AccountType> {AccountType.Registrant})
        {
        }

        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.IsRequireCheckInOut = IsCheckInOutFeatureEnabled();
            return View(SessionVariables.CurrentUser);
        }
        
        [HttpGet]
        public ActionResult EditCheckInOut()
        {
            var bundlePath = Server.MapPath("~/React/dist/bundle.js");
            var lastModifiedDate = System.IO.File.GetLastWriteTime(bundlePath);
            ViewBag.BundleVersion = lastModifiedDate.ToString("yyyyMMddHHmmss");
            ViewBag.IsRequireCheckInOut = IsCheckInOutFeatureEnabled();
            return View();
        }

        [HttpPost]
        public ActionResult Edit(string id, string firstname, string lastname, string email, string password1, string password2, string isactive)
        {
            var msgs = new SystemMessages();

            if (firstname.IsEmpty()) msgs.Add("First Name is required", MessageType.UserError);
            if (lastname.IsEmpty()) msgs.Add("Last Name is required", MessageType.UserError);
            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsMatch(Utilities.EmailRegEx))
                msgs.Add("Email must be valid", MessageType.UserError);
            else if (!SessionVariables.CurrentUser.CanEmailBeUsed(email, msgs)) msgs.Add("Email is already in use by another user", MessageType.UserError);
            if ((!password1.IsEmpty() || !password2.IsEmpty()) && password1 != password2) msgs.Add("Passwords must match", MessageType.UserError);
            if (password1.Length < 8) msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var user = WebUser.Load(SessionVariables.CurrentUser.ID.Value);
                user.FirstName = firstname;
                user.LastName = lastname;
                user.Email = email;
                user.LastModifiedDate = DateTime.UtcNow;
                user.LastModifiedBy = SessionVariables.CurrentUser.ID;

                if (!password1.IsEmpty()) user.Password = WebUserService.EncryptPassword(password1);

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

        private static bool IsCheckInOutFeatureEnabled()
        {
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var masterRegistrants = currentUser.GetStudentRegistrants(msgs);

            foreach (var masterRegistrant in masterRegistrants)
            {
                var registrants = masterRegistrant.GetSubmittedRecords(msgs);
                foreach (var registrant in registrants)
                {
                    var account = registrant.GetAccount(msgs);
                    var registrantActivity = registrant.GetRegistrantActivities(msgs)
                        .FirstOrDefault(ra => ra.ActivityGroupId.IsEmpty());
                    var activity = registrantActivity?.GetActivity(msgs);

                    if (account.IsNotEmpty() && activity.IsNotEmpty() && account.HasAccountPackageFeature(FeatureType.ManageCheckInCheckOut)  &&
                        activity.IsRequireCheckInCheckOut.IsNotEmpty() && activity.IsRequireCheckInCheckOut)
                        return true;
                }
            }
            return false;
        }
    }
}