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
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var accountPackage = SessionVariables.AccountPackage;


            // only redirect if onboarding steps haven't been acknowledged
            if (currentAccount.CanReceivePaymentDeposits() &&
                (CookieVariables.OnboardingStepsAcknowledged.IsEmpty() || !CookieVariables.OnboardingStepsAcknowledged.ToBoolean()))
                CookieVariables.OnboardingStepsAcknowledged = true.ToString();

            if (currentAccount.OnboardingStepDone != OnboardingStepType.All)
                return HandleOnboardingRedirect();

            // force to select a plan
            if (accountPackage.IsTrialExpiredAndUnpaid(currentAccount))
                return RedirectToAction("selectplan", "account");

            if (!currentAccount.CanReceivePaymentDeposits() && SessionVariables.AccountUserType == UserType.Treasurer)
                return RedirectToAction("getstartedpayments", "account");

            /*
             * probably not necessary, every body that works with a new account doesn't need payments setup
            if (currentAccount.IsNewProviderForOtherAccounts())
                return RedirectToAction("getstartedpayments", "account");
            */

            if (currentAccount.GetActivityCount(SessionVariables.AccountUserType, SessionVariables.CurrentUser, null, null, null, null, null) == 0 &&
                (currentAccount.AccountTypeId != AccountType.School || currentAccount.GetExternalLeagueInviteCount() == 0))
                return RedirectToAction("getstarted");

            var activityCount = currentAccount.GetActivityCount(
                SessionVariables.AccountUserType,
                currentUser,
                null,
                null,
                null,
                false,
                null,
                msgs);

            var totalPaid = currentAccount.GetTotalAmountBilledOut(null, PaymentSourceType.AfterSchoolPlatform, msgs: msgs) +
                            currentAccount.GetTotalAmountBilledOut(null, PaymentSourceType.ManualProvider, msgs: msgs);

            HandleTempMessages(msgs);

            ViewBag.Messages = msgs;
            ViewBag.HasVisitorAccountsToApprove = currentAccount.GetRequestedVisitorAccountsCount(msgs) > 0;
            ViewBag.HasOutstandingLeagueInvites = currentAccount.GetExternalLeagueInviteCount(msgs) > 0;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.ActivityCount = activityCount;
            ViewBag.CurrentRegistrations = currentAccount.GetRegistrationsByDate(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, msgs);
            ViewBag.RegistrationCount = currentAccount.GetRegistrationCountByDate(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, msgs);
            ViewBag.TotalPresent = currentAccount.GetAttendancePresentCount(msgs);
            ViewBag.TotalPossible = currentAccount.GetAttendancePossibleCount(msgs);
            ViewBag.TotalCompleted = currentAccount.GetAttendanceCompletedCount(msgs);
            ViewBag.ExternalLeagueCount = currentAccount.GetExternalLeagueCount(msgs);

            return View(currentAccount);
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