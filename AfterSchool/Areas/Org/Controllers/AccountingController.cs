using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using BasicLibrary;
using System.Linq;
using System.Web.Helpers;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using AfterSchoolLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountingController : BaseLoggedInController
    {
        public AccountingController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        // GET: Org/Accounting
        [HttpGet]
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
            var totalBilledTransactionCount = SessionVariables.CurrentAccount.GetBilledTransactionCount(startDate, endDate);
            var totalPaidTransactionCount = SessionVariables.CurrentAccount.GetPaidTransactionCount(startDate, endDate);
            var totalTransactionCount = totalBilledTransactionCount + totalPaidTransactionCount;

            // transaction data
            var billedTransactions = SessionVariables.CurrentAccount.GetBilledTransactions(startDate, endDate, start, length);
            var paidTransactions = SessionVariables.CurrentAccount.GetPaidTransactions(startDate, endDate, start, length);
            var allTransactions = billedTransactions.Concat(paidTransactions).ToList();

            var totalPages = Math.Ceiling(totalTransactionCount.ToDouble() / recordsPerPage).ToInteger();

            allTransactions.ForEach(transaction =>
                transaction.CreateDate = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value)
            );

            // sort transactions
            allTransactions = allTransactions.OrderByDescending(x => x.ID).ToList();

            ViewBag.TotalAmountPaid = SessionVariables.CurrentAccount.GetTotalAmountBilledOut(startDate, endDate, msgs);
            ViewBag.TotalAmountRefunded = SessionVariables.CurrentAccount.GetTotalAmountRefundedTo(startDate, endDate, msgs);
            ViewBag.TotalFeesPaid = SessionVariables.CurrentAccount.GetTotalFeesPaidAsBiller(startDate, endDate, msgs);
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
            ViewBag.MaxRequestAmount = SessionVariables.CurrentAccount.GetMaxAmountAvailableForPayout(msgs);
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

        [HttpPost]
        public ActionResult RequestPayout()
        {
            var transactionIds = Request.Form["TransactionIds"];
            var msgs = new SystemMessages();
            if (!string.IsNullOrEmpty(transactionIds))
            {
                var ids = transactionIds.Split(',').Select(p => Convert.ToInt32(p)).ToList();
                var transactions = Transaction.LoadList(ids);
                ViewBag.TranIds = transactionIds;
                ViewBag.TotalPayoutAmount = transactions.Sum(p => p.GetBillerPayout()).ToCurrency();
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
            if (!string.IsNullOrEmpty(transactionIds))
            {
                var msgs = new SystemMessages();
                var ids = transactionIds.Split(',').Select(p => Convert.ToInt32(p)).ToList();
                transactions = Transaction.LoadList(ids).ToList();
                ViewBag.TotalAmount = transactions.Sum(p => p.GetBillerPayout()).ToCurrency();
                SessionVariables.CurrentAccount.RequestNewPayout(transactions, loggedInUser: SessionVariables.CurrentUser, msgs);
                if (msgs.HasNoErrors())
                    return View();
            }
            return View("RequestPayout", transactions);
        }

        public ActionResult DownloadCSV(string sdate, string edate)
        {
            var startDate = sdate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) :
                new DateTime?();
            var endDate = edate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) :
                new DateTime?();
            // transaction data
            var billedTransactions = SessionVariables.CurrentAccount.GetBilledTransactions(startDate, endDate, null, null);
            var paidTransactions = SessionVariables.CurrentAccount.GetPaidTransactions(startDate, endDate, null, null);
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
                var purchase = t.GetPurchase();
                var registrant = Registrant.Load(purchase.RegistrantId.Value);
                var participationObjects = t.GetParticipationObjects();

                row = new List<string>();
                row.Add(t.GetTransactionNumber());
                row.Add(t.CreateDate.ToShortDateTimeString());
                row.Add(t.GetPayerAccount().Name);
                row.Add(t.IsCharge() ? registrant.DisplayName : t.GetBillerAccount().Name);
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

        //[HttpPost]
        //public ActionResult Refund(string uid, string amount)
        //{
        //    var msgs = new SystemMessages();
        //    var originalTransaction = Transaction.LoadByUid(uid.ToGuid());
        //    var loggedInUser = SessionVariables.LoggedInUser;

        //    if (originalTransaction == null)
        //        msgs.Add("Invalid transaction", MessageType.UserError);
        //    else
        //    {
        //        var totalRefund = Transaction.GetTotalRefundByOriginalTransaction(originalTransaction.ID.Value);
        //        var maximumRefund = originalTransaction.Amount + totalRefund;

        //        if (amount.ToDecimal() > maximumRefund)
        //            msgs.Add($"The maximum amount to refund is {maximumRefund.ToCurrency()}", MessageType.UserError);
        //    }

        //    if (amount.ToDouble() < 0.1)
        //        msgs.Add("The minimum amount to refund is $0.10", MessageType.UserError);

        //    if (msgs.HasNoErrors())
        //    {
        //        var newTransaction = AccountService.CreateRefundTransactionPayment(
        //            originalTransaction,
        //            amount.ToDecimal(),
        //            loggedInUser,
        //            msgs
        //        );

        //        if (newTransaction != null)
        //        {
        //            var transaction = new
        //            {
        //                id = newTransaction.GetTransactionNumber(),
        //                originalTransactionUid = originalTransaction.Uid,
        //                createdOn = newTransaction.CreateDate.ToShortDateTimeString(),
        //                payer = newTransaction.GetPayerAccount().Name,
        //                refundFor = newTransaction.GetBillerAccount().Name,
        //                itemDescription = newTransaction.GetParticipationObjects().DistinctBy(po => new { po.ObjectID, po.ObjectType }).Select(po => po.Name).ToStringList(", "),
        //                totalPaid = newTransaction.Amount,
        //                platformFee = newTransaction.GetPlatformFee(),
        //                payout = newTransaction.GetPayout(),
        //            };

        //            return new JsonResult { Data = new { status = "ok", transaction, refundedAmount = originalTransaction.GetRefundedAmount() } };
        //        }
        //        else
        //            msgs.Add("Error when processing the refund", MessageType.Error);
        //    }

        //    return new JsonResult { Data = new { status = "error", messages = msgs } };
        //}
    }
}