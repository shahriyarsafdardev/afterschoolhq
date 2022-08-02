using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Account = AfterSchoolLibrary.Models.Account;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class StripeSubscriberController: ApiController
    {   
        [HttpPost]
        public IHttpActionResult Index([FromBody] dynamic json)
        {
            var msgs = new SystemMessages();
            try
            {
                msgs.Add("Getting serializedJson", MessageType.Notice);
                var serializedJson = JsonConvert.SerializeObject(json);
                msgs.Add("Getting stripeEvent", MessageType.Notice);
                var stripeEvent = (Stripe.Event)Stripe.EventUtility.ParseEvent(serializedJson, throwOnApiVersionMismatch: false);

                switch (stripeEvent.Type)
                {
                    default:
                        return ReturnOK(msgs);
                }
            }
            catch (Exception ex)
            {
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                return Return500(msgs);
            }
        }
        
        private void SendLogEmail(SystemMessages msgs)
        {
            string toEmail = "dev@afterschoolhq.com";
            string subject = $"Stripe Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
            string body = msgs.RenderMessageList(MessageOutputType.Html);

            AfterSchoolLibrary.Helpers.EmailHelper.SendEmail(toEmail, subject, body, true);
        }

        private IHttpActionResult ReturnOK(SystemMessages msgs)
        {
            return Ok(msgs.ToJSON());
        }

        private IHttpActionResult Return500(SystemMessages msgs)
        {
            SendLogEmail(msgs);
            return InternalServerError(new Exception(msgs.RenderMessageList(MessageOutputType.Text)));
        }
    }
}