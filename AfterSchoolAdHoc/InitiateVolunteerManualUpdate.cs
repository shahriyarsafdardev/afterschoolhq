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
        
        private static void WriteVolunteerToCsv(IEnumerable<VolunteerRecord> record, string path)
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
        
        private static IEnumerable<VolunteerRecord> ReadImportedVolunteersCsv(string path)
        {
            try
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    return csv.GetRecords<VolunteerRecord>().ToList();
                }
            }
            catch (FileNotFoundException) { }
            return new List<VolunteerRecord>();
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

        public static void UpdateVolunteerStatus(SterlingCredentials credentials, SystemMessages msgs, DatabaseConnection dc)
        {
            var backgroundCheckList = BackgroundCheck.LoadByResultByIsActive(SterlingBackgroundCheckResult.Clear, dc: dc, msgs: msgs);
            var sToken = InititiateVolunteerBackgroundCheck.CreateSterlingToken(credentials);
            var passedCount = 0;
            var failedCount = 0;
            var failedWebUserIdList = new List<int>();
            foreach (var bgCheck in backgroundCheckList)
            {
                var (clientScreening, jsonRes) = GetScreening(bgCheck.SterlingCandidateId, credentials, sToken.Token);
                if (clientScreening?.ReportItems.Count >= 5)
                {
                    var isVolunteerPassed = clientScreening.ReportItems.All(reportItem =>
                        reportItem.Result is "clear" or "complete" or "unperformable" or "passed"
                        && reportItem.Status is "complete") && clientScreening.Result.ToLower() is "clear"
                        or "passed" && clientScreening.Status.ToLower() is "complete" or "archive";
                    if (isVolunteerPassed)
                    {
                        passedCount += 1;
                    }
                    else
                    {
                        failedCount += 1;
                        failedWebUserIdList.Add(bgCheck.WebUserId);
                    }

                    if (!isVolunteerPassed)
                    {
                        VolunteerTaskService.CreateNewTask(bgCheck.WebUserId,
                            TaskStatus.ReviewBackground,
                            ApplicationStatus.FailedBackgroundCheck,
                            bgCheck.RequestedForFiscalAccountId,
                            null,
                            msgs,
                            dc);
                    }
                    else
                    {
                        bgCheck.BackgroundCheckStatusId = BackgroundCheckStatus.Archive;
                        bgCheck.BackgroundCheckResultId = SterlingBackgroundCheckResult.Passed;
                        msgs.AddRange(bgCheck.Save(dc));
                    }
                }
            }

            Console.WriteLine(
                $"Passed Candidates Count : {passedCount} and Failed Candidates Count : {failedCount} and failed WebUserID's : {string.Join(", ", failedWebUserIdList)}");
        }

        public static void VerifyExpiredImportedVolunteers(SterlingCredentials credentials, SystemMessages msgs, DatabaseConnection dc)
        {
            var importedVolunteers = ReadImportedVolunteersCsv($"D:\\Imported Volunteers-07-01-2022\\Matched_Volunteers_Latest_List.csv")
                .Where(x => x.Status.ToLower() == "approved").ToList();
            var fileWritePath = $"D:\\Imported Volunteers-07-01-2022\\Matched_Volunteers({DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")}).csv";
            var matchedVolunteers = new List<VolunteerRecord>();
            var count = 0;
            var sToken = InititiateVolunteerBackgroundCheck.CreateSterlingToken(credentials);
            importedVolunteers.ForEach(volunteer =>
            {
                var webUser = WebUser.LoadByEmail(volunteer.Email.Trim(),msgs,dc);
                var expirationDate = volunteer.DateReviewed.Value.AddYears(1);
                if (webUser.IsNotEmpty())
                {
                    var volunteerApproval = VolunteerApproval.LoadByWebUserId(webUser.ID.Value,msgs,dc).MaxBy(x => x.CreateDate);
                    if (volunteerApproval.IsNotEmpty())
                    {
                        count++;
                        Console.WriteLine($"Current Count is: {count}");
                        var backgroundCheck = BackgroundCheck.LoadByWebUserId(webUser.ID.Value, true, msgs, dc).FirstOrDefault();
                        if (backgroundCheck.IsEmpty())
                            Console.WriteLine($"Background Check is Empty");
                        if (volunteerApproval.ExpirationDate < DateTime.UtcNow.Date && (backgroundCheck.IsEmpty() || 
                             IsBackgroundCheckExpired(backgroundCheck,sToken,credentials)) &&
                            (volunteerApproval.ExpirationDate == DateTime.UtcNow.ToString("07/01/2023").ToDateTime() ||
                            volunteerApproval.ExpirationDate == expirationDate))
                        {
                            var volRecord = new VolunteerRecord
                            {
                                FirstName = webUser.FirstName,
                                LastName = webUser.LastName,
                                WebUserId = webUser.ID,
                                Email = volunteer.Email,
                                Status = volunteer.Status,
                                DateReviewed = volunteer.DateReviewed,
                                ExpirationDate = volunteerApproval.ExpirationDate,
                            };
                            matchedVolunteers.Add(volRecord);
                        }
                    }
                }
            });
            WriteVolunteerToCsv(matchedVolunteers,fileWritePath);
        }
        
        public static void VerifyAndRemoveExpiredImportedVolunteers(SterlingCredentials credentials, SystemMessages msgs, DatabaseConnection dc)
        {
            var importedVolunteers = ReadImportedVolunteersCsv($"D:\\Imported Volunteers-07-01-2022\\Matched_Volunteers_Latest_List.csv")
                .Where(x => x.Status.ToLower() == "approved").ToList();
            var fileWritePath = $"D:\\Imported Volunteers-07-01-2022\\Matched_Volunteers({DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")}).csv";
            var matchedVolunteers = new List<VolunteerRecord>();
            var count = 0;
            importedVolunteers.ForEach(volunteer =>
            {
                var webUser = WebUser.LoadByEmail(volunteer.Email.Trim(),msgs,dc);
                var expirationDate = volunteer.DateReviewed.Value.AddYears(1);
                if (webUser.IsNotEmpty())
                {
                    var volunteerApproval = VolunteerApproval.LoadByWebUserId(webUser.ID.Value,msgs,dc).MaxBy(x => x.CreateDate);
                    if (volunteerApproval.IsNotEmpty())
                    {
                        count++;
                        Console.WriteLine($"Current Count is: {count}");
                        var backgroundCheck = BackgroundCheck.LoadByWebUserId(webUser.ID.Value, true, msgs, dc).FirstOrDefault();
                        if (volunteerApproval.ExpirationDate < DateTime.UtcNow.Date && backgroundCheck.IsEmpty() &&
                            (volunteerApproval.ExpirationDate == DateTime.UtcNow.ToString("07/01/2023").ToDateTime() ||
                             volunteerApproval.ExpirationDate == expirationDate))
                        {
                            var volRecord = new VolunteerRecord
                            {
                                FirstName = webUser.FirstName,
                                LastName = webUser.LastName,
                                WebUserId = webUser.ID,
                                Email = volunteer.Email,
                                Status = volunteer.Status,
                                DateReviewed = volunteer.DateReviewed,
                                ExpirationDate = volunteerApproval.ExpirationDate,
                            };
                            matchedVolunteers.Add(volRecord);

                            RemoveVolunteerAccount(webUser,volunteerApproval,msgs,dc);
                        }
                    }
                }
            });
            WriteVolunteerToCsv(matchedVolunteers,fileWritePath);
        }

        private static void RemoveVolunteerAccount(WebUser webUser, VolunteerApproval volunteerApproval,
            SystemMessages msgs, DatabaseConnection dc)
        {
            var account = Account.LoadByOwnerId(webUser.ID.Value, msgs, dc).FirstOrDefault(x => x.AccountTypeId == AccountType.Volunteer);
            var accountWebUser = account.GetAccountWebUser(webUser.ID.Value,msgs,dc);
            var registrants = webUser.GetAllVolunteerRegistrants(msgs, dc);
            Database.ExecuteTransaction(
                (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    registrants.ForEach(registrant =>
                    {
                        if (!registrant.IsMaster)
                        {
                            var registration = registrant.GetRegistrations(amsgs, adc).FirstOrDefault();
                            var registrantActivity = registrant.GetRegistrantActivities(amsgs, adc).FirstOrDefault();
                            if (registration.IsNotEmpty())
                                amsgs.AddRange(registration.Delete(adc));
                            if (registrantActivity.IsNotEmpty() && amsgs.HasNoErrors())
                                amsgs.AddRange(registrantActivity.Delete(adc));
                        }
                        else
                        {
                            var registrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(registrant.ID.Value, webUser.ID.Value, amsgs, adc);
                            if(registrantWebUser.IsNotEmpty())
                                amsgs.AddRange(registrantWebUser.Delete(adc));
                        }

                        amsgs.AddRange(registrant.Delete(adc));
                    });
                    if (volunteerApproval.IsNotEmpty() && amsgs.HasNoErrors())
                        amsgs.AddRange(volunteerApproval.Delete(adc));
                    if(accountWebUser.IsNotEmpty() && amsgs.HasNoErrors())
                        amsgs.AddRange(accountWebUser.Delete(adc));
                    if (account.IsNotEmpty() && amsgs.HasNoErrors())
                        amsgs.AddRange(account.Delete(adc));
                    if (webUser.IsNotEmpty() && amsgs.HasNoErrors())
                        amsgs.AddRange(webUser.Delete(adc));
                },
                (SystemMessages smsgs) => msgs.AddRange(smsgs),
                (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs),
                dc);
        }
        private static bool IsBackgroundCheckExpired(BackgroundCheck bgCheck, SterlingToken sToken, SterlingCredentials credentials)
        {
            if (bgCheck.IsNotEmpty())
            {
                var (clientScreening, jsonRes) = GetScreening(bgCheck.SterlingCandidateId, credentials, sToken.Token);
                if (clientScreening.Result.ToLower() == "pending" && clientScreening.Status.ToLower() == "predraft")
                    return true;
            }
            
            return false;
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
        private record VolunteerRecord
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public int? WebUserId { get; set; }
            public string? Status { get; set; }
            public DateTime? DateReviewed { get; set; }
            public DateTime? ExpirationDate { get; set; }
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
