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
            var activityGroups = ActivityGroup.LoadByAccountId(SessionVariables.CurrentAccount.ID.Value);
            var activties = new List<Activity>();

            activityGroups.ForEach(activityGroup => 
                activties.AddRange(activityGroup.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School.AsList()))
            );

            ViewBag.SubmittedActivities = activties.Where(sa => sa.DateSubmittedToActivityGroup.IsNotEmpty())
                .OrderByDescending(sa => sa.DateSubmittedToActivityGroup.Value)
                .Take(10)
                .ToList();

            return View();
        }
    }
}