using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using BasicLibrary.DataAccess;

namespace AfterSchool.Controllers
{
    public class RController : Controller
    {
        // GET: R
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Account(string accountname)
        {
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
                return RedirectToAction("getstarted", "account", new { area = "" });


            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.Form = activity.GetActiveActivityForm(msgs);

            return View(participationObject);
        }

        //public ActionResult AccountHome(string accountName, string activityGroupName)
        //{
        //    var account = Account.LoadByUniqueName(accountName);
        //    var activityGroups = account.GetPublishedActivityGroups();
        //    var activities = account.GetPublishedActivities();
        //    return View();
        //}

        //public ActionResult ActivityGroupHome(string accountName, string activityGroupName)
        //{
        //    var account = Account.LoadByUniqueName(accountName);
        //    var activityGroup = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, activityGroupName);
        //    var activities = activityGroup.GetActivities();

        //    return View();
        //}

        //public ActionResult ActivityHome(string accountName, string activityName)
        //{
        //    var account = Account.LoadByUniqueName(accountName);
        //    var activity = Activity.LoadByUniqueNameByAccountId(activityName, account.ID.Value);

        //    return View();
        //}

        //public ActionResult ActivityRegistration(string accountName, string activityName)
        //{
        //    var account = Account.LoadByUniqueName(accountName);
        //    var activity = Activity.LoadByUniqueNameByAccountId(activityName, account.ID.Value);

        //    return View();
        //}
    }
}