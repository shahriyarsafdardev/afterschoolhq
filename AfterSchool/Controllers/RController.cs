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
            return Redirect($"/{accountname}/browse");            
        }

        [HttpGet]
        public ActionResult Browse(string accountname)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Account.LoadByUniqueName(accountname, msgs);
            var participationObjects = new List<ParticipationObject>();
            var sessions = new List<ActivityGroup>();
            var locations = new List<Location>();

            if (account.IsNotEmpty())
            {
                participationObjects.AddRange(account.GetPublishedActivities(msgs).Select(activity => activity.ToParticipationObject(msgs)));

                var hostedLocationIds = account.GetHostedLocations(msgs).Select(location => location.ID.Value).ToList();
                account.GetApprovedVisitorAccounts(msgs).ForEach(visitor =>
                {
                    participationObjects.AddRange(visitor.GetPublishedActivitiesInLocations(hostedLocationIds, msgs).Select(activity => activity.ToParticipationObject(msgs)));
                });

                participationObjects.AddRange(account.GetPublishedExternalLeagues(msgs).Select(x => x.ToParticipationObject(msgs)));
                sessions.AddRange(account.GetPublishedSessions(msgs));
                locations.AddRange(account.GetAllAvailableLocations(msgs));
            }
            else
            {
                // 404
            }

            ViewBag.Grades = GradeLevel.Load(msgs: msgs);
            ViewBag.ParticipationObjects = participationObjects;
            ViewBag.Sessions = sessions;
            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Locations = locations;

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