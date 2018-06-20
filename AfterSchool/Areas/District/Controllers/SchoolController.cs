using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.District.Controllers
{
    public class SchoolController : BaseLoggedInController
    {
        // GET: District/School
        [HttpGet]
        public ActionResult Index()
        {
            return View(SessionVariables.CurrentAccount.GetVisitorAccounts());
        }

        [HttpGet]
        public ActionResult Invite()
        {
            ViewBag.Messages = new SystemMessages();
            return View();
        }

        [HttpPost]
        public ActionResult Invite(string orgname, string firstname, string lastname, string email, string message)
        {
            var msgs = new SystemMessages();
            WebUser user = null;
            Account account = null;

            if (orgname.IsEmpty())
                msgs.Add("School Name is required", MessageType.UserError);

            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Email must be valid", MessageType.UserError);

            if (firstname.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);
            if (lastname.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        user = WebUserService.CreateNew(
                            firstName: firstname,
                            lastName: lastname,
                            email: email,
                            password: null,
                            isActive: false,
                            isRegistered: false,
                            isVerified: false,
                            loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                            msgs: amsgs,
                            dc: adc);

                        account = user.AddNewAccount(
                            orgName: orgname,
                            phone: null,
                            isActive: true,
                            accountType: AccountType.School,
                            packageId: null,
                            packageBillingPeriod: null,
                            isAfterSchool: SessionVariables.IsAfterSchool.Value,
                            loggedInUser: SessionVariables.LoggedInUser,
                            msgs: amsgs,
                            dc: adc);

                        // Adding the school to the district account so district can see details
                        msgs.AddRange(
                            SessionVariables.CurrentAccount.AddVisitorAccount(
                                account, 
                                HostPermissionType.Edit,
                                ApprovalStatus.Approved, 
                                SessionVariables.LoggedInUser, 
                                adc
                            )
                        );
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.AddRange(account.SendInvitedEmail(user, SessionVariables.LoggedInUser, message));
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                return View("InviteSent", user);
            
            return View();
        }
    }
}