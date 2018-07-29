using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AfterSchool.Areas.StripeHooks.Controllers
{
    public class AccountController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Updated([FromBody] dynamic data)
        {
            var msgs = new SystemMessages();
            try
            {                
                var serialized = JsonConvert.SerializeObject(data);
                var stripeEvent = StripeEventUtility.ParseEvent(serialized);
                var stripeAccountId = (string)stripeEvent.Data.Object.id;
                var account = Account.LoadByStripeUserId(stripeAccountId);
                var stripeAccount = account.GetStripeAccount(msgs);
                var serializedAccountJson = JsonConvert.SerializeObject(stripeEvent.Data.Object);

                msgs.Add("Updating Parse and Update", MessageType.Notice);
                stripeAccount.ParseAndUpdateStripeCustomAccount((string)serializedAccountJson, null, msgs);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok();
        }
    }
}
