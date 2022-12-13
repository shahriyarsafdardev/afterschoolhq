using System;
using System.Web.Http;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using Newtonsoft.Json;
using BasicLibrary;
using Newtonsoft.Json.Linq;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class SterlingController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Update([FromBody] dynamic json)
        {
            var msgs = new SystemMessages();

            try
            {
                var parsedObject = JObject.Parse(json.ToString());
                var payload = parsedObject["payload"].ToString();
                var responseBody = JsonConvert.DeserializeObject<Payload>(payload);
            
                BackgroundCheck backGroundCheck = BackgroundCheck.LoadByCandidateId(responseBody.candidateId);

                if (backGroundCheck.IsNotEmpty())
                {
                    if (responseBody.status == "PreDraft")
                        backGroundCheck.BackgroundCheckStatusId = BackgroundCheckStatus.predraft;
            
                    if (responseBody.status == "Pending")
                        backGroundCheck.BackgroundCheckStatusId = BackgroundCheckStatus.pending;

                    if (responseBody.status == "Complete")
                    {
                        backGroundCheck.BackgroundCheckStatusId = BackgroundCheckStatus.Completed;
                
                        VolunteerApprovalService.AddApproval(backGroundCheck.WebUserId,
                            backGroundCheck.RequestedForFiscalAccountId,
                            ApprovalStatus.Approved,
                            "Approve By Sterling",
                            (DateTime)backGroundCheck.ExpirationDate,
                            null,
                            msgs
                        );
                    }

                    if (responseBody.status == "Review")
                    {
                        backGroundCheck.BackgroundCheckStatusId = BackgroundCheckStatus.Review;
                
                        VolunteerTaskService.AddTask(backGroundCheck.WebUserId, 
                            TaskStatus.ReviewBackground, 
                            ApplicationStatus.FailedBackgroundCheck, 
                            backGroundCheck.RequestedForFiscalAccountId,
                            null,
                            msgs);
                    }

                    if(msgs.HasNoErrors())
                        msgs.AddRange(backGroundCheck.Save());
                }

                return ReturnOK(msgs);
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
            string subject = $"Sterling Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
            string body = msgs.RenderMessageList(MessageOutputType.Html);

            AfterSchoolLibrary.Helpers.EmailHelper.SendEmail(toEmail, subject, body, true);
        }
    }
    
    public class Payload
    {
        [JsonProperty(PropertyName = "id")] 
        public string id { get; set; }
        
        [JsonProperty(PropertyName = "candidateId")] 
        public string candidateId { get; set; }
        
        [JsonProperty(PropertyName = "status")] 
        public string status { get; set; }
        
        [JsonProperty(PropertyName = "result")] 
        public string result { get; set; }
    }
}