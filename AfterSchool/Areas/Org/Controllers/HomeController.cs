using System.Collections.Generic;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System;

namespace AfterSchool.Areas.Org.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        // GET: Org/Home
        [HttpGet]
        public ActionResult Index()
        {
            if (!SessionVariables.CurrentAccount.CanReceivePaymentDeposits() && SessionVariables.AccountUserType == UserType.Treasurer)
                return RedirectToAction("getstartedpayments", "account");

            if (SessionVariables.CurrentAccount.IsNewProviderForOtherAccounts())
                return RedirectToAction("getstartedpayments", "account");

            if (SessionVariables.CurrentAccount.GetActivityCount(SessionVariables.AccountUserType, SessionVariables.CurrentUser, null, null, null, null) == 0)
                return RedirectToAction("getstarted");

            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var actions = SessionAction.LoadByAccountId(
                account.ID.Value, 
                new List<SessionActionType>() {
                    SessionActionType.RegistrationCompletedView,
                    SessionActionType.RegistrationUpdated,
                    SessionActionType.ActivityCreate,
                    SessionActionType.ActivityPublish,
                    SessionActionType.ActivityUnpublish,
                    SessionActionType.RegistrationAddActivity,
                    SessionActionType.RegistrationRemoveActivity,
                    SessionActionType.ActivityMoveToApplication,
                    SessionActionType.ActivityMoveToReserved,
                    SessionActionType.ActivityMoveToWaitingList,
                }, 
                0, 
                10,
                msgs);       
            
            var activityCount = account.GetActivityCount(
                SessionVariables.AccountUserType, 
                SessionVariables.CurrentUser, 
                ActivityType.Activity, 
                null, 
                false,
                null,
                msgs);

            ViewBag.Messages = msgs;
            ViewBag.Actions = actions;
            ViewBag.HasVisitorAccountsToApprove = SessionVariables.CurrentAccount.GetRequestedVisitorAccountsCount(msgs) > 0;
            ViewBag.HasOutstandingLeagueInvites = SessionVariables.CurrentAccount.GetExternalLeagueInviteCount(msgs) > 0;
            ViewBag.TotalPaid = SessionVariables.CurrentAccount.GetTotalAmountBilledOut(msgs: msgs);
            ViewBag.ActivityCount = activityCount;
            ViewBag.CurrentRegistrations = account.GetRegistrationsByDate(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, msgs);
            ViewBag.RegistrationCount = account.GetRegistrationCountByDate(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, msgs);
            ViewBag.TotalPresent = account.GetAttendancePresentCount(msgs);
            ViewBag.TotalPossible = account.GetAttendancePossibleCount(msgs);
            ViewBag.TotalCompleted = account.GetAttendanceCompletedCount(msgs);
            ViewBag.ExternalLeagueCount = account.GetExternalLeagueCount(msgs);

            return View(account);
        }

        [HttpGet]
        public ActionResult GetStarted()
        {
            return View();
        }
    }
}