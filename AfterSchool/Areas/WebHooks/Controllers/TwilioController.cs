using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Web.Http;
using Twilio.AspNet.Common;
using Twilio.Rest.Api.V2010.Account;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class TwilioController : ApiController
    {
        [HttpPost]
        public IHttpActionResult UpdateStatus(string id, string id2, [FromBody] SmsRequest request)
        {
            var msgs = new SystemMessages();

            if (request.IsNotEmpty() && request.MessageStatus.In(new string[] { "failed", "undelivered" }) && id.IsGuid() && id2.IsGuid())
            {
                try
                {
                    var activityMessage = ActivityMessage.LoadByUid(id.ToGuid(), msgs);
                    var registrant = Registrant.LoadByUid(id2.ToGuid(), msgs);

                    if (activityMessage.IsNotEmpty() && registrant.IsNotEmpty())
                    {
                        var resource = MessageResource.Fetch(request.SmsSid);

                        if (resource.IsNotEmpty() && resource.ErrorCode.HasValue)
                            registrant.AddPhoneToTwilioBlockedList(resource, null, msgs);
                    }

                    return ReturnOK(msgs);
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error, ex);
                    return Return500(msgs);
                }
            }

            return ReturnOK(msgs);
        }

        private void SendLogEmail(SystemMessages msgs)
        {
            string toEmail = "darye+twilio+errors@afterschoolhq.com";
            string subject = $"Twilio Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
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