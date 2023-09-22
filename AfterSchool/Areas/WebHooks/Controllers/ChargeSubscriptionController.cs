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
                msgs.Add("Getting stripeEvent", MessageType.Notice);
                var stripeEvent = (Stripe.Event) Stripe.EventUtility.ParseEvent(serializedJson, throwOnApiVersionMismatch: false);

                switch (stripeEvent.Type)
                {
                    case "invoice.payment_succeeded":
                        msgs.Add("Getting payment_succeeded", MessageType.Notice);
                        return InvoicePaymentSucceeded(stripeEvent, msgs);
                    
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
        private IHttpActionResult InvoicePaymentSucceeded(Stripe.Event stripeEvent, SystemMessages msgs)
        {
            var stripeInvoice = (Stripe.Invoice) stripeEvent.Data.Object;

            if (stripeInvoice.IsNotEmpty())
                return Return500(msgs);
            //return ProcessInvoiceDetails(stripeInvoice, msgs);

            return Return500(msgs);
        }
    }
}