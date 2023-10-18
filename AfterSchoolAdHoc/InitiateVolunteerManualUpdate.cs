using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Services;
using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;
using AfterSchoolLibrary.Integrations;
using Emmersion.Http;
using HttpClient = Emmersion.Http.HttpClient;
using HttpMethod = Emmersion.Http.HttpMethod;

namespace AfterSchoolAdHoc
{
    internal class InitiateVolunteerManualUpdate
    {
        public static void Run(SterlingCredentials credentials, SystemMessages msgs, DatabaseConnection dc)
        {
            var prevClients = ReadLatest($"C:\\Prod-LocalhostRecords(09-10-2023 20-49-37).csv");
            var prevClientIds = prevClients.Select(x => x.WebUserId);
            var bgClients = BackgroundCheck.LoadByStatusAndActive(BackgroundCheckStatus.EmailSentToVolunteer, dc: dc, msgs: msgs).Where(x => prevClientIds.Contains(x.WebUserId));//.Where(c => c.SterlingCandidateId == "462eb932-e2ef-4e71-82f4-dfec38f7efff");
            //var bgClients = new List<BackgroundCheck>() { BackgroundCheck.LoadByCandidateIdActive("462eb932-e2ef-4e71-82f4-dfec38f7efff", dc: dc, msgs: msgs) };
            var sToken = InititiateVolunteerBackgroundCheck.CreateSterlingToken(credentials);
            var filePath = $"C:\\Prod-LocalhostRecords({DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")}).csv";
            var count = 0;
            var failedCandidates = new List<bgRecord>();
            foreach (var client in bgClients)
            {
                (var clientScreening, var jsonRes) = GetScreening(client.SterlingCandidateId, credentials, sToken.Token);
                if (clientScreening?.Callback.uri.Contains("localhost:54849") ?? false)
                {
                    var rec = new bgRecord
                    {
                        ScreeningId = clientScreening.Id,
                        WebUserId = client.WebUserId,
                        SterlingCandidateId = client.SterlingCandidateId,
                        Uri = clientScreening.Callback.uri,
                        PrevBackgroundCheckStatus = client.BackgroundCheckStatusId.ToString(),
                        PrevBackgroundCheckResult = client.BackgroundCheckResultId.ToString(),
                        NewBackgroundCheckStatus = clientScreening.Status,
                        NewBackgroundCheckResult = clientScreening.Result
                    };
                    failedCandidates.Add(rec);

                    count++;
                    Console.WriteLine($"Current Count : {count}");
                    if (clientScreening.Result == "Consider" || clientScreening.Result == "Passed" && false)
                    {
                        var rmsgs = Update(jsonRes, dc);
                        if (rmsgs.HasErrors())
                        {
                            rmsgs.ToList().ForEach(msg => Console.WriteLine(msg.Text));
                            break;
                        }
                    }
                    //break;
                }
            }
            Console.WriteLine($"Total Count : {count}");

            WriteToCsv(failedCandidates, filePath);
        }

        private static IEnumerable<string>? GetScreeningIds(string clientId, SterlingCredentials credentials, string bearerToken)
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequest
            {
                Url = $"{credentials.Endpoint}/v2/candidates/{clientId}",
                Method = HttpMethod.GET,
                Headers = new HttpHeaders()
                    .Add("Authorization", $"Bearer {bearerToken}")
                    .Add("Content-Type", "application/json")
            };
            HttpResponse response;
            var issueCount = 0;
            while (true)
            {
                try
                {
                    response = httpClient.Execute(request);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sterling Issue Occured : {issueCount++}");
                    Task.Delay(10000).Wait();
                }
            }

            if (response.StatusCode != 200)
                throw new Exception($"Sterling candidate request failed with status code {response.StatusCode}; body: {response.Body}");

            var responseBody = JsonConvert.DeserializeObject<SterlingCandidateResponse>(response.Body);

            return responseBody?.ScreeningId;
        }

        private static (SterlingScreeningResponse? screeningObj, string? screeningResponse) GetScreening(string clientId, SterlingCredentials credentials, string bearerToken)
        {
            var screeningIds = GetScreeningIds(clientId, credentials, bearerToken);
            var maxDate = DateTime.MinValue;
            SterlingScreeningResponse? responseBody = null;
            string responseJson = "";
            if (screeningIds.IsNotEmpty())
            {
                foreach (var screeningId in screeningIds)
                {
                    using var httpClient = new HttpClient();
                    var request = new HttpRequest
                    {
                        Url = $"{credentials.Endpoint}/v2/screenings/{screeningId}",
                        Method = HttpMethod.GET,
                        Headers = new HttpHeaders()
                            .Add("Authorization", $"Bearer {bearerToken}")
                            .Add("Content-Type", "application/json")
                    };
                    HttpResponse response;
                    int issueCount = 0;
                    while (true)
                    {
                        try
                        {
                            response = httpClient.Execute(request);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Sterling Issue Occured : {issueCount++}");
                            Task.Delay(10000).Wait();
                        }
                    }
                    httpClient.Dispose();

                    if (response.StatusCode != 200)
                        throw new Exception($"Sterling candidate request failed with status code {response.StatusCode}; body: {response.Body}");

                    var res = JsonConvert.DeserializeObject<SterlingScreeningResponse>(response.Body);
                    if (maxDate < res?.UpdatedAt)
                    {
                        responseBody = res;
                        responseJson = response.Body;
                        maxDate = res.UpdatedAt;
                    }
                }

                return (responseBody, responseJson);
            }
            return (null, null);
        }

        private static void WriteToCsv(IEnumerable<bgRecord> record, string path)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(record);
            }

        }

        private static SystemMessages Update(dynamic json, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();

            try
            {
                var responseBody = (Payload) JsonConvert.DeserializeObject<Payload>(json);

                if (responseBody.reportItems.Count() < 5)
                {
                    msgs.Add(new SystemMessage("Missing report items!", MessageType.ApiError));
                    return msgs;
                }

                List<BackgroundCheck> backgroundChecks = BackgroundCheck.LoadByCandidateId(responseBody.candidateId);
                var latestBackgroundCheck = backgroundChecks.OrderByDescending(x => x.CreateDate).FirstOrDefault();

                if (IsPreviouslyApproved(latestBackgroundCheck.WebUserId, msgs))
                {
                    msgs.Add(new SystemMessage("Volunteer already approved!", MessageType.Confirmation));
                    return msgs;
                }
                else if (IsPreviouslyTaskCreated(latestBackgroundCheck.WebUserId, msgs))
                {
                    msgs.Add(new SystemMessage("Volunteer Task already created!", MessageType.Confirmation));
                    return msgs;
                }

                if (responseBody.result != "")
                {

                    string[] passedResultCriterias = new string[] { "clear", "passed" };
                    var passedStatusCriterias = new string[] { "complete", "archive" };
                    var considerResultCriterias = new string[] { "review", "consider" };
                    var isPassedScreen = passedResultCriterias.Contains(responseBody.result.ToLower()) && passedStatusCriterias.Contains((string) responseBody.status.ToLower());
                    var isConsiderScreen = considerResultCriterias.Contains(responseBody.result.ToLower());
                    if (IsAllClear(responseBody.reportItems) && isPassedScreen)
                        VolunteerApprovalService.CreateNewApproval(latestBackgroundCheck.WebUserId,
                            latestBackgroundCheck.RequestedForFiscalAccountId,
                            ApprovalStatus.Approved,
                            "Approved By Sterling",
                            DateTime.UtcNow.AddYears(1),
                            null,
                            DateTime.UtcNow,
                            msgs
                        );

                    else if (IsAnyConsider(responseBody.reportItems) && isConsiderScreen)
                        VolunteerTaskService.CreateNewTask(latestBackgroundCheck.WebUserId,
                            TaskStatus.ReviewBackground,
                            ApplicationStatus.FailedBackgroundCheck,
                            latestBackgroundCheck.RequestedForFiscalAccountId,
                            null,
                            msgs);

                    latestBackgroundCheck.BackgroundCheckStatusId = (BackgroundCheckStatus) Enum.Parse(typeof(BackgroundCheckStatus), responseBody.status);

                    latestBackgroundCheck.BackgroundCheckResultId = (SterlingBackgroundCheckResult) Enum.Parse(typeof(SterlingBackgroundCheckResult), responseBody.result);
                    latestBackgroundCheck.IsActive = true;

                    msgs.AddRange(latestBackgroundCheck.Save(dc));
                }
                else
                {

                    if (responseBody.status != "Expired Invite")
                        latestBackgroundCheck.BackgroundCheckStatusId = (BackgroundCheckStatus) Enum.Parse(typeof(BackgroundCheckStatus), responseBody.status);
                    else
                        latestBackgroundCheck.BackgroundCheckStatusId = BackgroundCheckStatus.ExpiredInvite;

                    latestBackgroundCheck.BackgroundCheckResultId = SterlingBackgroundCheckResult.Pending;

                    msgs.AddRange(latestBackgroundCheck.Save(dc));
                }

                return msgs;
            }
            catch (Exception ex)
            {
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                return msgs;
            }

        }

        private static IEnumerable<bgRecord> ReadLatest(string path)
        {
            try
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    return csv.GetRecords<bgRecord>().ToList();
                }
            }
            catch (FileNotFoundException) { }
            return new List<bgRecord>();
        }

        private static bool IsAnyConsider(IEnumerable<ReportItem> reportItems)
        {
            var criterias = new string[] { "consider", "review" };
            return reportItems.Any(item => criterias.Contains(item.Result.ToLower()));
        }

        private static bool IsAllClear(IEnumerable<ReportItem> reportItems)
        {
            var criterias = new string[] { "clear", "passed", "complete", "unperformable" };
            return reportItems.All(item => item.Status.ToLower().Equals("complete") && criterias.Contains(item.Result.ToLower()));
        }

        private static bool IsPreviouslyApproved(int? webuserID, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var approvalEntry = VolunteerApprovalService.GetLatestPreviousApproval(webuserID, msgs, dc);
            return approvalEntry.IsNotEmpty() && approvalEntry.ApprovalStatusId == ApprovalStatus.Approved;
        }

        private static bool IsPreviouslyTaskCreated(int? webuserID, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var previousTask = VolunteerTaskService.GetLatestPreviousTask(webuserID, msgs, dc);
            return previousTask.IsNotEmpty() && previousTask.Task == TaskStatus.ReviewBackground && previousTask.ApplicationStatus == ApplicationStatus.FailedBackgroundCheck;
        }

        private record bgRecord
        {
            public string ScreeningId { get; set; }
            public int WebUserId { get; set; }
            public string SterlingCandidateId { get; set; }
            public string Uri { get; set; }
            public string PrevBackgroundCheckStatus { get; set; }
            public string PrevBackgroundCheckResult { get; set; }
            public string NewBackgroundCheckStatus { get; set; }
            public string NewBackgroundCheckResult { get; set; }
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
}
