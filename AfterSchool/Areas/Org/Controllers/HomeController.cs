using System.Collections.Generic;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;

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
            if (SessionVariables.CurrentAccount.GetActivityCount(SessionVariables.AccountUserType, SessionVariables.LoggedInUser, null, null, null) == 0)
                return RedirectToAction("GetStarted");

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
                10);            
            ViewBag.Messages = new SystemMessages();
            ViewBag.Actions = actions;
            ViewBag.HasVisitorAccountsToApprove = SessionVariables.CurrentAccount.GetRequestedVisitorAccountsCount() > 0;
            ViewBag.HasOutstandingLeagueInvites = SessionVariables.CurrentAccount.GetExternalLeagueInviteCount() > 0;
            ViewBag.TotalPaid = SessionVariables.CurrentAccount.GetTotalAmountPaidTo();
            return View(account);
        }

        [HttpGet]
        public ActionResult GetStarted()
        {
            return View();
        }
    }
}