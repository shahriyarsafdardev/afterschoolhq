using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace AfterSchool.Controllers
{
    public class IsypDirectoryController : Controller
    {
        private readonly string IsypHostAccountId;
        private static List<GradeLevel> ListGrades = GradeLevel.Load();

        public IsypDirectoryController()
        {
            var isypCodeToShare = ConfigurationManager.AppSettings["ISYPInviteCodeToShare"];

            if (isypCodeToShare.IsNotEmpty() && isypCodeToShare.IsGuid())
            {
                var isypAccount = Account.LoadByInviteCodeToShare(isypCodeToShare.ToGuid());

                if (isypAccount.IsNotEmpty())
                    IsypHostAccountId = isypAccount.ID.ToString();
            }
        }

        // GET: ISYPDirectory
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var neighborhoodList = Location.LoadNeighborhoodsByHostAccount(hostAccountId: IsypHostAccountId, msgs: msgs).ToList();

            ViewBag.Messages = msgs;
            ViewBag.Neighborhoods = neighborhoodList;
            ViewBag.Grades = ListGrades;

            return View();
        }

        [HttpPost]
        public JsonResult GetISYPSearch(string search, string neighborhoods, string grades, string ages, string gender, string timing, string cost, string starting, string other, string special)
        {
            var msgs = new SystemMessages();
            var parsedGrade = ParseAndLoadGrade(grades);
            var parsedAge = ParseAndLoadAge(ages);
            var parsedGender = ParseAndLoadGender(gender);

            var searchedAccounts = AccountService.SearchDirectoryAccounts(IsypHostAccountId, search, neighborhoods, parsedGrade, parsedAge, parsedGender, timing, cost, starting, other, special, msgs: msgs);

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

        #region Private Methods

        private static string ParseAndLoadGrade(string grades)
        {
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

            return gradeIds;
        }

        private static string ParseAndLoadAge(string ages)
        {
            var ageIds = "";
            if (ages.IsNotEmpty())
            {
                var lstAges = ages.Split(',');
                foreach (var item in lstAges)
                {
                    if (item.Contains('_'))
                    {
                        var age = item.Split('_');

                        if (age.IsNotEmpty())
                        {
                            var ageStart = age[0];
                            var ageEnd = age[1];

                            if (ageStart.IsNotEmpty() && ageEnd.IsNotEmpty())
                            {
                                for (int i = ageStart.ToInteger(); i <= ageEnd.ToInteger(); i++)
                                {
                                    ageIds += $"{i},";
                                }
                            }
                        }
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