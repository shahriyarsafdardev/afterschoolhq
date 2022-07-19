using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;

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
            var currentAccount = SessionVariables.CurrentAccount;
            
            // only check last 60 days to speed up query
            var activityGroups = ActivityGroup.LoadByAccountIdByDateRange(currentAccount.ID.Value, DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow);
            var activities = new List<Activity>();

            activityGroups.ForEach(activityGroup => 
                activities.AddRange(activityGroup.GetLeagueTeams(currentAccount, AccountType.School.AsList()))
            );

            ViewBag.SubmittedActivities = activities.Where(sa => sa.DateSubmittedToActivityGroup.IsNotEmpty())
                .OrderByDescending(sa => sa.DateSubmittedToActivityGroup.Value)
                .Take(10)
                .ToList();

            return View();
        }
    }
}