using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using System.Collections;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(new List<AccountType> { AccountType.AfterSchool })
        {

        }

        // GET: Account
        [HttpGet]
        public ActionResult Index(string pn, string rpp)
        {
            var msgs = new SystemMessages();
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var accountTypes = new List<AccountType>()
            {
                AccountType.ActivityProvider,
                AccountType.Registrant,
                AccountType.School,
                AccountType.SchoolDistrict
            };

            var count = Account.GetCount(accountTypes, msgs);
            var accounts = Account.LoadByAccountTypeIds(accountTypes, start, length, msgs);
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = msgs;
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
        public ActionResult Invite(string id)
        {
            var msgs = new SystemMessages();

            ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
            ViewBag.Account = id.IsGuid() ? Account.LoadByUid(id.ToGuid(), msgs) : null;
            ViewBag.Messages = msgs;

            return View();
        }

        [HttpPost]
        public ActionResult Invite(string id, string orgname, string accounttype, string package, string billingperiod, string email, string firstname, string lastname, string message)
        {
            var msgs = new SystemMessages();
            WebUser user = null;
            Account account = null;

            if (!id.IsGuid())
            {
                if (orgname.IsEmpty())
                    msgs.Add("Organization Name is required", MessageType.UserError);
                if (!accounttype.IsNumber())
                    msgs.Add("Organization type is required", MessageType.UserError);
            }
            else
                account = Account.LoadByUid(id.ToGuid());

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
                        if (account.IsNotEmpty())
                        {
                            account.AddPackage(package.ToInteger(), (BillingPeriodType)billingperiod.ToInteger(), SessionVariables.LoggedInUser, amsgs, adc);
                            user = account.AddUser(firstname, lastname, email, UserType.Owner, SessionVariables.LoggedInUser.ID.Value, amsgs, adc);
                        }
                        else
                        {
                            user = WebUserService.InviteNew(
                                firstName: firstname,
                                lastName: lastname,
                                email: email,
                                loggedInUser: SessionVariables.LoggedInUser,
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
                        }
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

            ViewBag.Account = account;
            ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
            return View();
        }
    }
}