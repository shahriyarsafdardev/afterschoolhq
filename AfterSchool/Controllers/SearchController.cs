using System.Linq;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using static AfterSchoolLibrary.Helpers.GoogleGeoCode;

namespace AfterSchool.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index(string city, string state, string neighborhoods, string covid, string elc)
        {
            var msgs = new SystemMessages();

            ViewBag.Messages = msgs;
            ViewBag.Neighborhoods = null;
            ViewBag.Grades = GradeLevel.Load();
            ViewBag.AccountUniqueName = "Search";
            ViewBag.AccountId = "0";

            return View("~/views/r/search.cshtml");
        }

        [HttpPost]
        public JsonResult GetAccounts(string accountId, string search, string enrollToday, string grades, string ages,
            string gender, string services, string timing, string pricing, string cost, string starting, string other,
            string special, string zipCode, decimal? latitude, decimal? longitude, int pageNum = 0)
        {
            const int pageSize = 9;

            var msgs = new SystemMessages();

            var parsedGrade = ParseAndLoadGrade(grades);
            var parsedAge = ParseAndLoadAge(ages);
            var parsedGender = ParseAndLoadGender(gender);
            var showDistance = zipCode.IsNotEmpty();

            if (showDistance && !latitude.HasValue && !longitude.HasValue)
                (latitude, longitude) = GetLatitudeAndLongitude(zipCode);

            var searchedAccounts = AccountService.SearchDirectoryAccounts(accountId.ToInteger(), search, enrollToday,
                parsedGrade, parsedAge, parsedGender, services, timing, pricing, cost, starting, other, special,
                latitude, longitude, pageNum, pageSize, msgs);

            if (searchedAccounts.IsNotEmpty() && searchedAccounts.Any())
                //.. call success and records found
                return Json(new
                    {
                        success = true,
                        status = "ok",
                        data = searchedAccounts,
                        pageNum,
                        showDistance,
                        latitude,
                        longitude,
                    },
                    JsonRequestBehavior.AllowGet);
            if (msgs.IsNotEmpty() && msgs.Count > 0)
                //.. something went wrong, error occured
                return Json(
                    new
                    {
                        success = false, status = "error", data = msgs.ToList().Select(msg => new {msg.Text}), pageNum,
                        showDistance,
                    }, JsonRequestBehavior.AllowGet);

            //.. no result found
            return Json(new {success = false, status = "no-result"}, JsonRequestBehavior.AllowGet);
        }

        #region Private Methods

        private static string ParseAndLoadGrade(string grades)
        {
            var gradeIds = "";
            var ListGrades = GradeLevel.Load();

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

            return gradeIds;
        }

        private static string ParseAndLoadAge(string ages)
        {
            var ageIds = "";
            if (ages.IsNotEmpty())
            {
                var lstAges = ages.Split(',');
                foreach (var item in lstAges)
                    if (item.Contains('_'))
                    {
                        var age = item.Split('_');

                        if (age.IsNotEmpty())
                        {
                            var ageStart = age[0];
                            var ageEnd = age[1];

                            if (ageStart.IsNotEmpty() && ageEnd.IsNotEmpty())
                                for (var i = ageStart.ToInteger(); i <= ageEnd.ToInteger(); i++)
                                    ageIds += $"{i},";
                        }
                    }
            }

            return ageIds;
        }

        private static string ParseAndLoadGender(string gender)
        {
            var genderIds = "";
            if (gender.IsNotBlank() && gender == GenderRestrictionType.MaleOnly.ToString().ToLower())
                genderIds = "1,";
            if (gender.IsNotBlank() && gender == GenderRestrictionType.FemaleOnly.ToString().ToLower())
                genderIds += "2,";

            return genderIds;
        }

        #endregion
    }
}