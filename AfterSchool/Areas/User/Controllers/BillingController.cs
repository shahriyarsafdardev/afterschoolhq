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
        public ActionResult Transactions(string pn, string rpp, string sdate, string edate, string q)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 25;
            var earliestCorrectTransactionRecords = new DateTime(2019, 12, 1);

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

            if (!startDate.HasValue || startDate.Value < earliestCorrectTransactionRecords)
                startDate = earliestCorrectTransactionRecords;

            // transaction count
            var totalTransactionCount = SessionVariables.CurrentAccount.GetTransactionCount(q, startDate, endDate, false, msgs);

            // transaction data
            var allTransactions = SessionVariables.CurrentAccount.GetTransactions(q, startDate, endDate, false, start, length, msgs);

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
            var registrants = SessionVariables.CurrentUser.GetStudentRegistrants(msgs);
            var plans = new List<RegistrantSubscriptionPlan>();

            registrants.ForEach(registrant => {
                plans.AddRange(registrant.GetRegistrantSubscriptionPlans(msgs));
            });

            return View(plans.Where(plan => plan.CreateDate >= new DateTime(2019,12,1)).OrderByDescending(x => x.CreateDate.Value).ToList());
        }

        [HttpPost]
        public ActionResult RequestRefund(string uid, string amount, string reason)
        {
            var msgs = new SystemMessages();

            if (uid.IsGuid() && amount.IsNumber())
            {
                var transaction = Transaction.LoadByUid(uid.ToGuid(), msgs);
                var refundRequest = new RefundRequest();
                if (transaction.IsNotEmpty())
                    refundRequest = transaction.RequestRefund(amount.ToDecimal(), RefundRequestStatus.RequestedByParent, reason, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    SessionVariables.WebSession.LogRefundRequestUpdate(SessionVariables.CurrentAccount, SessionVariables.CurrentUser,
                        WebSessionActionType.Requested, refundRequest.Amount.ToCurrency(), refundRequest, msgs);
                    return new JsonResult
                        { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }

            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}