using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;

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
            // check for requests to post activities
            var requestingAccounts = "";

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
            return View(account);
        }
    }
}