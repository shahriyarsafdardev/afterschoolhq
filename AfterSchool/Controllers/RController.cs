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
                participationObjects.AddRange(account.GetPublishedActivities(msgs).Select(activity => activity.GetParticipationObject(msgs)));

                var hostedLocationIds = account.GetHostedLocations(msgs).Select(location => location.ID.Value).ToList();
                account.GetApprovedVisitorAccounts(msgs).ForEach(visitor =>
                {
                    participationObjects.AddRange(
                        visitor.GetPublishedActivitiesInLocations(hostedLocationIds, msgs)
                            .Select(activity =>  activity.GetParticipationObject(msgs))
                            .DistinctBy(participationObject => new { participationObject.ObjectID, participationObject.ObjectType }));
                });

                participationObjects.AddRange(account.GetPublishedExternalLeagues(msgs).Select(x => x.GetParticipationObject(msgs)));
                participationObjects.AddRange(account.GetPublishedSessions(msgs).Select(x => x.GetParticipationObject(msgs)));
                locations.AddRange(account.GetAllAvailableLocations(msgs));
            }
            else
            {
                var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var accountUniqueName = reg.Replace(accountname.GetMaxWords(25), "");

                account = new Account()
                {
                    Name = accountname.SplitByUpperCase(),
                    UniqueName = accountUniqueName,
                    IsActive = true,
                    Uid = Guid.NewGuid()
                };

                var activityNames = new List<string>() { "Chess Club", "Robotics", "Theater Club", "Movie Making" };
                var contactNames = new List<string>() { "Geraldine Fisher", "Robert Henderson", "James Boyce" };
                var locationDetails = new List<string>() { "Room 105", "Meet in the Cafeteria", "Gym", "Football Field" };
                var costs = new List<decimal>() { 100, 50, 27.50M, 0, 10 };
                var genderRestrictions = new List<GenderRestrictionType>() { GenderRestrictionType.None, GenderRestrictionType.MaleOnly, GenderRestrictionType.FemaleOnly };
                var gradeRestrictions = grades.OrderBy(x => x.ID).Take(9);

                locations.Add(new Location()
                {
                    Address1 = "12175 Visionary Way",
                    Address2 = "#710",
                    City = "Indianapolis",
                    State = "IN",
                    PostalCode = "46038",
                    Country = "US"
                });

                var registrationPeriod = new RegistrationPeriod()
                {
                    StartDateTime = DateTime.UtcNow.Date,
                    EndDateTime = DateTime.UtcNow.Date.AddDays(2).AddMinutes(-1)                     
                };

                var minGradeId = grades.Shuffle().First().ID;
                var maxGradeId = minGradeId + 3;

                // load demo account
                activityNames.ForEach(activityName => 
                {                    
                    var uniqueName = reg.Replace(activityName.GetMaxWords(25), "");
                    var cost = costs.Shuffle().First();

                    participationObjects.Add(new ParticipationObject()
                    {
                        OwnerAccount = account,
                        Schedule = ActivitySchedule.GetNewDefaultSchedule(),
                        SeatsRemaining = 20,
                        Description = "",
                        UniqueName = uniqueName,
                        Name = activityName,
                        AllowToPayLater = true,
                        ContactName = contactNames.Shuffle().First(),
                        ContactEmail = $"info@{accountUniqueName}.com",
                        ContactPhone = "888-123-1234",
                        Cost = cost,
                        CostUnitType = CostUnitType.PerRegistrant,
                        Details = "",
                        Location = locations.First(),
                        PaymentPlans = new List<SubscriptionPlan>(),
                        SubscriptionPlans = new List<SubscriptionPlan>(),
                        WaitingListSeatsRemaining = 10,
                        RegistrationMode = RegistrationMode.Register,
                        RegistrationPeriod = registrationPeriod,
                        Uid = Guid.NewGuid(),
                        LocationDetails = $"<p>{locationDetails.Shuffle().First()}</p>",
                        IsFree = cost == 0,
                        IsTuitionFree = cost == 0,
                        GenderRestriction = genderRestrictions.Shuffle().First(),
                        MinGradeId = minGradeId,
                        MaxGradeId = maxGradeId                        
                    });
                });
            }

            ViewBag.Grades = grades;
            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Locations = locations;
            ViewBag.ParticipationObjects = participationObjects.OrderBy(po => po.RegistrationPeriod.IsClosed(account))
                                            .ThenBy(po => po.RegistrationPeriod.WillOpen(account))
                                            .ThenBy(po => po.RegistrationPeriod.IsOpen(account))
                                            .ToList();

            return View(account);
        }

        public ActionResult Activity(string accountname, string activitytype, string activityname)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            SetRegistrationStartPage(account);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;

            return View(activity.GetParticipationObject());
        }

        public ActionResult ActivitySession(string accountname, string sessionname)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);

            SetRegistrationStartPage(account);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;

            return View("activity", session.GetParticipationObject());
        }

        public ActionResult League(string accountname, string districtname, string leaguename)
        {
            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var districtAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(districtname, msgs);
            var league = AfterSchoolLibrary.Models.ActivityGroup.LoadByAccountIdByUniqueName(districtAccount.ID.Value, leaguename, msgs);
            var aag = league.GetAccountActivityGroup(account, msgs);

            SetRegistrationStartPage(account);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;

            return View("activity", aag.GetParticipationObject(msgs));
        }

        public ActionResult Register(string accountname, string activitytype, string activityname)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);
            var participationObject = activity.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, msgs);
        }

        public ActionResult RegisterSession(string accountname, string sessionname)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);
            var participationObject = session.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, msgs);
        }

        public ActionResult RegisterLeague(string accountname, string activitygrouptype, string externalaccountname, string group)
        {
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var activityGroupType = activitygrouptype.ParseEnum<ActivityGroupType>();
            var externalAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(externalaccountname, msgs);
            var activityGroup = ActivityGroup.LoadByAccountIdByUniqueName(externalAccount.ID.Value, group, msgs);
            var aag = activityGroup.GetAccountActivityGroup(account, msgs);
            var participationObject = aag.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, msgs);
        }

        private ActionResult RedirectToRegistration(Account account, ParticipationObject participationObject, SystemMessages msgs)
        {
            SetRegistrationStartPage(account);

            SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.LoggedInUser, msgs: msgs);
            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            if (SessionVariables.CurrentRegistrant.IsEmpty())
                return RedirectToAction("getstarted", "account", new { area = "", regaccount = account.Uid });

            return RedirectToAction("register", "account", new { area = "", regaccount = account.Uid });
        }
    }
}