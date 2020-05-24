using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using BasicLibrary;
using System.Linq;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountingController : BaseLoggedInController
    {
        public AccountingController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        // GET: Org/Accounting
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string sdate, string edate, string poId, string poTypeId)
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

            //PO filters
            List<(int ID, ParticipationObjectType ObjectType)> poFilter = null;
            if (!string.IsNullOrEmpty(poId) && !string.IsNullOrEmpty(poTypeId))
            {
                poFilter = new List<(int ID, ParticipationObjectType ObjectType)>();
                poFilter.Add((poId.ToInteger(), (ParticipationObjectType)poTypeId.ToInteger()));
            }

            var totalTransactionCount = SessionVariables.CurrentAccount.GetTransactionCount(startDate, endDate, poFilter, false);
            var allTransactions = SessionVariables.CurrentAccount.GetTransactions(startDate, endDate, poFilter, false, start, length);
            var totalPages = Math.Ceiling(totalTransactionCount.ToDouble() / recordsPerPage).ToInteger();

            allTransactions.ForEach(transaction =>
                transaction.CreateDate = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value)
            );

            var allPos = Transaction.GetParticipationObjectsByBillerAccountId(SessionVariables.CurrentAccount.ID.Value, null, null, false);
            var allParticipationObjects = allPos.DistinctBy(po => (po.ID, po.ParticipationObjectTypeId, po.Name));

            ViewBag.TotalAmountPaid = SessionVariables.CurrentAccount.GetTotalAmountBilledOut(null, null, startDate, endDate, msgs);
            ViewBag.TotalAmountRefunded = SessionVariables.CurrentAccount.GetTotalAmountRefunded(startDate, endDate, msgs);
            ViewBag.TotalAmountRefundedManual = SessionVariables.CurrentAccount.GetTotalAmountRefundedManually(startDate, endDate, msgs);
            ViewBag.TotalAmountRefundedAHQ = SessionVariables.CurrentAccount.GetTotalAmountRefundedAHQ(true, startDate, endDate, msgs);
            ViewBag.TotalFeesPaid = SessionVariables.CurrentAccount.GetTotalFeesPaidAsBiller(null, startDate, endDate, msgs);
            ViewBag.TotalPaidOut = SessionVariables.CurrentAccount.GetTotalPayoutAmount(PayoutStatus.Completed, startDate, endDate, msgs);
            ViewBag.TotalAvailableForPayout = SessionVariables.CurrentAccount.GetMaxAmountAvailableForPayout(startDate, endDate, msgs);
            ViewBag.ParticipationObjects = allParticipationObjects;
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
        public ActionResult Payouts(string pn, string rpp, string sdate, string edate)
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

            var totalPayoutAmount = SessionVariables.CurrentAccount.GetTotalPayoutAmount(PayoutStatus.Completed, startDate, endDate, msgs);
            var totalPayoutCount = SessionVariables.CurrentAccount.GetPayoutCount(null, startDate, endDate);
            var payouts = SessionVariables.CurrentAccount.GetPayouts(null, startDate, endDate, start, length);
            var totalPages = Math.Ceiling(totalPayoutCount.ToDouble() / recordsPerPage).ToInteger();

            payouts.ForEach(payout =>
                payout.DateRequested = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(payout.DateRequested.Value)
            );

            // sort transactions
            payouts = payouts.OrderByDescending(x => x.ID).ToList();

            ViewBag.CanReceiveACHDeposits = SessionVariables.CurrentAccount.CanReceiveACHDeposits(msgs);
            ViewBag.CanReceiveDeposits = SessionVariables.CurrentAccount.CanReceivePaymentDeposits(msgs);
            ViewBag.MaxRequestAmount = SessionVariables.CurrentAccount.GetMaxAmountAvailableForPayout(msgs: msgs);
            ViewBag.LastPayout = SessionVariables.CurrentAccount.GetLastPayout(msgs);
            ViewBag.TotalPayoutAmount = totalPayoutAmount;
            ViewBag.TotalPendingAmount = SessionVariables.CurrentAccount.GetTotalPayoutAmount(PayoutStatus.InProgress, msgs: msgs);
            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalPayoutCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(payouts);
        }

        [HttpGet]
        public ActionResult PayoutTransactions(string id)
        {
            var transactions = new List<Transaction>();

            if (id.IsGuid())
            {
                var msgs = new SystemMessages();
                var payout = Payout.LoadByUid(id.ToGuid(), msgs);

                transactions = payout.GetTransactions(msgs);

                ViewBag.TotalPayout = transactions.Sum(p => p.GetBillerPayout()).ToCurrency();
            }
            return View(transactions);
        }

        [HttpPost]
        public ActionResult RequestPayout()
        {
            var transactionIds = Request.Form["TransactionIds"];
            var msgs = new SystemMessages();

            if (!string.IsNullOrEmpty(transactionIds))
            {
                var ids = transactionIds.Split(',').Select(p => Convert.ToInt32(p)).ToList();
                var transactions = Transaction.LoadList(ids, msgs);

                ViewBag.TranIds = transactionIds;
                ViewBag.TotalPayoutAmount = transactions.Sum(transaction => transaction.GetBillerPayout(msgs));
                ViewBag.MaxPayoutAvailable = SessionVariables.CurrentAccount.GetMaxAmountAvailableForPayout(msgs: msgs);
                ViewBag.Messages = msgs;

                return View(transactions);
            }
            return null;
        }

        [HttpPost]
        public ActionResult RequestPayoutDone()
        {
            var transactionIds = Request.Form["TransactionIds"];
            var transactions = new List<Transaction>();
            var msgs = new SystemMessages();

            if (!string.IsNullOrEmpty(transactionIds))
            {
                var ids = transactionIds.Split(',').Select(p => p.ToInteger()).ToList();

                transactions = Transaction.LoadList(ids).ToList();
                ViewBag.TotalAmount = transactions.Sum(p => p.GetBillerPayout()).ToCurrency();
                ViewBag.TranIds = transactionIds;
                ViewBag.TotalPayoutAmount = ViewBag.TotalAmount;

                SessionVariables.CurrentAccount.RequestNewPayout(transactions, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return View();
            }

            ViewBag.Messages = msgs;

            return View("RequestPayout", transactions);
        }

        public ActionResult DownloadCSV(string sdate, string edate, string poId, string poTypeId)
        {
            var startDate = sdate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) :
                new DateTime?();
            var endDate = edate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) :
                new DateTime?();

            //PO filters
            List<(int ID, ParticipationObjectType ObjectType)> poFilter = null;
            if (!string.IsNullOrEmpty(poId) && !string.IsNullOrEmpty(poTypeId))
            {
                poFilter = new List<(int ID, ParticipationObjectType ObjectType)>();
                poFilter.Add((poId.ToInteger(), (ParticipationObjectType)poTypeId.ToInteger()));
            }

            // transaction data
            var billedTransactions = SessionVariables.CurrentAccount.GetBilledTransactions(startDate, endDate, poFilter, false, null, null);
            var paidTransactions = SessionVariables.CurrentAccount.GetPaidTransactions(startDate, endDate, poFilter, false, null, null);
            var allTransactions = billedTransactions.Concat(paidTransactions).ToList();

            allTransactions.ForEach(transaction =>
                transaction.CreateDate = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value)
            );
            // sort transactions
            allTransactions = allTransactions.OrderByDescending(x => x.ID).ToList();
            var table = new List<List<string>>();
            var row = new List<string>();
            row.Add("ID");
            row.Add("CreatedOn");
            row.Add("Payer");
            row.Add("For");
            row.Add("Item Description");
            row.Add("Total Amount Paid");
            row.Add("Platform Fee");
            row.Add("Your Amount");
            table.Add(row);
            foreach (var t in allTransactions)
            {
                var registrant = t.GetPurchase()?.GetRegistrant();
                var participationObjects = t.GetParticipationObjects();

                row = new List<string>();
                row.Add(t.GetTransactionNumber());
                row.Add(t.CreateDate.ToShortDateTimeString());
                row.Add(t.GetPayerAccount().Name);
                row.Add(t.IsCharge() && registrant.IsNotEmpty() ? registrant.DisplayName : t.GetBillerAccount().Name);
                row.Add(participationObjects.DistinctBy(po => new { po.ObjectID, po.ObjectType }).Select(po => po.Name).ToStringList(", "));
                row.Add(t.GetBillerAmount().ToCurrency());
                row.Add(t.GetFeesChargedToBiller().ToCurrency());
                row.Add(t.GetBillerPayout().ToCurrency());
                table.Add(row);
            }
            string csv = CsvFile.Create(table);

            Response.Clear();
            Response.AddHeader("Content-Type", BasicLibrary.Utilities.GetMimeType("csv"));
            Response.AddHeader("Content-Disposition", "attachment; filename=transactions.csv");

            byte[] filebytes = System.Text.UTF8Encoding.UTF8.GetBytes(csv);
            Response.AddHeader("Content-Length", filebytes.Length.ToString());
            Response.BinaryWrite(filebytes);
            Response.End();
            return null;
        }

        [HttpGet]
        public ActionResult Subscriptions()
        {
            var msgs = new SystemMessages();
            var plans = SessionVariables.CurrentAccount.GetSubscriptionPlans(msgs);
            var subscriptions = new List<RegistrantSubscriptionPlan>();

            plans.ForEach(plan => {
                subscriptions.AddRange(plan.GetRegistrantSubscriptionPlans(msgs));
            });

            subscriptions = subscriptions.Where(subscription => subscription.CreateDate >= new DateTime(2019, 12, 1))
                                .OrderByDescending(x => x.CreateDate.Value)
                                .ToList();

            ViewBag.Messages = msgs;

            return View(subscriptions);
        }

        [HttpGet]
        public ActionResult RefundRequests()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AmountAvailableToRefund = SessionVariables.CurrentAccount.GetMaxAmountAvailableForPayout();
            return View(SessionVariables.CurrentAccount.GetRefundRequests().OrderByDescending(request => request.CreateDate).ToList());
        }

        [HttpGet]
        public ActionResult ProcessManualRefund(string uid, string amount, string reason)
        {
            var msgs = new SystemMessages();

            if (uid.IsGuid() && amount.IsNumber())
            {
                var transaction = Transaction.LoadByUid(uid.ToGuid(), msgs);

                if (transaction.IsNotEmpty())
                    transaction.ManualRefund(amount.ToDecimal(), SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult RequestRefund(string uid, string amount, string reason)
        {
            var msgs = new SystemMessages();

            if (uid.IsGuid() && amount.IsNumber())
            {
                var transaction = Transaction.LoadByUid(uid.ToGuid(), msgs);

                if (transaction.IsNotEmpty())
                    transaction.RequestRefund(amount.ToDecimal(), RefundRequestStatus.RequestedByProvider, reason, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ApproveRefundRequest(string uid)
        {
            var msgs = new SystemMessages();

            if (uid.IsGuid())
            {
                var request = RefundRequest.LoadByUid(uid.ToGuid(), msgs);

                if (request.IsNotEmpty())
                    request.ApproveRequestAsProvider(null, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ApproveManualRefundRequest(string uid)
        {
            var msgs = new SystemMessages();

            if (uid.IsGuid())
            {
                var request = RefundRequest.LoadByUid(uid.ToGuid(), msgs);

                if (request.IsNotEmpty())
                    request.UpdateStatus(RefundRequestStatus.ApprovedManualRefund, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult DenyRefundRequest(string uid, string reason)
        {
            var msgs = new SystemMessages();

            if (uid.IsGuid())
            {
                var request = RefundRequest.LoadByUid(uid.ToGuid(), msgs);

                if (request.IsNotEmpty())
                    request.DenyRequestAsProvider(reason, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}