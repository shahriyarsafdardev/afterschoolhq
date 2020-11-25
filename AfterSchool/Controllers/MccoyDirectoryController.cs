using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class MccoyDirectoryController : Controller
    {
        private readonly string MccoyHostAccountId = string.Empty;
        private static List<GradeLevel> ListGrades = GradeLevel.Load();

        public MccoyDirectoryController()
        {
            var mcCoyCodeToShare = ConfigurationManager.AppSettings["MccoyInviteCodeToShare"];

            SystemMessages emsgs = new SystemMessages();
            emsgs.Add(new SystemMessage() { Text = mcCoyCodeToShare, MessageType = MessageType.UserError });
            AfterSchoolLibrary.Helpers.Utilities.SystemLog(emsgs);

            if (mcCoyCodeToShare.IsNotEmpty() && mcCoyCodeToShare.IsGuid())
            {
                var mccoyAccount = Account.LoadByInviteCodeToShare(new Guid(mcCoyCodeToShare));

                SystemMessages emsgs2 = new SystemMessages();
                emsgs2.Add(new SystemMessage() { Text = mccoyAccount.ID.ToString(), MessageType = MessageType.UserError });
                AfterSchoolLibrary.Helpers.Utilities.SystemLog(emsgs2);

                if (mccoyAccount.IsNotEmpty())
                    MccoyHostAccountId = mccoyAccount.ID.ToString();
            }
        }

        // GET: Mccoy
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var neighborhoodList = Location.LoadMccoyNeighborhoods(hostAccountId: MccoyHostAccountId, msgs: msgs)
                                   .Where(x => x.IsNotEmpty())
                                   .ToList();

            ViewBag.Messages = msgs;
            ViewBag.Neighborhoods = neighborhoodList;
            ViewBag.Grades = ListGrades;

            return View();
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

            var genderIds = "";
            if (gender.IsNotBlank() && gender.Contains("maleonly"))
                genderIds = "1,";
            if (gender.IsNotBlank() && gender.Contains("femaleonly"))
                genderIds += "2,";

            var searchedAccounts = AccountService.SearchMccoyAccounts(currentAccount, MccoyHostAccountId, search, neighborhoods, gradeIds, ageIds, genderIds, timing, cost, starting, other, special, msgs: msgs);

            if (searchedAccounts.IsNotEmpty() && searchedAccounts.Any())
            {
                return Json(new { success = true, status = "ok", data = searchedAccounts }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, status = "error", data = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }
    }
}