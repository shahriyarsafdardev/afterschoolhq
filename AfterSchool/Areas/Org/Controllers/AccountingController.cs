using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
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
        public ActionResult Index(string pn, string rpp, string sdate, string edate)
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
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) : new DateTime?();
            var endDate = edate.IsDateTime() ? 
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) : new DateTime?();

            var totalTransactionCount = SessionVariables.CurrentAccount.GetBilledTransactionCount(startDate, endDate);
            var transactions = SessionVariables.CurrentAccount.GetBilledTransactions(startDate, endDate, start, length);
            var totalPages = Math.Ceiling(totalTransactionCount.ToDouble() / recordsPerPage).ToInteger();

            transactions.ForEach(transaction => 
                transaction.CreateDate = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value)
            );

            ViewBag.TotalAmountPaid = SessionVariables.CurrentAccount.GetTotalAmountPaidTo(startDate, endDate, msgs);
            ViewBag.TotalFeesPaid = SessionVariables.CurrentAccount.GetTotalFeesPaidBy(startDate, endDate, msgs);
            ViewBag.Messages = msgs;
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