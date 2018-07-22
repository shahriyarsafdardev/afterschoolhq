using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;

namespace AfterSchool.Controllers
{
    public class RController : BaseController
    {
        // GET: R
        public ActionResult Index()
        {
            return View();
        }

        private void RestartRegistrationSession()
        {
            SessionVariables.CurrentRegistrant = null;
            SessionVariables.Cart = null;
        }

        [HttpGet]
        public ActionResult Account(string accountname)
        {
            RestartRegistrationSession();

            var account = AfterSchoolLibrary.Account.LoadByUniqueName(accountname);
            var participationObjects = new List<ParticipationObject>();
            var sessions = new List<ActivityGroup>();
            var msgs = new SystemMessages();

            if (account.IsNotEmpty())
            {
                participationObjects.AddRange(account.GetPublishedActivities().Select(x => x.ToParticipationObject()));
                participationObjects.AddRange(account.GetPublishedExternalLeagues().Select(x => x.ToParticipationObject()));
                sessions.AddRange(account.GetPublishedSessions());
            }
            else
            {
                // 404
            }

            ViewBag.ParticipationObjects = participationObjects;
            ViewBag.Sessions = sessions;
            ViewBag.Messages = msgs;

            return View("browse", account);
        }

        [HttpGet]
        public ActionResult Browse(string accountname)
        {
            RestartRegistrationSession();

            var account = AfterSchoolLibrary.Account.LoadByUniqueName(accountname);
            var participationObjects = new List<ParticipationObject>();
            var sessions = new List<ActivityGroup>();
            var msgs = new SystemMessages();

            if (account.IsNotEmpty())
            {
                participationObjects.AddRange(account.GetPublishedActivities().Select(x => x.ToParticipationObject()));
                participationObjects.AddRange(account.GetPublishedExternalLeagues().Select(x => x.ToParticipationObject()));
                sessions.AddRange(account.GetPublishedSessions());
            }
            else
            {
                // 404
            }

            ViewBag.ParticipationObjects = participationObjects;
            ViewBag.Sessions = sessions;
            ViewBag.Messages = msgs;

            return View(account);
        }

        public ActionResult Activity(string accountname, string activitytype, string activityname)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Account.LoadByUniqueName(accountname, msgs);
            var activity = AfterSchoolLibrary.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;

            return View(activity.ToParticipationObject());
        }

        public ActionResult Register(string accountname, string activitytype, string activityname)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Account.LoadByUniqueName(accountname, msgs);
            var activity = AfterSchoolLibrary.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);
            var participationObject = activity.ToParticipationObject(msgs);

            SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.LoggedInUser, msgs: msgs);
            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            if (SessionVariables.CurrentRegistrant.IsEmpty())
                return RedirectToAction("getstarted", "account", new { area = "", regaccount = account.Uid });
            else
                return RedirectToAction("register", "account", new { area = "", regaccount = account.Uid });
            
        }
    }
}