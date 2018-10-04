using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Controllers
{
    public class RController : BaseController
    {
        // GET: R
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Used to know where to direct someone if there is an error or lack of information
        /// the question is where did they start and how do we get them back there?
        /// </summary>
        /// <param name="account"></param>
        private void SetRegistrationStartPage(Account account, bool forceReset = false)
        {
            if (SessionVariables.RegistrationStartPageAccount.IsEmpty() || forceReset)
                SessionVariables.RegistrationStartPageAccount = account;
        }

        private void RestartRegistrationSession()
        {
            SessionVariables.CurrentRegistrant = null;
            SessionVariables.Cart = null;
        }

        [HttpGet]
        public ActionResult Account(string accountname)
        {
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname);
            RestartRegistrationSession();
            SetRegistrationStartPage(account, true);
            return Redirect($"/{accountname}/browse");            
        }

        [HttpGet]
        public ActionResult Browse(string accountname)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname);

            RestartRegistrationSession();
            SetRegistrationStartPage(account, true);

            var participationObjects = new List<ParticipationObject>();
            var sessions = new List<ActivityGroup>();
            var locations = new List<Location>();

            if (account.IsNotEmpty())
            {
                participationObjects.AddRange(account.GetPublishedActivities(msgs).Select(activity => activity.ToParticipationObject(msgs)));

                var hostedLocationIds = account.GetHostedLocations(msgs).Select(location => location.ID.Value).ToList();
                account.GetApprovedVisitorAccounts(msgs).ForEach(visitor =>
                {
                    participationObjects.AddRange(
                        visitor.GetPublishedActivitiesInLocations(hostedLocationIds, msgs)
                            .Select(activity =>  activity.ToParticipationObject(msgs))
                            .DistinctBy(participationObject => new { participationObject.ObjectID, participationObject.ObjectType }));
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

            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            SetRegistrationStartPage(account);

            var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);
            
            ViewBag.Messages = msgs;
            ViewBag.Account = account;

            return View(activity.ToParticipationObject());
        }

        public ActionResult League(string accountname, string districtname, string leaguename)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();

            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            SetRegistrationStartPage(account);

            var districtAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(districtname, msgs);
            var league = AfterSchoolLibrary.Models.ActivityGroup.LoadByAccountIdByUniqueName(districtAccount.ID.Value, leaguename, msgs);
            var aag = league.GetAccountActivityGroup(account, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;

            return View("activity", aag.ToParticipationObject(msgs));
        }

        public ActionResult Register(string accountname, string activitytype, string activityname)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);
            var participationObject = activity.ToParticipationObject(msgs);

            SetRegistrationStartPage(account);

            SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.LoggedInUser, msgs: msgs);
            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            if (SessionVariables.CurrentRegistrant.IsEmpty())
                return RedirectToAction("getstarted", "account", new { area = "", regaccount = account.Uid });

            return RedirectToAction("register", "account", new { area = "", regaccount = account.Uid });
        }

        public ActionResult RegisterLeague(string accountname, string activitygrouptype, string externalaccountname, string group)
        {
            var msgs = new SystemMessages();
            var currentAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var activityGroupType = activitygrouptype.ParseEnum<ActivityGroupType>();
            var externalAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(externalaccountname, msgs);
            var activityGroup = ActivityGroup.LoadByAccountIdByUniqueName(externalAccount.ID.Value, group, msgs);
            var aag = activityGroup.GetAccountActivityGroup(currentAccount, msgs);
            var participationObject = aag.ToParticipationObject(msgs);

            SetRegistrationStartPage(currentAccount);

            SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.LoggedInUser, msgs: msgs);
            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            if (SessionVariables.CurrentRegistrant.IsEmpty())
                return RedirectToAction("getstarted", "account", new { area = "", regaccount = currentAccount.Uid });

            return RedirectToAction("register", "account", new { area = "", regaccount = currentAccount.Uid });
        }
    }
}