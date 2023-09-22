using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using static AfterSchool.Controllers.AWSSubscriptionLambdaController;
using SubscriptionSchedule = AfterSchoolLibrary.Models.SubscriptionSchedule;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class ChargeSubscriptionController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Updated(dynamic json)
        {
            var msgs = new SystemMessages();
            try
            {
                msgs.Add("Getting serializedJson", MessageType.Notice);
                var serializedJson = JsonConvert.SerializeObject(json);
                WebhookPayload webhookPayload = JsonConvert.DeserializeObject<WebhookPayload>(serializedJson);

                var subscriptionPaymentHistory = webhookPayload.PaymentHistory;
                var subscriptionSchedule = webhookPayload.Schedule;

                //msgs.Add("Getting stripeEvent", MessageType.Notice);
                //var stripeEvent = (Stripe.Event) Stripe.EventUtility.ParseEvent(serializedJson, throwOnApiVersionMismatch: false);
                var stripeEvent = "invoice.payment_succeeded";

                switch (stripeEvent)
                {
                    case "invoice.payment_succeeded":
                        msgs.Add("Getting payment_succeeded", MessageType.Notice);
                        return InvoicePaymentSucceeded(subscriptionSchedule, subscriptionPaymentHistory, msgs);
                    
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
        
        private IHttpActionResult InvoicePaymentSucceeded(SubscriptionSchedule subscriptionSchedule, 
            SubscriptionPaymentHistory subscriptionPaymentHistory, SystemMessages msgs)
        {
            //create transaction here

            return Return500(msgs);
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
        private void SendLogEmail(SystemMessages msgs)
        {
            string toEmail = "dev@afterschoolhq.com";
            string subject = $"Stripe Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
            string body = msgs.RenderMessageList(MessageOutputType.Html);

            AfterSchoolLibrary.Helpers.EmailHelper.SendEmail(toEmail, subject, body, true);
        }
    }
}