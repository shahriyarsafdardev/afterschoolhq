using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AfterSchoolLibrary.Enums;
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
                
                List<BackgroundCheck> backgroundChecks = BackgroundCheck.LoadByCandidateId(responseBody.candidateId);
                var latestBackgroundCheck = backgroundChecks.OrderByDescending(x => x.CreateDate).FirstOrDefault();
                
                if (responseBody.result != "")
                {
                    
                    if (backgroundChecks.IsNotEmpty())
                    {
                        foreach (var backgroundCheck in backgroundChecks)
                        {
                            backgroundCheck.IsActive = false;
                            msgs.AddRange(backgroundCheck.Save());
                        }
                    }

                    latestBackgroundCheck.SterlingReviewCompleteDate = DateTime.UtcNow;
            
                    if(responseBody.result == "Clear")
                        VolunteerApprovalService.CreateNewApproval(latestBackgroundCheck.WebUserId,
                            latestBackgroundCheck.RequestedForFiscalAccountId,
                            ApprovalStatus.Approved,
                            "Approve By Sterling",
                            DateTime.UtcNow.AddYears(1),
                            null,
                            msgs
                        );
                    
                    if(responseBody.result == "Review")
                        VolunteerTaskService.CreateNewTask(latestBackgroundCheck.WebUserId, 
                            TaskStatus.ReviewBackground, 
                            ApplicationStatus.FailedBackgroundCheck, 
                            latestBackgroundCheck.RequestedForFiscalAccountId,
                            null,
                            msgs);
                    
                    latestBackgroundCheck.BackgroundCheckStatusId = (BackgroundCheckStatus)Enum.Parse(typeof(BackgroundCheckStatus), responseBody.status);
                    latestBackgroundCheck.BackgroundCheckResultId = (SterlingBackgroundCheckResult)Enum.Parse(typeof(SterlingBackgroundCheckResult), responseBody.result);
                    latestBackgroundCheck.IsActive = true;
                    
                    msgs.AddRange(latestBackgroundCheck.Save());
                }
                else
                {
                    latestBackgroundCheck.BackgroundCheckStatusId = (BackgroundCheckStatus)Enum.Parse(typeof(BackgroundCheckStatus), responseBody.status);
                    latestBackgroundCheck.BackgroundCheckResultId = SterlingBackgroundCheckResult.Pending;
                    
                    msgs.AddRange(latestBackgroundCheck.Save());
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