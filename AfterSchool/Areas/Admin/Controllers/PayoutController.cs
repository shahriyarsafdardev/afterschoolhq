using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class PayoutController : BaseLoggedInController
    {
        public PayoutController() : base(new List<AccountType>() { AccountType.AfterSchool })
        {

        }

        // GET: Admin/Payout
        public ActionResult Index(string pn, string rpp, string sdate, string edate, string ps)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 25;
            PayoutStatus? payoutStatus = null;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (ps.IsNumber())
                payoutStatus = ps.ToInteger().ToEnum<PayoutStatus>();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var startDate = sdate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) :
                new DateTime?();
            var endDate = edate.IsDateTime() ?
                SessionVariables.CurrentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) :
                new DateTime?();

            var totalPayoutAmount = Payout.GetTotalAmount(payoutStatus, startDate, endDate, msgs);
            var totalPayoutCount = Payout.GetCount(payoutStatus, startDate, endDate);
            var payouts = Payout.Load(payoutStatus, startDate, endDate, start, length);
            var totalPages = Math.Ceiling(totalPayoutCount.ToDouble() / recordsPerPage).ToInteger();

            payouts.ForEach(payout =>
                payout.DateRequested = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(payout.DateRequested.Value)
            );

            // sort transactions
            payouts = payouts.OrderByDescending(x => x.ID).ToList();
            
            ViewBag.TotalPayoutAmount = totalPayoutAmount;
            ViewBag.TotalPendingAmount = Payout.GetTotalAmount(PayoutStatus.InProgress, null, null, msgs: msgs);
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
        public ActionResult Approve(string uid)
        {
            var msgs = new SystemMessages();

            if (!uid.IsGuid())
                msgs.Add("Invalid payout", MessageType.UserError);
            
            if (msgs.HasNoErrors())
            {
                var payout = Payout.LoadByUid(uid.ToGuid(), msgs);
                payout.Approve(SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                status = "error",
                messages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Deny(string uid)
        {
            var msgs = new SystemMessages();

            if (!uid.IsGuid())
                msgs.Add("Invalid payout", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var payout = Payout.LoadByUid(uid.ToGuid(), msgs);
                payout.Deny(SessionVariables.CurrentUser, msgs);

                if(msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                status = "error",
                messages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Complete(string uid)
        {
            var msgs = new SystemMessages();

            if (!uid.IsGuid())
                msgs.Add("Invalid payout", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var payout = Payout.LoadByUid(uid.ToGuid(), msgs);
                payout.Complete(SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                status = "error",
                messages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string uid)
        {
            var msgs = new SystemMessages();

            if (!uid.IsGuid())
                msgs.Add("Invalid payout", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var payout = Payout.LoadByUid(uid.ToGuid(), msgs);

                if (payout.IsNotEmpty())
                {
                    var payoutId = payout.ID.Value;

                    msgs.AddRange(payout.Delete());

                    if (msgs.HasNoErrors())
                        Transaction.LoadByPayoutIdByIsDeleted(payoutId, false, msgs).ForEach(transaction => {
                            transaction.PayoutId = null;
                            msgs.AddRange(transaction.Save());
                        });

                    if (msgs.HasNoErrors())
                        return Json(new
                        {
                            status = "ok",
                        }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new
            {
                status = "error",
                messages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetBankInfo(string uid)
        {
            var msgs = new SystemMessages();

            if (!uid.IsGuid())
                msgs.Add("Invalid payout", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var payout = Payout.LoadByUid(uid.ToGuid(), msgs);
                var account = payout.GetAccount(msgs);
                var bankAccount = account.GetBankPaymentAccount(msgs);

                if (msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                        accountHoldersName = bankAccount.AccountHoldersName,
                        accountNumber = bankAccount.AccountNumber,
                        routingNumber = bankAccount.RoutingNumber,
                        bankName = bankAccount.BankName,
                        accountType = bankAccount.BankAccountTypeId.ToString().SplitByUpperCase()
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                status = "error",
                messages = msgs
            }, JsonRequestBehavior.AllowGet);
        }
    }
}