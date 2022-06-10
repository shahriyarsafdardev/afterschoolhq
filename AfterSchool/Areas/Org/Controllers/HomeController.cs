using System;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(FormConstants.OrganizationAccountTypes)
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
            if (account.CanReceivePaymentDeposits() &&
                (CookieVariables.OnboardingStepsAcknowledged.IsEmpty() || !CookieVariables.OnboardingStepsAcknowledged.ToBoolean()))
                CookieVariables.OnboardingStepsAcknowledged = true.ToString();

            if (account.OnboardingStepDone != OnboardingStepType.All)
                return HandleOnboardingRedirect();

            if (!account.CanReceivePaymentDeposits() && SessionVariables.AccountUserType == UserType.Treasurer)
                return RedirectToAction("getstartedpayments", "account");

            if (account.IsNewProviderForOtherAccounts())
                return RedirectToAction("getstartedpayments", "account");

            if (account.GetActivityCount(SessionVariables.AccountUserType, SessionVariables.CurrentUser, null, null, null, null, null) == 0 &&
                (account.AccountTypeId != AccountType.School || account.GetExternalLeagueInviteCount() == 0))
                return RedirectToAction("getstarted");

            var activityCount = account.GetActivityCount(
                SessionVariables.AccountUserType,
                currentUser,
                null,
                null,
                null,
                false,
                null,
                msgs);

            var totalPaid = account.GetTotalAmountBilledOut(null, PaymentSourceType.AfterSchoolPlatform, msgs: msgs) +
                            account.GetTotalAmountBilledOut(null, PaymentSourceType.ManualProvider, msgs: msgs);

            HandleTempMessages(msgs);

            ViewBag.Messages = msgs;
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