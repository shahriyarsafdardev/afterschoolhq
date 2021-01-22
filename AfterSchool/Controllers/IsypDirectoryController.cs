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
                var isypAccount = Account.LoadByInviteCodeToShare(new Guid(isypCodeToShare));

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

            var searchedAccounts = AccountService.SearchDirectoryAccounts(IsypHostAccountId, search, neighborhoods, grades, ages, gender, timing, cost, starting, other, special, msgs: msgs);

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