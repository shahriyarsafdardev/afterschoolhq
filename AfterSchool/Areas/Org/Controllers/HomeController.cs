using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
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
            var currentUserType = currentUser.GetUserType(currentAccount);

            //ISYP Directory Check
            if (currentAccount.IsInIsypDirectory(msgs))
            {
                var hasNoPaymentMethod = !currentAccount.HasDefaultPaymentMethod.HasValue || !currentAccount.HasDefaultPaymentMethod.Value;
                var summerTrialEndDate = $"{DateTime.UtcNow.Year}/08/01".ToDateTime();
                var isBeforeSummer = DateTime.UtcNow.IsBefore(summerTrialEndDate);

                if (isBeforeSummer && !accountPackage.IsDuringTrial && hasNoPaymentMethod && accountPackage.IsFree)
                {
                    if (currentAccount.StripeCustomerId.IsNotEmpty())
                    {
                        var stripeCustomer = currentAccount.GetStripeCustomer(AfterSchoolLibrary.Billing.StripeAccountType.SubscriberAccount, msgs);
                        if (stripeCustomer.IsEmpty())
                            currentAccount.ClearStripeCustomerId(msgs);
                    }

                    if (currentAccount.StripeCustomerId.IsEmpty())
                        currentAccount.CreateStripeCustomer(
                            AfterSchoolLibrary.Billing.StripeAccountType.SubscriberAccount,
                            currentAccount.GetAccountOwner(msgs),
                            msgs);

                    SessionVariables.AccountPackage = accountPackage = currentAccount.SwitchPackagesWithStripe(
                        Package.LoadByIsDefaultCustomerPackage(true, msgs),
                        BillingPeriodType.Month,
                        false,
                        accountPackage.StripeSubscriptionId,
                        summerTrialEndDate,
                        currentUser,
                        msgs);
                }
            }

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

            var registrationCount = new int();
            var totalPresent = new int();
            var totalPossible = new int();
            var totalCompleted = new int();

            if (currentUserType == UserType.Instructor)
            {
                var activities = currentAccount.GetActivities(currentUserType, currentUser, null, ActivityType.Activity, false, false, false, null, null, msgs);
                var activitiesIds = activities.Select(a => a.ID.Value).ToList();
                totalPresent = activities.Sum(activity => activity.GetAttendancePresentCount(msgs));
                totalPossible = activities.Sum(activity => activity.GetAttendancePossibleCount(msgs));
                totalCompleted = activities.Sum(activity => activity.GetCompletedAttendance(msgs).Count());
                registrationCount = currentAccount.GetRegistrationsCountByActivityIDsByDate(activitiesIds, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, msgs);
            }
            else
            {
                registrationCount = currentAccount.GetRegistrationCountByDate(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, msgs);
                totalPresent = currentAccount.GetAttendancePresentCount(msgs);
                totalPossible = currentAccount.GetAttendancePossibleCount(msgs);
                totalCompleted = currentAccount.GetAttendanceCompletedCount(msgs);
            }

            ViewBag.Messages = msgs;
            ViewBag.HasVisitorAccountsToApprove = currentAccount.GetRequestedVisitorAccountsCount(msgs) > 0;
            ViewBag.HasOutstandingLeagueInvites = currentAccount.GetExternalLeagueInviteCount(msgs) > 0;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.ActivityCount = activityCount;
            ViewBag.CurrentRegistrations = currentAccount.GetRegistrationsByDate(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, msgs);
            ViewBag.RegistrationCount = registrationCount;
            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalPossible = totalPossible;
            ViewBag.TotalCompleted = totalCompleted;
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