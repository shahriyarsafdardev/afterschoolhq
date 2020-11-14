using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Controllers
{
    public class SearchController : Controller
    {
        // ..TODO: this should be off mccoy and we have to make this dynamic. ask darye once
        private static string MccoyHostAccountId = "3106";
        private static List<GradeLevel> ListGrades = GradeLevel.Load();

        // GET: Search
        public ActionResult Index(string city, string state, string neighborhoods, string covid, string elc)
        {
            var msgs = new SystemMessages();

            List<string> neighborhoodSearch = null;

            if (neighborhoods.IsNotEmpty())
                neighborhoodSearch = neighborhoods.Split(',').ToList();

            var providers = AccountProfile.Search(
                                accountTypes: AccountService.CustomerAccountTypes,
                                isOpenDuringCovid: covid.IsEmpty() ? new bool?() : covid.ToBoolean(),
                                isELearningCenter: elc.IsEmpty() ? new bool?() : elc.ToBoolean(),
                                neighborhoods: neighborhoodSearch,
                                cities: city.IsEmpty() ? null : city.AsList(),
                                states: state.IsEmpty() ? null : state.AsList(),
                                msgs: msgs);

            var neighborhoodList = Location.LoadAllNeighborhoods(
                                        city: city,
                                        state: state,
                                        msgs: msgs)
                                    .Where(x => x.IsNotEmpty())
                                    .ToList();

            ViewBag.Messages = msgs;
            ViewBag.Neighborhoods = neighborhoodList;
            ViewBag.City = city;
            ViewBag.State = state;

            return View(providers.Shuffle().ToList());
        }

        public ActionResult Mccoy()
        {            
            var msgs = new SystemMessages();
            var neighborhoodList = Location.LoadMccoyNeighborhoods(hostAccountId: MccoyHostAccountId, msgs: msgs)
                                   .Where(x => x.IsNotEmpty())
                                   .ToList();            

            ViewBag.Messages = msgs;
            ViewBag.Neighborhoods = neighborhoodList;
            ViewBag.Grades = ListGrades;

            return View("~/Views/mccoy/Index.cshtml");
        }

        [HttpPost]
        public JsonResult GetMccoySearch(string search, string neighborhoods, string grades, string ages, string gender, string timing, string cost, string starting, string other, string special)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            var gradeIds = "";
            if (grades.IsNotEmpty())
            {
                var lstGrades = grades.Split(',');
                foreach (var item in lstGrades)
                {
                    var grade = ListGrades.FirstOrDefault(m => m.Name.ToLower().Replace(" ", "_") == item);

                    if (grade.IsNotEmpty())
                        gradeIds += $"{grade.ID},";
                }
            }

            var searchedAccounts = AccountService.SearchMccoyAccounts(currentAccount, MccoyHostAccountId, search, neighborhoods, gradeIds, other, special, msgs: msgs);

            if (searchedAccounts.IsNotEmpty() && searchedAccounts.Any())
            {
                return Json(new { success = true, status = "ok", data = searchedAccounts }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, status = "error", data = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }
    }
}