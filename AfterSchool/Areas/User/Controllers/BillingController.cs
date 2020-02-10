using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System;

namespace AfterSchool.Areas.User.Controllers
{
    public class BillingController : BaseLoggedInController
    {
        public BillingController() : base(new List<AccountType>() { AccountType.Registrant })
        {

        }

        public ActionResult Index()
        {
            return View();
        }

       [HttpGet]
       public ActionResult Transactions(string pn, string rpp, string sdate, string edate)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 25;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var startDate = sdate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) :
                new DateTime?();
            var endDate = edate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) :
                new DateTime?();

            // transaction count
            var totalTransactionCount = SessionVariables.CurrentAccount.GetPaidTransactionCount(startDate, endDate, msgs);

            // transaction data
            var allTransactions = SessionVariables.CurrentAccount.GetPaidTransactions(startDate, endDate, start, length, msgs);

            var totalPages = Math.Ceiling(totalTransactionCount.ToDouble() / recordsPerPage).ToInteger();

            allTransactions.ForEach(transaction => {
                if (SessionVariables.CurrentAccount.TimeZone.IsNotEmpty())
                    transaction.CreateDate = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value);
            });

            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalTransactionCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(allTransactions);
        }

        [HttpGet]
        public ActionResult Plans()
        {
            var msgs = new SystemMessages();
            var registrants = SessionVariables.CurrentUser.GetChildrenRegistrants(msgs);
            var plans = new List<RegistrantSubscriptionPlan>();

            registrants.ForEach(registrant => {
                plans.AddRange(registrant.GetRegistrantSubscriptionPlans(msgs));
            });

            return View(plans.OrderByDescending(x => x.CreateDate.Value).ToList());
        }
    }
}