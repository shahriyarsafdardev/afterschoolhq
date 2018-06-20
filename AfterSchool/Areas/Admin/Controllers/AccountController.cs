using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;
using System.Collections;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        // GET: Account
        [HttpGet]
        public ActionResult Index(string pn, string rpp)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int count = Account.GetCount(criterias: new Hashtable() { { "IsAdminAccount", false } });
            var accounts = Account.Load(
                criterias: new Hashtable() { { "IsAdminAccount", false } },
                start: start,
                length: length);
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(accounts);
        }
        
        [HttpGet]
        public ActionResult Invite()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
            return View();
        }

        [HttpPost]
        public ActionResult Invite(string orgname, string accounttype, string package, string billingperiod, string email, string firstname, string lastname, string message)
        {
            var msgs = new SystemMessages();
            WebUser user = null;
            Account account = null;

            if (orgname.IsEmpty())
                msgs.Add("Organization Name is required", MessageType.UserError);
            if (!accounttype.IsNumber())
                msgs.Add("Organization type is required", MessageType.UserError);
            if (!package.IsNumber())
                msgs.Add("Package is required", MessageType.UserError);
            if (!billingperiod.IsNumber())
                msgs.Add("Billing Period is required", MessageType.UserError);

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
                            accountType: (AccountType)accounttype.ToInteger(), 
                            packageId: package.ToInteger(),
                            packageBillingPeriod: (BillingPeriodType)billingperiod.ToInteger(),
                            isAfterSchool: SessionVariables.IsAfterSchool.Value,
                            loggedInUser: SessionVariables.LoggedInUser, 
                            msgs: amsgs, 
                            dc: adc);
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

            ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
            return View();
        }
    }
}