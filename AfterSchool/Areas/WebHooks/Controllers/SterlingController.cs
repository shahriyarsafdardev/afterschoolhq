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
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class SterlingController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Update([FromBody] dynamic json)
        {
            var msgs = new SystemMessages();

            if (!AfterSchoolLibrary.Helpers.Utilities.IsProduction)
                return ReturnOK(msgs);

            try
            {
                var parsedObject = JObject.Parse(json.ToString());
                var payload = parsedObject["payload"].ToString();
                var responseBody = (Payload) JsonConvert.DeserializeObject<Payload>(payload);

                var backgroundChecks = BackgroundCheck.LoadByCandidateId(responseBody.candidateId);
                var latestBackgroundCheck = backgroundChecks.OrderByDescending(x => x.CreateDate).FirstOrDefault();

                if (latestBackgroundCheck.IsEmpty())
                    return Ok(msgs);

                if (IsPreviouslyApproved(latestBackgroundCheck.WebUserId, msgs))
                    return Ok(msgs);

                if (IsPreviouslyTaskCreated(latestBackgroundCheck.WebUserId, msgs))
                    return Ok(msgs);

                responseBody.result = responseBody.result.Replace(" ", "");
                responseBody.status = responseBody.status.Replace(" ", "");

                if (responseBody.result != "")
                {
                    var passedStatusCriterias = new string[] {"complete"};
                    var passedResultCriterias = new string[] {"clear"};
                    var considerStatusCriterias = new string[] {"complete"};
                    var considerResultCriterias = new string[] {"consider"};

                    if (backgroundChecks.Count > 0)
                        foreach (var backgroundCheck in backgroundChecks)
                        {
                            backgroundCheck.IsActive = false;
                            msgs.AddRange(backgroundCheck.Save());
                        }

                    latestBackgroundCheck.SterlingReviewCompleteDate = DateTime.UtcNow;

                    var isPassedScreen = passedResultCriterias.Contains(responseBody.result.ToLower()) &&
                                         passedStatusCriterias.Contains(responseBody.status.ToLower());
                    var isConsiderScreen = considerResultCriterias.Contains(responseBody.result.ToLower()) &&
                                           considerStatusCriterias.Contains(responseBody.status.ToLower());

                    if (isPassedScreen && IsAllReportsClear(responseBody.reportItems))
                        VolunteerApprovalService.CreateNewApproval(latestBackgroundCheck.WebUserId,
                            latestBackgroundCheck.RequestedForFiscalAccountId,
                            ApprovalStatus.Approved,
                            "Approved By Sterling",
                            DateTime.UtcNow.AddYears(1),
                            null,
                            DateTime.UtcNow,
                            msgs
                        );

                    else if (isConsiderScreen && IsAnyReportConsider(responseBody.reportItems))
                        VolunteerTaskService.CreateNewTask(latestBackgroundCheck.WebUserId,
                            TaskStatus.ReviewBackground,
                            ApplicationStatus.FailedBackgroundCheck,
                            latestBackgroundCheck.RequestedForFiscalAccountId,
                            null,
                            msgs);

                    latestBackgroundCheck.BackgroundCheckStatusId = (BackgroundCheckStatus) Enum.Parse(typeof(BackgroundCheckStatus), responseBody.status);
                    latestBackgroundCheck.BackgroundCheckResultId =
                        (SterlingBackgroundCheckResult) Enum.Parse(typeof(SterlingBackgroundCheckResult), responseBody.result);
                    latestBackgroundCheck.IsActive = true;
                    msgs.AddRange(latestBackgroundCheck.Save());
                }
                else
                {
                    if (responseBody.status != "Expired Invite")
                        latestBackgroundCheck.BackgroundCheckStatusId = (BackgroundCheckStatus) Enum.Parse(typeof(BackgroundCheckStatus), responseBody.status);
                    else
                        latestBackgroundCheck.BackgroundCheckStatusId = BackgroundCheckStatus.ExpiredInvite;

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
            var toEmail = "dev@afterschoolhq.com";
            var subject = $"Sterling Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
            var body = msgs.RenderMessageList(MessageOutputType.Html);

            EmailHelper.SendEmail(toEmail, subject, body, true);
        }

        private static bool IsAnyReportConsider(IEnumerable<ReportItem> reportItems)
        {
            var criterias = new string[] {"consider", "review"};
            return reportItems.Any(item => criterias.Contains(item.Result.ToLower()));
        }

        private static bool IsAllReportsClear(IEnumerable<ReportItem> reportItems)
        {
            var criterias = new string[] {"clear", "complete"};
            return reportItems.All(item => item.Status.ToLower().Equals("complete") &&
                                           criterias.Contains(item.Result.ToLower()));
        }

        private static bool IsPreviouslyApproved(int? webuserID, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var approvalEntry = VolunteerApprovalService.GetLatestPreviousApproval(webuserID, msgs);
            return approvalEntry.IsNotEmpty() &&
                   approvalEntry.ApprovalStatusId == ApprovalStatus.Approved;
        }

        private static bool IsPreviouslyTaskCreated(int? webuserID, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var previousTask = VolunteerTaskService.GetLatestPreviousTask(webuserID, msgs);
            return previousTask.IsNotEmpty() &&
                   previousTask.Task == TaskStatus.ReviewBackground &&
                   previousTask.ApplicationStatus == ApplicationStatus.FailedBackgroundCheck &&
                   previousTask.DateCompleted.IsEmpty();
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

        [JsonProperty(PropertyName = "reportItems")]
        public IEnumerable<ReportItem> reportItems { get; set; }
    }

    public class ReportItem
    {
        [JsonProperty(PropertyName = "id")] 
        public string ID { get; set; }

        [JsonProperty(PropertyName = "type")] 
        public string Type { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "estimatedCompletionTime")]
        public DateTime EstimatedCompletionTime { get; set; }
    }
}