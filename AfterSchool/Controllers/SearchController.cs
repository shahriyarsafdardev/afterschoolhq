using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace AfterSchool.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index(string city, string state, string neighborhoods, string covid, string elc)
        {
            var msgs = new SystemMessages();

            //List<string> neighborhoodSearch = null;

            //if (neighborhoods.IsNotEmpty())
            //    neighborhoodSearch = neighborhoods.Split(',').ToList();

            //var providers = AccountProfile.Search(
            //                    accountTypes: AccountService.CustomerAccountTypes,
            //                    isOpenDuringCovid: covid.IsEmpty() ? new bool?() : covid.ToBoolean(),
            //                    isELearningCenter: elc.IsEmpty() ? new bool?() : elc.ToBoolean(),
            //                    neighborhoods: neighborhoodSearch,
            //                    cities: city.IsEmpty() ? null : city.AsList(),
            //                    states: state.IsEmpty() ? null : state.AsList(),
            //                    msgs: msgs);

            //var neighborhoodList = Location.LoadAllNeighborhoods(
            //                            city: city,
            //                            state: state,
            //                            msgs: msgs)
            //                        .Where(x => x.IsNotEmpty())
            //                        .ToList();

            //ViewBag.Messages = msgs;
            //ViewBag.Neighborhoods = neighborhoodList;
            //ViewBag.City = city;
            //ViewBag.State = state;

            //return View(providers.Shuffle().ToList());

            //var neighborhoodList = Location.LoadNeighborhoodsByHostAccount(hostAccountId: 0, msgs: msgs).ToList();

            ViewBag.Messages = msgs;
            ViewBag.Neighborhoods = null;
            ViewBag.Grades = GradeLevel.Load();
            ViewBag.AccountUniqueName = "Search";
            ViewBag.AccountId = "0";

            return View("~/views/r/search.cshtml");
        }

        [HttpPost]
        public JsonResult GetAccounts(string accountId, string search, string neighborhoods, string grades, string ages, string gender, string timing, string cost, string starting, string other, string special)
        {
            var msgs = new SystemMessages();

            var searchedAccounts = AccountService.SearchDirectoryAccounts(accountId, search, neighborhoods, grades, ages, gender, timing, cost, starting, other, special, msgs: msgs);

            if (searchedAccounts.IsNotEmpty() && searchedAccounts.Any())
            {
                //.. call success and records found
                return Json(new { success = true, status = "ok", data = searchedAccounts }, JsonRequestBehavior.AllowGet);
            }
            else if (msgs.IsNotEmpty() && msgs.Count > 0)
            {
                //.. something went wrong, error occured
                return Json(new { success = false, status = "error", data = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
            }

            //.. no result found
            return Json(new { success = false, status = "no-result" }, JsonRequestBehavior.AllowGet);
        }
    }
}