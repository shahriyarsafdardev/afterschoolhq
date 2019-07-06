using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Areas.Lob.Controllers
{
    public class PayoutController : BaseController
    {
        // GET: Lob/Payout
        public ActionResult Attachment(string id)
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
    }
}