using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchoolLibrary;
using AfterSchool.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountingController : BaseLoggedInController
    {
        public AccountingController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        // GET: Org/Accounting
        public ActionResult Index(string pn, string rpp)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var totalTransactionCount = SessionVariables.CurrentAccount.GetBilledTransactionCount();
            var transactions = SessionVariables.CurrentAccount.GetBilledTransactions(start, length);
            var totalPages = Math.Ceiling(totalTransactionCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalTransactionCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(transactions);
        }
    }
}