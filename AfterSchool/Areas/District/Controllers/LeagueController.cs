using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;
using AfterSchoolLibrary;

namespace AfterSchool.Areas.District.Controllers
{
    public class LeagueController : BaseLoggedInController
    {
        // GET: District/League
        public ActionResult Index(string pn, string rpp)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int count = SessionVariables.CurrentAccount.GetLeagueCount(false);
            var leagues = SessionVariables.CurrentAccount.GetLeagues(false, start, length);
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(leagues);
        }
    }
}