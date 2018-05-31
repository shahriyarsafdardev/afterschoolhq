using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using BasicLibrary;
using AfterSchoolLibrary;
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