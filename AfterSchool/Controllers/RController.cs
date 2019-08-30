using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System;
using System.Text.RegularExpressions;

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
            var locations = new List<Location>();
            var grades = GradeLevel.Load(msgs: msgs);

            if (account.IsNotEmpty())
            {
                participationObjects.AddRange(account.GetPublishedParticipationObjects(msgs));
                locations.AddRange(account.GetAllAvailableLocations(msgs));

                participationObjects = participationObjects
                                        .Where(po => !po.IsActivity() || !po.GetActivity().ActivityGroupId.HasValue)
                                        .OrderBy(po => po.RegistrationPeriod.IsClosed(account))
                                        .ThenBy(po => po.RegistrationPeriod.WillOpen(account))
                                        .ThenBy(po => po.RegistrationPeriod.IsOpen(account))
                                        .ToList();

                ViewBag.RegistrationOpened = participationObjects.Where(po => po.RegistrationPeriod.IsOpen()).ToList();
                ViewBag.RegistrationClosed = participationObjects.Where(po => po.RegistrationPeriod.IsClosed()).ToList();
                ViewBag.RegistrationWillOpen = participationObjects.Where(po => po.RegistrationPeriod.WillOpen()).ToList();
            }
            else
            {
                if (CookieVariables.DemoUniqueOrgName.IsNotEmpty() && accountname != CookieVariables.DemoUniqueOrgName)
                    return Redirect($"/{CookieVariables.DemoUniqueOrgName}");

                var accountUniqueName = "";
                (accountname, accountUniqueName) = GetDemoAccountDetails(accountname);

                ParticipationObject.GenerateDemoObjects(ref account, participationObjects, locations, accountname, accountUniqueName, grades);
                participationObjects = participationObjects.Take(11).ToList();

                ViewBag.RegistrationOpened = participationObjects;
                ViewBag.RegistrationClosed = new List<ParticipationObject>();
                ViewBag.RegistrationWillOpen = new List<ParticipationObject>();
            }

            ViewBag.Grades = grades;
            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Locations = locations;
            ViewBag.ParticipationObjects = participationObjects;

            return View(account);
        }

        private (string AccountName, string AccountUniqueName) GetDemoAccountDetails(string accountName)
        {
            var accountUniqueName = "";

            if (CookieVariables.DemoOrgName.IsNotEmpty())
            {
                accountName = CookieVariables.DemoOrgName;
                accountUniqueName = CookieVariables.DemoUniqueOrgName;
            }
            else
            {
                var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                accountName = accountName.SplitByUpperCase();
                accountUniqueName = reg.Replace(accountName.GetMaxWords(25), "");
            }

            return (accountName, accountUniqueName);
        }

        [HttpGet]
        public ActionResult Activity(string accountname, string activitytype, string activityname, string preview)
        {
            RestartRegistrationSession();

            ParticipationObject participationObject = null;
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);  
            var canPublishActivity = false;

            if (account.IsNotEmpty())
            {
                var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

                if (activity.IsEmpty())
                    return HttpNotFound();

                participationObject = activity.GetParticipationObject(msgs);

                if (!activity.IsPublished && activity.GetLocation().HostAccountId != activity.AccountId)
                {
                    var accountOwnerId = activity.GetLocation().GetHostAcount().OwnerId;

                    if (SessionVariables.CurrentUser.ID == accountOwnerId)
                        canPublishActivity = true;
                }
            }
            else
            {
                var participationObjects = new List<ParticipationObject>();
                var locations = new List<Location>();
                var grades = GradeLevel.Load(msgs: msgs);

                var accountUniqueName = "";
                (accountname, accountUniqueName) = GetDemoAccountDetails(accountname);
                ParticipationObject.GenerateDemoObjects(ref account, participationObjects, locations, accountname, accountUniqueName, grades);

                participationObject = participationObjects.Find(x => x.UniqueName.ToLower() == activityname);

                if (participationObject.IsEmpty())
                    return HttpNotFound();
            }

            SetRegistrationStartPage(account);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.CanPublishActivity = canPublishActivity;
            ViewBag.CanRegisterFor = true;

            return View(participationObject);
        }

        [HttpGet]
        public ActionResult ActivitySession(string accountname, string sessionname, string invite, string decline)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty())
                return HttpNotFound();

            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);

            if (session.IsEmpty())
                return HttpNotFound();

            LoadSessionInvite(session, invite, decline, msgs);

            SetRegistrationStartPage(account);

            var canRegisterFor = !session.IsInviteOnly || session.IsInvited(SessionVariables.SessionInvite, msgs);

            if (canRegisterFor)
                return Redirect(session.BrowseUrl);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.CanRegisterFor = canRegisterFor;

            return View("activity", session.GetParticipationObject());
        }

        [HttpGet]
        public ActionResult League(string accountname, string districtname, string leaguename)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty())
                return HttpNotFound();

            var districtAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(districtname, msgs);

            if (districtAccount.IsEmpty())
                return HttpNotFound();

            var league = ActivityGroup.LoadByAccountIdByUniqueName(districtAccount.ID.Value, leaguename, msgs);

            if (league.IsEmpty())
                return HttpNotFound();

            var aag = league.GetAccountActivityGroup(account, msgs);

            SetRegistrationStartPage(account);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;

            return View("activity", aag.GetParticipationObject(msgs));
        }

        [HttpGet]
        public ActionResult Register(string accountname, string activitytype, string activityname)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty())
                return HttpNotFound();

            var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            var participationObject = activity.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, msgs);
        }

        [HttpGet]
        public ActionResult RegisterSession(string accountname, string sessionname)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty())
                return HttpNotFound();

            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);

            if (session.IsEmpty())
                return HttpNotFound();

            var participationObject = session.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, msgs);
        }

        [HttpGet]
        public ActionResult BrowseSession(string accountname, string sessionname, string invite, string decline)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty())
                return HttpNotFound();

            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);

            if (session.IsEmpty())
                return HttpNotFound();

            LoadSessionInvite(session, invite, decline, msgs);

            var canRegisterFor = !session.IsInviteOnly || (SessionVariables.SessionInvite.IsGuid() && session.IsInvited(SessionVariables.SessionInvite.ToGuid(), msgs));

            if (!canRegisterFor)
                return Redirect(session.HomePageUrl);

            List<Activity> activities = null;

            if (session.IsInviteOnly)
                activities = session.GetInvitedToActivities(SessionVariables.SessionInvite.ToGuid(), msgs);
            else
                activities = session.GetActivities(msgs);

            var participationObjects = activities.Select(activity => activity.GetParticipationObject(msgs)).ToList();

            ViewBag.Session = session.GetParticipationObject(msgs);
            ViewBag.Grades = GradeLevel.Load(msgs: msgs);
            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Locations = account.GetAllAvailableLocations(msgs);
            ViewBag.ParticipationObjects = participationObjects;
            ViewBag.RegistrationOpened = participationObjects.Where(po => po.RegistrationPeriod.IsOpen()).ToList();
            ViewBag.RegistrationClosed = participationObjects.Where(po => po.RegistrationPeriod.IsClosed()).ToList();
            ViewBag.RegistrationWillOpen = participationObjects.Where(po => po.RegistrationPeriod.WillOpen()).ToList();

            return View("browse", account);
        }

        [HttpGet]
        public ActionResult RegisterLeague(string accountname, string activitygrouptype, string externalaccountname, string group)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty())
                return HttpNotFound();

            var externalAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(externalaccountname, msgs);

            if (externalAccount.IsEmpty())
                return HttpNotFound();

            var activityGroup = ActivityGroup.LoadByAccountIdByUniqueName(externalAccount.ID.Value, group, msgs);

            if (activityGroup.IsEmpty())
                return HttpNotFound();

            var aag = activityGroup.GetAccountActivityGroup(account, msgs);
            var participationObject = aag.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, msgs);
        }

        private ActionResult RedirectToRegistration(Account account, ParticipationObject participationObject, SystemMessages msgs)
        {
            SetRegistrationStartPage(account);

            if (!participationObject.IsActivityGroup() || participationObject.GetActivityGroup().ActivityGroupTypeId != ActivityGroupType.Session)
                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);

            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            if (SessionVariables.CurrentRegistrant.IsEmpty())
                return RedirectToAction("getstarted", "account", new { area = "", regaccount = account.Uid });

            if (SessionVariables.CurrentRegistrant.IsParentOrGuardian)
                return RedirectToAction("register", "account", new { area = "", regaccount = account.Uid });

            return RedirectToAction("registerstudent", "account", new { area = "", regaccount = account.Uid });
        }

        private void LoadSessionInvite(ActivityGroup session, string invite, string decline, SystemMessages msgs)
        {
            if (invite.IsGuid())
            {
                SessionVariables.SessionInvite = invite;

                if (decline.IsBoolean() && decline.ToBoolean())
                    session.DeclineInvite(SessionVariables.SessionInvite.ToGuid(), msgs);
            }
        }
    }
}