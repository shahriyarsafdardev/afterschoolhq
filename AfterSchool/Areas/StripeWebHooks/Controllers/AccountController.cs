using BasicLibrary;
using Stripe;
using System;
using System.Web.Http;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Areas.StripeWebHooks.Controllers
{
    public class AccountController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Updated([FromBody] dynamic data)
        {
            var msgs = new SystemMessages();
            try
            {
                msgs.Add("Getting post data", MessageType.Notice);
                var stripeEvent = StripeEventUtility.ParseEvent((string)data);
                msgs.Add("Getting post data", MessageType.Notice);
                var stripeAccountId = (string)stripeEvent.Data.Object.id;
                msgs.Add("Loading account", MessageType.Notice);
                var account = Account.LoadByStripeUserId(stripeAccountId);
                msgs.Add("Loading stripe account", MessageType.Notice);
                var stripeAccount = account.GetStripeAccount(msgs);
                msgs.Add("Getting stripe json", MessageType.Notice);
                var stripeAccountJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(stripeEvent.Data.Object);

                msgs.Add("Updating Parse and Update", MessageType.Notice);
                stripeAccount.ParseAndUpdateStripeCustomAccount((string)stripeAccountJson, msgs);
            }
            catch(Exception ex)
            {
                throw new Exception(msgs.RenderMessageList(MessageOutputType.Text), ex);
            }

            return Ok();
        }
    }
}
