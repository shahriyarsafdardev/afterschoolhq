using System;
using System.Web.Http;
using AfterSchoolLibrary.Helpers;
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

                    if (responseBody.result == "Clear")
                    {
                        backGroundCheck.SterlingReviewCompleteDate = DateTime.UtcNow;
                
                        VolunteerApprovalService.AddApproval(backGroundCheck.WebUserId,
                            backGroundCheck.RequestedForFiscalAccountId,
                            ApprovalStatus.Approved,
                            "Approve By Sterling",
                            (DateTime)backGroundCheck.ExpirationDate,
                            null,
                            msgs
                        );
                        
                    }

                    if (responseBody.result == "Review")
                    {
                        backGroundCheck.SterlingReviewCompleteDate = DateTime.UtcNow;
                        
                        VolunteerTaskService.AddTask(backGroundCheck.WebUserId, 
                            TaskStatus.ReviewBackground, 
                            ApplicationStatus.FailedBackgroundCheck, 
                            backGroundCheck.RequestedForFiscalAccountId,
                            null,
                            msgs);
                    }
                    
                    EmailHelper.SendEmail("bilal@reborncode.com", "Json", $"<html><body><h1>HTML Email</h1><p>{parsedObject}</p></body></html>", false);
                    
                    backGroundCheck.BackgroundCheckStatusId = (BackgroundCheckStatus)Enum.Parse(typeof(BackgroundCheckStatus), responseBody.status);
                    backGroundCheck.BackgroundCheckResultId = (BackgroundCheckResult)Enum.Parse(typeof(BackgroundCheckResult), responseBody.result);

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