using AfterSchool.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.District.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/Account
        [HttpGet]
        public ActionResult Team()
        {
            var members = SessionVariables.CurrentAccount.GetMembers().Select(x => x.ToWebUser()).ToList();
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts).GetMaxValue();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult TeamInvite()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult TeamInvite(string emails, string message)
        {
            var msgs = new SystemMessages();

            if (emails.IsEmpty())
                msgs.Add("Please enter at least one email address", MessageType.UserError);
            else
                emails.Split(',')
                    .Select(x => x.Trim())
                    .Where(email => !email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    .ToList()
                    .ForEach(email => msgs.Add(string.Format("{0} is not a valid email address", email), MessageType.UserError));

            if (msgs.HasNoErrors())
                emails.Split(',')
                    .Select(x => x.Trim())
                    .ToList()
                    .ForEach(email =>
                        SessionVariables.CurrentAccount.InviteUser(
                            "",
                            "",
                            email,
                            "",
                            Request.Params["message"],
                            UserType.Administrator,
                            SessionVariables.CurrentUser,
                            msgs)
                     );

            if (msgs.HasNoErrors())
                msgs.Add("All invites have been sent", MessageType.Success);

            var members = SessionVariables.CurrentAccount.GetMembers().Select(x => x.ToWebUser()).ToList();
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts).GetMaxValue();
            return View("Team", SessionVariables.CurrentAccount);
        }
    }
}