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
            ViewBag.Accounts = Account.Load(msgs: msgs);
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
        public ActionResult RefundRequests(string pn, string rpp, string sdate, string edate)
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

            var requestCount = RefundRequest.GetCount(startDate, endDate, msgs);
            var requests = RefundRequest.LoadByDate(startDate, endDate, start, length, msgs);

            var totalPages = Math.Ceiling(requestCount.ToDouble() / recordsPerPage).ToInteger();

            requests.ForEach(request => {
                if (SessionVariables.CurrentAccount.TimeZone.IsNotEmpty())
                    request.CreateDate = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(request.CreateDate.Value);
            });

            ViewBag.TotalAmountPaid = Transaction.GetTotalAmountBilledOut(startDate, endDate, false, msgs);
            ViewBag.TotalFeesPaid = Transaction.GetTotalFeesPaid(startDate, endDate, false, msgs);
            ViewBag.TotalProfit = Transaction.GetTotalAHQProfit(startDate, endDate, false, msgs);
            ViewBag.TotalRefunded = Transaction.GetTotalAmountRefunded(startDate, endDate, false, msgs);
            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = requestCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(requests);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var msgs = new SystemMessages();

            if (id.IsGuid())
            {
                var transaction = Transaction.LoadByUid(id.ToGuid(), msgs);

                if (transaction.IsNotEmpty())
                    transaction.MarkAsDeleted(SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CreateTransfer(string id)
        {
            var msgs = new SystemMessages();

            if (id.IsGuid())
            {
                var currentUser = SessionVariables.CurrentUser;
                var transaction = Transaction.LoadByUid(id.ToGuid(), msgs);

                if (transaction.IsNotEmpty())
                {
                    var transfer = transaction.CreateStripeTransfer(currentUser, msgs);

                    if (msgs.HasNoErrors() && transfer.IsNotEmpty())
                        return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { status = "error", messages = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(Guid accountUid, decimal amount, int transactionType, string note)
        {
            var msgs = new SystemMessages();
            var account = Account.LoadByUid(accountUid, msgs);

            if (account.IsNotEmpty())
            {
                var transactionTypeId = (TransactionType)transactionType;
                var ahqAccount = AccountService.GetAfterSchoolAccount(msgs);
                Account fromAccount = null;
                Account toAccount = null;

                if (transactionTypeId == TransactionType.Debit)
                {
                    fromAccount = account;
                    toAccount = ahqAccount;
                }
                else if (transactionTypeId == TransactionType.Credit)
                {
                    toAccount = account;
                    fromAccount = ahqAccount;
                }

                TransactionService.CreateNew(
                        fromAccount,
                        toAccount,
                        amount,
                        note,
                        transactionTypeId,
                        PaymentSourceType.ManualAfterSchoolHQ,
                        false,
                        SessionVariables.CurrentUser,
                        msgs);

                return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult Refund(string id, string request, string refundcc, string refundcf, PaymentSourceType paymentsource)
        {
            var msgs = new SystemMessages();

            if (id.IsGuid())
            {
                var transaction = Transaction.LoadByUid(id.ToGuid(), msgs);
                Transaction refund = new Transaction();
                
                if (request.IsNotEmpty())
                {
                    if (request.IsGuid())
                    {
                        var refundRequest = RefundRequest.LoadByUid(request.ToGuid(), msgs);
                        var refundCreditFees = refundcc.IsBoolean() && refundcc.ToBoolean();
                        var refundConvenienceFees = refundcf.IsBoolean() && refundcf.ToBoolean();

                        if (refundRequest.IsNotEmpty() && refundRequest.CanRefundAmountRequested(msgs))
                            refund = refundRequest.ApproveAndProcessRequest(
                                true, 
                                refundCreditFees, 
                                refundConvenienceFees,
                                paymentsource,
                                SessionVariables.CurrentUser, 
                                msgs);
                    }
                    else
                        msgs.Add("Invalid Request", MessageType.UserError);
                }
                else
                {
                    if (transaction.IsNotEmpty() && !transaction.HasRefunds())
                        refund = transaction.RefundPayerFullAmount(SessionVariables.CurrentUser, msgs);
                    else
                        msgs.Add("Invalid Request", MessageType.UserError);
                }

                if (msgs.HasNoErrors())
                {
                    SessionVariables.WebSession.LogRefundCompleted(SessionVariables.CurrentAccount, SessionVariables.CurrentUser, WebSessionActionType.Refunded, refund.Amount.ToCurrency(), transaction, msgs);
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
                }
                    
            }


            return Json(new { status = "error", messages = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DenyRefundRequest(string uid)
        {
            var msgs = new SystemMessages();

            if (uid.IsGuid())
            {
                var request = RefundRequest.LoadByUid(uid.ToGuid(), msgs);

                if (request.IsNotEmpty())
                    request.DenyRequestAsAHQ(null, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    SessionVariables.WebSession.LogRefundRequestUpdate(SessionVariables.CurrentAccount, SessionVariables.CurrentUser, WebSessionActionType.Denied, request.Amount.ToCurrency(), request, msgs);
                    return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }


            }

            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult MoveTransaction(string id, string toaccount)
        {
            var msgs = new SystemMessages();

            if (!id.IsGuid() || !toaccount.IsGuid())
                msgs.Add("Invalid data", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var transaction = Transaction.LoadByUid(id.ToGuid(), msgs);
                var toAccount = Account.LoadByUid(toaccount.ToGuid(), msgs);

                if (transaction.IsNotEmpty() && toaccount.IsNotEmpty())
                    transaction.MoveToAccount(toAccount, msgs);
                else
                    msgs.Add("Invalid data", MessageType.UserError);

                if (msgs.HasNoErrors())
                    return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }


            return new JsonResult { Data = new { status = "error", messages = msgs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}