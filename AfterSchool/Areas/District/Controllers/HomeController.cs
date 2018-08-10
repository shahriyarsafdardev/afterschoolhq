using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;

namespace AfterSchool.Areas.District.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/Home
        public ActionResult Index()
        {
            List<ActivityGroup> activityGroups = ActivityGroup.LoadByAccountId(SessionVariables.CurrentAccount.ID.Value);
            List<Activity> activties = new List<Activity>();

            foreach(ActivityGroup activityGroup in activityGroups)
            {
                
                activties.AddRange(activityGroup.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School));
            }

            ViewBag.SubmittedActivities = activties.Where(sa => sa.DateSubmittedToActivityGroup.IsNotEmpty()).ToList();

            return View();
        }
    }
}