using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using System.Collections;
using BasicLibrary.DataAccess;
using System.Web.WebPages;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(new List<AccountType> { AccountType.AfterSchool })
        {

        }

        private List<Account> IndexViewBag(string pn, string rpp, string search, AccountType type, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var accountTypes = AccountService.CustomerAccountTypes.ToList();

            if (type != AccountType.NotSet)
                accountTypes = type.AsList();

            var count = Account.GetSearchCount(search, accountTypes, false, msgs);
            var accounts = Account.Search(search, accountTypes, isDeleted: false, OrderByProperty: "CreateDate", orderByDirection: OrderByDirectionEnum.Descending, start, length, msgs);
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return accounts;
        }

        // GET: Account
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string search, AccountType type = AccountType.NotSet)
        {
            return View(IndexViewBag(pn, rpp, search, type));
        }

        [HttpGet]
        public ActionResult ResendInvite(string id, string pn, string rpp, string search, AccountType type = AccountType.NotSet)
        {
            var msgs = new SystemMessages();
            var account = id.IsGuid() ? Account.LoadByUid(id.ToGuid(), msgs) : null;

            if (account.IsNotEmpty())
            {
                var user = account.GetUsers(msgs: msgs).FirstOrDefault();
                if (user.IsNotEmpty())
                    msgs.AddRange(account.SendInvitedEmail(user, SessionVariables.CurrentUser));
            }

            return View("index", IndexViewBag(pn, rpp, search, type, msgs));
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
        public ActionResult Invite(string id, string orgname, string accounttype, string package, string billingperiod, string email, string claimemaildomain, string firstname, string lastname, string message, string isUnclaimed, string timezone, string istest)
        {
            var msgs = new SystemMessages();
            var sendInvite = !isUnclaimed.IsNumber();

            WebUser user = null;
            Account account = null;
            WebUser currentUser = SessionVariables.CurrentUser;

            if (!id.IsGuid())
            {
                if (orgname.IsBlank())
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

            if (sendInvite)
            {
                if (!email.IsEmail())
                    msgs.Add("Email is required and must be valid", MessageType.UserError);
                if (firstname.IsBlank())
                    msgs.Add("First Name is required", MessageType.UserError);
                if (lastname.IsBlank())
                    msgs.Add("Last Name is required", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (account.IsNotEmpty())
                        {
                            account.AddPackage(
                                package.ToInteger(),
                                (BillingPeriodType)billingperiod.ToInteger(),
                                currentUser,
                                amsgs,
                                adc
                            );

                            if (sendInvite)
                            {
                                account.IsClaimed = true;
                                account.ClaimEmailDomain = claimemaildomain;
                                amsgs.AddRange(account.Save(adc));

                                user = account.AddUser(
                                    firstname,
                                    lastname,
                                    email,
                                    UserType.Owner,
                                    currentUser,
                                    amsgs,
                                    adc);
                            }
                        }
                        else
                        {
                            if (sendInvite)
                            {
                                user = WebUserService.InviteNew(
                                        firstName: firstname,
                                        lastName: lastname,
                                        email: email,
                                        loggedInUser: currentUser,
                                        msgs: amsgs,
                                        dc: adc);

                                account = user.AddNewAccount(
                                            orgName: orgname,
                                            claimemaildomain: claimemaildomain,
                                            phone: null,
                                            isActive: true,
                                            accountType: (AccountType)accounttype.ToInteger(),
                                            packageId: package.ToInteger(),
                                            packageBillingPeriod: (BillingPeriodType)billingperiod.ToInteger(),
                                            timeZone: timezone,
                                            isAfterSchool: true,
                                            loggedInUser: currentUser,
                                            msgs: amsgs,
                                            dc: adc);
                            }
                            else
                            {
                                account = AccountService.CreateNewUnclaimedAccount(
                                            orgName: orgname,
                                            email: null,
                                            claimemaildomain: claimemaildomain,
                                            phone: null,
                                            isActive: true,
                                            accountType: (AccountType)accounttype.ToInteger(),
                                            accountOwner: currentUser,
                                            packageId: package.ToInteger(),
                                            packageBillingPeriod: (BillingPeriodType)billingperiod.ToInteger(),
                                            timeZone: timezone,
                                            loggedInUser: currentUser,
                                            isAfterSchool: true,
                                            createWithStripe: false,
                                            msgs: amsgs,
                                            dc: adc);
                            }
                        }

                        account.IsTest = istest.IsNumber();
                        amsgs.AddRange(account.Save(adc));
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);

                        if (sendInvite)
                            msgs.AddRange(account.SendInvitedEmail(user, SessionVariables.CurrentUser, message));
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                return View("InviteSent", user);

            ViewBag.Account = account;
            ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
            return View();
        }

        public ActionResult LoginTo(string id)
        {
            var msgs = new SystemMessages();

            if (id.IsGuid())
            {
                var account = Account.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessTo(account, msgs))
                    account.AddGhostUser(SessionVariables.CurrentUser, UserType.TechSupport, SessionVariables.CurrentUser, msgs);

                SessionVariables.OriginalLoggedInUser = SessionVariables.CurrentUser;
                LoadSessionInfo(SessionVariables.CurrentUser, account, false, msgs);

                return LoginRedirect();
            }

            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult UpdateAccountStatus(string id, string pn, string rpp, string search, AccountType type = AccountType.NotSet)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var account = id.IsGuid() ? Account.LoadByUid(id.ToGuid(), msgs) : null;

            if (account.IsEmpty())
                msgs.Add("Invalid Account.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                AccountService.SetIsActive(account, !account.IsActive, currentUser, msgs);
                msgs.Add($"Account {(account.IsActive ? "activated" : "deactivated")} successfully.", MessageType.Success);
            }

            SetTempMessage(msgs);
            return View("index", IndexViewBag(pn, rpp, search, type, msgs));
        }

        [HttpGet]
        public ActionResult UpdateAccountClaims(string id, string pn, string rpp, string search, AccountType type = AccountType.NotSet)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var account = id.IsGuid() ? Account.LoadByUid(id.ToGuid(), msgs) : null;

            if (account.IsEmpty())
                msgs.Add("Invalid Account.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                AccountService.SetIsClaimed(account, !account.IsClaimed, currentUser, msgs);
                msgs.Add("Account claims changed successfully.", MessageType.Success);
            }

            SetTempMessage(msgs);
            return View("index", IndexViewBag(pn, rpp, search, type, msgs));
        }

        [HttpGet]
        public ActionResult DeleteAccount(string id, string pn, string rpp, string search, AccountType type = AccountType.NotSet)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var account = id.IsGuid() ? Account.LoadByUid(id.ToGuid(), msgs) : null;

            if (account.IsEmpty())
                msgs.Add("Invalid Account.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                AccountService.MarkAsDeleted(account, currentUser, msgs);
                msgs.Add("Account removed successfully.", MessageType.Success);
            }

            SetTempMessage(msgs);
            return View("index", IndexViewBag(pn, rpp, search, type, msgs));
        }

        [HttpGet]
        public ActionResult UpdateAccountTestFlagStatus(string id, string pn, string rpp, string search, AccountType type = AccountType.NotSet)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var account = id.IsGuid() ? Account.LoadByUid(id.ToGuid(), msgs) : null;

            if (account.IsEmpty())
                msgs.Add("Invalid Account.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                AccountService.SetIsTest(account, !account.IsTest, currentUser, msgs);
                msgs.Add($"Account is {(account.IsTest ? "in" : "not in")} test mode  {(account.IsTest ? "now" : "anymore")}.", MessageType.Success);
            }

            SetTempMessage(msgs);
            return View("index", IndexViewBag(pn, rpp, search, type, msgs));
        }
    }
}