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
            //http://localhost:54849/Lob/Payout/attachment/F6CA8D1B-3D0C-4685-B069-DEA9C4F2C9ED
            if (id.IsGuid())
            {
                var msgs = new SystemMessages();
                var payout = Payout.LoadByUid(id.ToGuid(), msgs);
                //var transactions = payout.GetTransactions(msgs);


            }

            return View();
        }
    }
}