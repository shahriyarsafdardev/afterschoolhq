using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using BasicLibrary;
using System.Linq;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class TransactionController : BaseLoggedInController
    {
        public TransactionController() : base(new List<AccountType> { AccountType.AfterSchool })
        {

        }

        // GET: Admin/Transaction
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
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) :
                new DateTime?();
            var endDate = edate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) :
                new DateTime?();

            // transaction count
            var totalTransactionCount = Transaction.GetCount(startDate, endDate, false, msgs);

            // transaction data
            var allTransactions = Transaction.LoadByDate(startDate, endDate, false, start, length, msgs);

            var totalPages = Math.Ceiling(totalTransactionCount.ToDouble() / recordsPerPage).ToInteger();

            allTransactions.ForEach(transaction => {
                if (SessionVariables.CurrentAccount.TimeZone.IsNotEmpty())
                    transaction.CreateDate = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value);
            });

            ViewBag.TotalAmountPaid = Transaction.GetTotalAmountBilledOut(startDate, endDate, false, msgs);
            ViewBag.TotalFeesPaid = Transaction.GetTotalFeesPaid(startDate, endDate, false, msgs);
            ViewBag.TotalProfit = Transaction.GetTotalAHQProfit(startDate, endDate, false, msgs);
            ViewBag.TotalRefunded = Transaction.GetTotalAmountRefunded(startDate, endDate, false, msgs);
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
        public ActionResult Delete(string id)
        {
            var msgs = new SystemMessages();

            if (id.IsGuid())
            {
                var transaction = Transaction.LoadByUid(id.ToGuid(), msgs);

                if (transaction.IsNotEmpty())
                    transaction.MarkAsDeleted(SessionVariables.LoggedInUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Refund(string id)
        {
            var msgs = new SystemMessages();

            if (id.IsGuid())
            {
                var transaction = Transaction.LoadByUid(id.ToGuid(), msgs);

                if (transaction.IsNotEmpty() && !transaction.HasRefunds())
                    transaction.Refund(SessionVariables.LoggedInUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }
    }
}