using System.Collections.Generic;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System;
using System.Linq;

namespace AfterSchool.Areas.Org.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(AccountService.ProviderAccountTypes.ToList())
        {

        }

        // GET: Org/Home
        [HttpGet]
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            // only redirect if onboarding steps haven't been acknowledged
            if (account.CanReceivePaymentDeposits() && (CookieVariables.OnboardingStepsAcknowledged.IsEmpty() || !CookieVariables.OnboardingStepsAcknowledged.ToBoolean()))
                CookieVariables.OnboardingStepsAcknowledged = true.ToString();

            if (account.OnboardingStepDone != OnboardingStepType.All)
                return HandleOnboardingRedirect();

            if (!account.CanReceivePaymentDeposits() && SessionVariables.AccountUserType == UserType.Treasurer)
                return RedirectToAction("getstartedpayments", "account");

            if (account.IsNewProviderForOtherAccounts())
                return RedirectToAction("getstartedpayments", "account");

            if (account.GetActivityCount(SessionVariables.AccountUserType, SessionVariables.CurrentUser, null, null, null, null) == 0 && (account.AccountTypeId != AccountType.School || account.GetExternalLeagueInviteCount() == 0))
                return RedirectToAction("getstarted");                      

            
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
                currentUser,
                null,
                null,
                false,
                null,
                msgs);

            var totalPaid = account.GetTotalAmountBilledOut(search: null, paymentSourceType: PaymentSourceType.AfterSchoolPlatform, msgs: msgs) +
                            account.GetTotalAmountBilledOut(search: null, paymentSourceType: PaymentSourceType.ManualProvider, msgs: msgs);

            HandleTempMessages(msgs);

            ViewBag.Messages = msgs;
            ViewBag.Actions = actions;
            ViewBag.HasVisitorAccountsToApprove = account.GetRequestedVisitorAccountsCount(msgs) > 0;
            ViewBag.HasOutstandingLeagueInvites = account.GetExternalLeagueInviteCount(msgs) > 0;
            ViewBag.TotalPaid = totalPaid;
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
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            ViewBag.Messages = msgs;
            return View();
        }
    }
}