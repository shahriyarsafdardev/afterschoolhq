using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.District.Controllers
{
    public class SchoolController : BaseLoggedInController
    {
        public SchoolController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/School
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

            int count = SessionVariables.CurrentAccount.GetVisitorAccountsCount(AccountType.School.AsList());
            var visitorAccounts = SessionVariables.CurrentAccount.GetVisitorAccounts(AccountType.School.AsList(), start, length);
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(visitorAccounts);
        }

        [HttpGet]
        public ActionResult LoginTo(string id)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsGuid())
            {
                var account = Account.LoadByUid(id.ToGuid(), msgs);
                var visitorAccountIds = currentAccount.GetVisitorAccounts(AccountType.School.AsList()).Select(x => x.ID.Value);

                if (account.ID.Value.In(visitorAccountIds))
                {
                    if (!currentUser.HasAccessTo(account, msgs))
                        account.AddUser(currentUser, UserType.Administrator, currentUser, msgs);

                    LoadSessionInfo(currentUser, account, false, msgs);

                    return LoginRedirect();
                }
            }

            return RedirectToAction("index");
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
                        user = WebUserService.InviteNew(
                            firstName: firstname,
                            lastName: lastname,
                            email: email,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        account = user.AddNewAccount(
                            orgName: orgname,
                            phone: null,
                            isActive: true,
                            accountType: AccountType.School,
                            packageId: Package.Load().First().ID.Value,
                            packageBillingPeriod: BillingPeriodType.Month,
                            timeZone: null,
                            isAfterSchool: SessionVariables.IsAfterSchool.Value,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        // Adding the school to the district account so district can see details
                        msgs.AddRange(
                            SessionVariables.CurrentAccount.AddVisitorAccount(
                                account, 
                                HostPermissionType.ApproveOnly,
                                ApprovalStatus.Approved, 
                                true,
                                false,
                                SessionVariables.CurrentUser, 
                                adc
                            )
                        );
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.AddRange(account.SendInvitedEmail(user, SessionVariables.CurrentUser, message));
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