using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountingController : BaseLoggedInController
    {
        public AccountingController() : base(FormConstants.OrganizationAccountTypes)
        {
        }

        // GET: Org/Accounting
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string sdate, string edate, string poId, string poTypeId, string q)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var pageNumber = 1;
            var recordsPerPage = 25;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var startDate = sdate.IsDateTime() ? currentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) : new DateTime?();
            var endDate = edate.IsDateTime() ? currentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) : new DateTime?();

            //PO filters
            List<(int ID, ParticipationObjectType ObjectType)> poFilter = null;
            if (!string.IsNullOrEmpty(poId) && !string.IsNullOrEmpty(poTypeId))
            {
                poFilter = new List<(int ID, ParticipationObjectType ObjectType)>();
                poFilter.Add((poId.ToInteger(), (ParticipationObjectType) poTypeId.ToInteger()));
            }

            var totalTransactionCount = currentAccount.GetTransactionCount(q, startDate, endDate, poFilter, false, msgs);
            var allTransactions = currentAccount.GetTransactions(q, startDate, endDate, poFilter, false, start, length, msgs);
            var totalPages = Math.Ceiling(totalTransactionCount.ToDouble() / recordsPerPage).ToInteger();

            allTransactions.ForEach(transaction =>
                transaction.CreateDate = currentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value)
            );

            var allPos = currentAccount.GetParticipationObjectsByBillerAccountId(null, null, false, msgs);
            var allParticipationObjects = allPos.DistinctBy(po => (po.ID, po.ParticipationObjectTypeId, po.Name));

            ViewBag.TotalAmountPaid = currentAccount.GetTotalAmountBilledOut(q, null, null, startDate, endDate, msgs);
            ViewBag.TotalAmountRefunded = currentAccount.GetTotalAmountRefunded(q, startDate, endDate, msgs);
            ViewBag.TotalAmountRefundedManual = currentAccount.GetTotalAmountRefundedManually(q, startDate, endDate, msgs);
            ViewBag.TotalAmountRefundedAHQ = currentAccount.GetTotalAmountRefundedAHQ(q, true, startDate, endDate, msgs);
            ViewBag.TotalFeesPaid = currentAccount.GetTotalFeesPaidAsBiller(q, null, startDate, endDate, msgs);
            ViewBag.TotalPaidOut = currentAccount.GetTotalPayoutAmount(q, PayoutStatus.Completed, startDate, endDate, msgs);
            ViewBag.TotalAvailableForPayout = currentAccount.GetMaxAmountAvailableForPayout(q, startDate, endDate, msgs);
            ViewBag.ParticipationObjects = allParticipationObjects;
            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalTransactionCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            return View(allTransactions);
        }

        public ActionResult DownloadCSV(string sdate, string edate, string poId, string poTypeId, string q)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var startDate = sdate.IsDateTime() ? currentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) : new DateTime?();
            var endDate = edate.IsDateTime() ? currentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) : new DateTime?();

            //PO filters
            List<(int ID, ParticipationObjectType ObjectType)> poFilter = null;
            if (!string.IsNullOrEmpty(poId) && !string.IsNullOrEmpty(poTypeId))
            {
                poFilter = new List<(int ID, ParticipationObjectType ObjectType)>();
                poFilter.Add((poId.ToInteger(), (ParticipationObjectType) poTypeId.ToInteger()));
            }

            // transaction data
            var billedTransactions = currentAccount.GetBilledTransactions(q, startDate, endDate, poFilter, false, null, null, msgs);
            var paidTransactions = currentAccount.GetPaidTransactions(q, startDate, endDate, poFilter, false, null, null, msgs);
            var allTransactions = billedTransactions.Concat(paidTransactions).ToList();

            allTransactions.ForEach(transaction =>
                transaction.CreateDate = currentAccount.ConvertToAccountTimeZone(transaction.CreateDate.Value)
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
            row.Add("Credit Card Fee");
            row.Add("Your Amount");
            row.Add("Status");
            row.Add("Type");

            table.Add(row);

            foreach (var t in allTransactions)
            {
                var registrant = t.GetPurchase(msgs)?.GetRegistrant(msgs);
                var participationObjects = t.GetParticipationObjects(msgs);
                var description = participationObjects.DistinctBy(po => new {po.ObjectID, po.ObjectType}).Select(po => po.Name).ToStringList(", ");

                row = new List<string>();
                row.Add(t.GetTransactionNumber()); // ID
                row.Add(t.CreateDate.ToShortDateTimeString()); // CreatedOn
                row.Add(t.GetPayerAccount(msgs).Name); // Payer
                row.Add(t.IsCharge() && registrant.IsNotEmpty() ? registrant.DisplayName : t.GetBillerAccount(msgs).Name); // For
                row.Add(description.IsNotEmpty() ? description : t.Description); // Item Description
                row.Add(t.GetPayerAmount().ToCurrency()); // Total Amount Paid
                row.Add(t.ConvenienceFeeCharged.ToCurrency()); // Platform Fee
                row.Add(t.PaymentTypeFeeCharged.ToCurrency()); // Credit Card Fee
                row.Add(t.GetBillerPayout(msgs).ToCurrency()); // Your Amount
                row.Add(t.GetStatus(msgs)); // Status
                row.Add(t.PaymentTypeId.ToString().SplitByUpperCase()); // Type
                table.Add(row);
            }

            ViewBag.Messages = msgs;

            string csv = CsvFile.Create(table);

            Response.Clear();
            Response.AddHeader("Content-Type", Utilities.GetMimeType("csv"));
            Response.AddHeader("Content-Disposition", "attachment; filename=transactions.csv");

            byte[] filebytes = UTF8Encoding.UTF8.GetBytes(csv);
            Response.AddHeader("Content-Length", filebytes.Length.ToString());
            Response.BinaryWrite(filebytes);
            Response.End();

            return null;
        }

        [HttpGet]
        public ActionResult CreateTransfer(string id)
        {
            var msgs = new SystemMessages();

            if (id.IsGuid() && SessionVariables.AccountUserType == UserType.TechSupport)
            {
                var currentAccount = SessionVariables.CurrentAccount;
                var currentUser = SessionVariables.CurrentUser;
                var transaction = currentAccount.GetBillerTransaction(id.ToGuid(), msgs);

                if (transaction.IsNotEmpty())
                {
                    var transfer = transaction.CreateStripeTransfer(currentUser, msgs);

                    if (msgs.HasNoErrors() && transfer.IsNotEmpty())
                        return Json(new {status = "ok"}, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new {status = "error", messages = msgs.ToList().Select(msg => new {msg.Text})}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStripeLoginLink(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            var loginLink = StripeHelper.GetStripeHelperConnectAccount().CreateLoginLink(currentAccount.StripeUserId, msgs);

            if (loginLink.IsNotEmpty())
                return Json(new {status = "ok", link = loginLink.Url}, JsonRequestBehavior.AllowGet);

            return Json(new {status = "error", messages = msgs.ToList().Select(msg => new {msg.Text})}, JsonRequestBehavior.AllowGet);
        }

        #region Payouts

        [HttpGet]
        public ActionResult Payouts(string pn, string rpp, string sdate, string edate)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var pageNumber = 1;
            var recordsPerPage = 25;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var startDate = sdate.IsDateTime() ? currentAccount.ConvertFromAccountTimeZone(sdate.ToDateTime()) : new DateTime?();
            var endDate = edate.IsDateTime() ? currentAccount.ConvertFromAccountTimeZone(edate.ToDateTime().AddDays(1).AddMilliseconds(-1)) : new DateTime?();

            var totalPayoutAmount = currentAccount.GetTotalPayoutAmount(null, PayoutStatus.Completed, startDate, endDate, msgs);
            var totalPayoutCount = currentAccount.GetPayoutCount(null, startDate, endDate, msgs);
            var payouts = currentAccount.GetPayouts(null, startDate, endDate, start, length, msgs);
            var totalPages = Math.Ceiling(totalPayoutCount.ToDouble() / recordsPerPage).ToInteger();

            payouts.ForEach(payout =>
                payout.DateRequested = currentAccount.ConvertToAccountTimeZone(payout.DateRequested.Value)
            );

            // sort transactions
            payouts = payouts.OrderByDescending(x => x.ID).ToList();

            ViewBag.CanReceiveACHDeposits = currentAccount.CanReceiveACHDeposits(msgs);
            ViewBag.CanReceiveDeposits = currentAccount.CanReceivePaymentDeposits(msgs);
            ViewBag.MaxRequestAmount = currentAccount.GetMaxAmountAvailableForPayout(search: null, msgs: msgs);
            ViewBag.LastPayout = currentAccount.GetLastPayout(msgs);
            ViewBag.TotalPayoutAmount = totalPayoutAmount;
            ViewBag.TotalPendingAmount = currentAccount.GetTotalPayoutAmount(null, PayoutStatus.InProgress, msgs: msgs);
            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalPayoutCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            return View(payouts);
        }

        [HttpGet]
        public ActionResult PayoutTransactions(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var transactions = new List<Transaction>();

            if (id.IsGuid())
            {
                var msgs = new SystemMessages();
                var payout = currentAccount.GetPayout(id.ToGuid(), msgs);
                transactions = payout.GetTransactions(msgs);

                ViewBag.TotalPayout = transactions.Sum(p => p.GetBillerPayout(msgs)).ToCurrency();
            }

            return View(transactions);
        }

        [HttpPost]
        public ActionResult RequestPayout()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var transactionIds = Request.Form["TransactionIds"];
            var msgs = new SystemMessages();

            if (!string.IsNullOrEmpty(transactionIds))
            {
                var ids = transactionIds.Split(',').Select(p => Convert.ToInt32(p)).ToList();
                var transactions = currentAccount.GetBillerAccountTransactions(ids, msgs);

                ViewBag.TranIds = transactionIds;
                ViewBag.TotalPayoutAmount = transactions.Sum(transaction => transaction.GetBillerPayout(msgs));
                ViewBag.MaxPayoutAvailable = currentAccount.GetMaxAmountAvailableForPayout(search: null, msgs: msgs);
                ViewBag.Messages = msgs;

                return View(transactions);
            }

            return null;
        }

        [HttpPost]
        public ActionResult RequestPayoutDone()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var transactionIds = Request.Form["TransactionIds"];
            var transactions = new List<Transaction>();
            var msgs = new SystemMessages();

            if (!string.IsNullOrEmpty(transactionIds))
            {
                var ids = transactionIds.Split(',').Select(p => p.ToInteger()).ToList();

                transactions = currentAccount.GetBillerAccountTransactions(ids, msgs);
                currentAccount.RequestNewPayout(null, transactions, SessionVariables.CurrentUser, msgs);

                ViewBag.TotalAmount = transactions.Sum(p => p.GetBillerPayout(msgs)).ToCurrency();
                ViewBag.TranIds = transactionIds;
                ViewBag.TotalPayoutAmount = ViewBag.TotalAmount;
                ViewBag.Messages = msgs;

                if (msgs.HasNoErrors())
                    return View();
            }

            ViewBag.Messages = msgs;

            return View("RequestPayout", transactions);
        }

        #endregion

        #region Subscriptions

        [HttpGet]
        public ActionResult Subscriptions()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var plans = currentAccount.GetSubscriptionPlans(msgs);
            var subscriptions = new List<RegistrantSubscriptionPlan>();

            var listsOfRegistrantSubscriptionPlan = plans.Select(plan => plan.GetRegistrantSubscriptionPlans(msgs)).ToList();
            listsOfRegistrantSubscriptionPlan.ForEach(registrantSubscriptionPlan => subscriptions.AddRange(registrantSubscriptionPlan));

            subscriptions = subscriptions.Where(subscription => subscription.CreateDate >= new DateTime(2019, 12, 1))
                .OrderByDescending(x => x.CreateDate.Value)
                .ToList();

            ViewBag.Messages = msgs;

            return View(subscriptions);
        }

        [HttpGet]
        public ActionResult WithdrawAndCancel(string id, string po, string type, string returnurl)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (po.IsEmpty() || id.IsEmpty() || type.IsEmpty())
                msgs.Add("Validation error", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var registrant = currentAccount.GetRegistrant(id.ToGuid(), msgs);

                if (registrant.IsNotEmpty())
                {
                    registrant.WithdrawRegistration(
                        participationObjectId: po.ToInteger(),
                        participationObjectType: (ParticipationObjectType) type.ToInteger(),
                        loggedInUser: SessionVariables.CurrentUser,
                        isCancelPayment: true,
                        isParent: false,
                        account: currentAccount,
                        msgs: msgs);
                }
                else
                    msgs.Add("Registrant not found", MessageType.UserError);
                
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors() && returnurl.IsNotBlank())
                return Redirect(returnurl);

            return RedirectToAction("Subscriptions");
        }

        #endregion

        #region Refunds

        [HttpGet]
        public ActionResult RefundRequests()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            ViewBag.AmountAvailableToRefund = currentAccount.GetMaxAmountAvailableForRefund(search: null, msgs: msgs);
            ViewBag.AmountRefunded = currentAccount.GetTotalAmountRefunded(search: null, msgs: msgs);

            var model = currentAccount.GetRefundRequests(msgs).OrderByDescending(request => request.CreateDate).ToList();

            ViewBag.Messages = msgs;

            return View(model);
        }

        [HttpGet]
        public ActionResult ProcessManualRefund(string uid, string amount, string reason)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (uid.IsGuid() && amount.IsNumber())
            {
                var transaction = currentAccount.GetBillerTransaction(uid.ToGuid(), msgs);

                if (transaction.IsNotEmpty())
                    transaction.ManualRefund(amount.ToDecimal(), SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    SessionVariables.WebSession.LogRefundCompleted(currentAccount, SessionVariables.CurrentUser, WebSessionActionType.ManuallyRefunded, amount,
                        transaction, msgs);
                    return new JsonResult {Data = new {status = "ok"}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
                }
            }

            return new JsonResult {Data = new {status = "error", messages = msgs}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }


        [HttpPost]
        public ActionResult RequestRefund(string uid, string amount, string reason)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (uid.IsGuid() && amount.IsNumber())
            {
                var transaction = currentAccount.GetBillerTransaction(uid.ToGuid(), msgs);
                RefundRequest refundRequest = new RefundRequest();

                if (transaction.IsNotEmpty())
                    refundRequest = transaction.RequestRefund(amount.ToDecimal(), RefundRequestStatus.RequestedByProvider, reason, SessionVariables.CurrentUser,
                        msgs);

                if (msgs.HasNoErrors() && refundRequest.IsNotEmpty())
                {
                    SessionVariables.WebSession.LogRefundRequestUpdate(currentAccount, SessionVariables.CurrentUser, WebSessionActionType.Requested,
                        refundRequest.Amount.ToCurrency(), refundRequest, msgs);
                    return new JsonResult {Data = new {status = "ok"}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
                }
            }

            return new JsonResult {Data = new {status = "error", messages = msgs}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpGet]
        public ActionResult ApproveRefundRequest(string uid)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (uid.IsGuid())
            {
                var request = currentAccount.GetRefundRequest(uid.ToGuid(), msgs);

                if (request.IsNotEmpty())
                    request.ApproveRequestAsProvider(null, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    SessionVariables.WebSession.LogRefundRequestUpdate(SessionVariables.CurrentAccount, SessionVariables.CurrentUser,
                        WebSessionActionType.Approved, request.Amount.ToCurrency(), request, msgs);
                    return new JsonResult {Data = new {status = "ok"}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
                }
            }

            return new JsonResult {Data = new {status = "error", messages = msgs}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpGet]
        public ActionResult ApproveManualRefundRequest(string uid)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (uid.IsGuid())
            {
                var request = currentAccount.GetRefundRequest(uid.ToGuid(), msgs);

                if (request.IsNotEmpty())
                    request.UpdateStatus(RefundRequestStatus.ApprovedManualRefund, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    SessionVariables.WebSession.LogRefundRequestUpdate(SessionVariables.CurrentAccount, SessionVariables.CurrentUser,
                        WebSessionActionType.Approved, request.Amount.ToCurrency(), request, msgs);
                    return new JsonResult {Data = new {status = "ok"}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
                }
            }

            return new JsonResult {Data = new {status = "error", messages = msgs}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        public ActionResult DenyRefundRequest(string uid, string reason)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (uid.IsGuid())
            {
                var request = currentAccount.GetRefundRequest(uid.ToGuid(), msgs);

                if (request.IsNotEmpty())
                    request.DenyRequestAsProvider(reason, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    SessionVariables.WebSession.LogRefundRequestUpdate(SessionVariables.CurrentAccount, SessionVariables.CurrentUser,
                        WebSessionActionType.Denied, request.Amount.ToCurrency(), request, msgs);
                    return new JsonResult {Data = new {status = "ok"}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
                }
            }

            return new JsonResult {Data = new {status = "error", messages = msgs}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        #endregion

        #region Coupons

        // GET: Org/Accounting/Coupon
        [HttpGet]
        public ActionResult Coupon()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var coupons = currentAccount.GetCoupons(msgs);

            ViewBag.Messages = msgs;

            return View(coupons);
        }

        [HttpGet]
        public ActionResult EditCoupon(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            Coupon coupon = null;

            if (id.IsNotBlank() && id.IsGuid())
            {
                coupon = currentAccount.GetCoupon(id.ToGuid(), msgs);
                ViewBag.Activities = GetActivityIdsForCoupon(coupon.ID.Value);
            }
            else
            {
                coupon = CouponService.CreateBlank(currentAccount, SessionVariables.CurrentUser);
            }

            ViewBag.Messages = msgs;
            return View(coupon);
        }

        public static string GetActivityIdsForCoupon(int id)
        {
            var purchaseableObjectCoupons = PurchaseableObjectCoupon.LoadByCouponId(id);
            if (purchaseableObjectCoupons.Any())
                return string.Join(",",
                    purchaseableObjectCoupons.Where(m => m.PurchaseableObjectTypeId == PurchaseableObjectType.Activity).Select(m => m.PurchaseableObjectId)
                        .Distinct());

            return null;
        }

        [HttpPost]
        public virtual JsonResult GetActivities()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            try
            {
                var activities = currentAccount.GetActivities(msgs)?
                    .Where(activity => activity.IsOpenOrUpcoming(msgs) && !activity.IsFree && !activity.HasPostPayPlan(msgs) &&
                                       !activity.HasSubscriptionPlan(msgs))
                    .Select(m => new ModalViewModel()
                    {
                        ID = m.ID.Value.ToString(),
                        Title = m.Name,
                    }).ToList();

                if (msgs.HasNoErrors())
                    return Json(new {success = true, status = "ok", data = activities}, JsonRequestBehavior.AllowGet);
                else
                    return Json(new {success = false, status = "error", data = msgs.ToList().Select(msg => new {msg.Text})}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {success = false, status = "error", data = ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveCoupon(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            Coupon coupon = null;

            if (id.IsNotBlank() && id.IsGuid())
                coupon = currentAccount.GetCoupon(id.ToGuid(), msgs);

            if (coupon.IsEmpty())
                coupon = CouponService.CreateBlank(currentAccount, SessionVariables.LoggedInUser);

            var name = Request.Form["name"];
            var description = Request.Form["description"];
            var couponCode = Request.Form["couponCode"];
            var maxUsesAll = Request.Form["maxUsesAll"];
            var maxUsesPerPerson = Request.Form["maxUsesPerPerson"];
            var expirationDate = Request.Form["expirationDate"];
            var amount = Request.Form["amount"];
            var amountType = Request.Form["amountType"];
            var activityIds = Request.Form["activityIds"];
            var applyType = Request.Form["applyType"];

            //..applyType applies to the registration fee or the subscription, payment plan, post pay plan.  but for now we are just having coupons be just for registration fees
            applyType = Convert.ToString((int) CouponApplyType.RegistrationFee);

            if (name.IsBlank())
                msgs.Add("Name is required", MessageType.UserError);

            // description can be empty
            if (couponCode.IsBlank())
                msgs.Add("Coupon Code Required", MessageType.UserError);

            var hasMaxUses = maxUsesAll.IsNotBlank();
            var hasMaxUsesPerPerson = maxUsesPerPerson.IsNotBlank();
            var hasExpirationDate = expirationDate.IsNotBlank();

            if (hasMaxUses && !maxUsesAll.IsInteger())
                msgs.Add("Number of Uses must be a number", MessageType.UserError);

            if (hasMaxUsesPerPerson && !maxUsesPerPerson.IsInteger())
                msgs.Add("Number of Uses Per Person must be a number", MessageType.UserError);

            if (hasExpirationDate && !expirationDate.IsDate())
                msgs.Add("Please enter a valid Expiration Date", MessageType.UserError);

            if (amount.IsBlank() || !amount.IsNumber())
                msgs.Add("Amount Required, Must be a number greater than 0", MessageType.UserError);

            if (amountType.IsBlank() || Enum.IsDefined(typeof(CouponAmountType), amountType))
                msgs.Add("Amount must be flate rate or percentage", MessageType.UserError);

            if (applyType.IsBlank() || Enum.IsDefined(typeof(CouponApplyType), applyType))
                msgs.Add("Coupon Must have a Fee Type Set", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var ids = new List<int>();
                var loggedInUser = SessionVariables.CurrentUser;

                if (activityIds.IsNotBlank())
                    ids = activityIds.Split(',').Select(p => Convert.ToInt32(p)).ToList();

                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create new coupon
                        coupon = currentAccount.CreateorUpdateCoupon(
                            coupon: coupon,
                            amount: amount.ToDecimal(),
                            expirationDate: hasExpirationDate ? expirationDate.ToDateTime() : new DateTime?(),
                            amountType: amountType.ToInteger().ToEnum<CouponAmountType>(),
                            applyType: applyType.ToInteger().ToEnum<CouponApplyType>(),
                            maxUsesAll: hasMaxUses ? maxUsesAll.ToInteger() : new int?(),
                            maxUsesPerPerson: hasMaxUsesPerPerson ? maxUsesPerPerson.ToInteger() : new int?(),
                            couponCode: couponCode,
                            name: name,
                            description: description.IsBlank() ? "" : description,
                            couponDuration: StripeCouponDuration.Once, // TODO :  figure this out
                            durationInMonths: null,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);

                        //clear previous entries
                        PurchaseableObjectCouponService.ClearPurchaseableObjectCoupons(coupon.ID.Value, amsgs, adc);

                        //add newly selected activities
                        if (ids.Any())
                            PurchaseableObjectCouponService.InsertPurchaseableObjectCoupons(
                                activityIds: ids,
                                CouponId: coupon.ID.Value,
                                loggedInUser: loggedInUser,
                                msgs: amsgs,
                                dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(msgs)
                );
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add("Coupon Saved", MessageType.Success);
                ViewBag.Messages = msgs;
                return RedirectToAction("Coupon");
            }
            else
            {
                ViewBag.Messages = msgs;
                ViewBag.Activities = activityIds;

                return View("EditCoupon", new Coupon()
                {
                    ID = coupon.ID,
                    Uid = coupon.Uid,
                    Name = name,
                    Description = description,
                    MaxNumberOfUsesAll = Convert.ToInt32(maxUsesAll),
                    MaxNumberOfUsesPerPerson = Convert.ToInt32(maxUsesPerPerson),
                    ExpirationDate = Convert.ToDateTime(expirationDate),
                    Amount = Convert.ToDecimal(amount),
                    AmountTypeId = amountType.ToInteger().ToEnum<CouponAmountType>(),
                });
            }
        }

        #endregion
    }
}