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