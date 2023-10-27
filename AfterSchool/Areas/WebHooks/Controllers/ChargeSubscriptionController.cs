using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using static AfterSchool.Controllers.AWSSubscriptionLambdaController;
using Account = AfterSchoolLibrary.Models.Account;
using SubscriptionSchedule = AfterSchoolLibrary.Models.SubscriptionSchedule;
using SubscriptionScheduleService = AfterSchoolLibrary.Services.SubscriptionScheduleService;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

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
                var serializedJson = JsonConvert.SerializeObject(json);
                WebhookPayload webhookPayload = JsonConvert.DeserializeObject<WebhookPayload>(serializedJson);

                var subscriptionPaymentHistory = SubscriptionPaymentHistory.LoadByUid(webhookPayload.PaymentHistoryUid, msgs);
                var subscriptionSchedule = SubscriptionSchedule.LoadByUid(webhookPayload.ScheduleUid, msgs);
                var charge = StripeHelper.GetStripeHelperConnectAccount().GetCharge(webhookPayload.ChargeId, msgs);
                var pendingCase = webhookPayload.PendingCase;

                var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);

                ChargePendingToday(charge, subscriptionSchedule, subscriptionPaymentHistory, msgs, pendingCase);

                if (msgs.HasNoErrors())
                    return ReturnOK(msgs);
                else
                    return Return500(msgs);
            }
            catch (Exception ex)
            {
                Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                return Return500(msgs);
            }
        }

        private void ChargePendingToday(Charge charge, SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory subscriptionPaymentHistory, 
            SystemMessages msgs, bool pendingCase = false, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    HandleCharge(charge, subscriptionSchedule, subscriptionPaymentHistory, msgs);
                    CreatePaymentRecords(subscriptionSchedule, subscriptionPaymentHistory, charge, msgs, pendingCase, dc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
            );
        }

        private void CreatePaymentRecords(SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory subscriptionPaymentHistory,
            Charge charge, SystemMessages msgs, bool pendingCase = false, DatabaseConnection dc = null)
        {
            msgs.AddRange(SubscriptionScheduleService.CreatePaymentRecordsAfterCharge(subscriptionSchedule, subscriptionPaymentHistory, charge,
                msgs, pendingCase));
        }

        private void HandleCharge(Charge charge, SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory subscriptionPaymentHistory,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            msgs.AddRange(SubscriptionScheduleService.UpdateSubscriptionAndPaymentRecordsAfterCharge(charge, subscriptionSchedule,
                subscriptionPaymentHistory, msgs, dc));
        }

        private IHttpActionResult ReturnOK(SystemMessages msgs)
        {
            return Ok(msgs.ToJSON());
        }
        private IHttpActionResult Return500(SystemMessages msgs)
        {
            return InternalServerError(new Exception(msgs.RenderMessageList(MessageOutputType.Text)));
        }
    }
}