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
            var groupedTransactions = new List<(ParticipationObject ParticipationObject, decimal? BillerAmount, decimal? FeesChargedToBiller, decimal? BillerPayout)>();

            if (id.IsGuid())
            {
                var msgs = new SystemMessages();
                var payout = Payout.LoadByUid(id.ToGuid(), msgs);
                var transactions = payout.GetTransactions(msgs);

                transactions.ForEach(transaction => {

                    var participationObject = transaction.GetParticipationObjects(msgs).First();
                    var groupedTransaction = groupedTransactions.Find(item => item.ParticipationObject.ObjectID == participationObject.ObjectID);

                    if (groupedTransaction.IsNotEmpty() && groupedTransaction.ParticipationObject.IsNotEmpty())
                    {
                        groupedTransaction.BillerAmount += transaction.GetBillerAmount();
                        groupedTransaction.FeesChargedToBiller += transaction.GetFeesChargedToBiller();
                        groupedTransaction.BillerPayout += transaction.GetBillerPayout();
                    }
                    else
                        groupedTransactions.Add((
                            participationObject, 
                            transaction.GetBillerAmount(), 
                            transaction.GetFeesChargedToBiller(), 
                            transaction.GetBillerPayout()));

                });

                ViewBag.TotalPayout = transactions.Sum(p => p.GetBillerPayout()).ToCurrency();
            }
            return View(groupedTransactions);
        }
    }
}