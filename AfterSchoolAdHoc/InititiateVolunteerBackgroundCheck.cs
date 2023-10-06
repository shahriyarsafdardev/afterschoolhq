using System.Web;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Services;
using Emmersion.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpClient = Emmersion.Http.HttpClient;
using HttpMethod = Emmersion.Http.HttpMethod;
using Lob.Models;
using System.Data;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using Org.BouncyCastle.Ocsp;
using System.Net;
using Amazon.DynamoDBv2.Model;

namespace AfterSchoolAdHoc;

internal class InititiateVolunteerBackgroundCheck
{
    public static void Run(SterlingCredentials credentials, SystemMessages msgs, DatabaseConnection dc)
    {
        var registration = Registration.Load(54178, msgs, dc);
        var participationObject = registration.GetFirstParticipationObject(msgs, dc);
        var webUser = WebUser.Load(20387, msgs, dc);
        var backgroundCheck = BackgroundCheck.LoadByWebUserId((int) webUser.ID, true, msgs, dc)
            .OrderByDescending(x => x.CreateDate).FirstOrDefault();

        if (registration.IsNotEmpty() && participationObject.IsNotEmpty() && participationObject.OwnerAccount.FiscalAccountId.IsNotEmpty()
            && backgroundCheck.IsEmpty())
            InitiateBackgroundCheck(credentials, webUser, participationObject, (int) participationObject.OwnerAccount.FiscalAccountId, msgs, dc);
    }

    private static void InitiateBackgroundCheck(SterlingCredentials credentials, WebUser webUser, ParticipationObject participationObject,
        int accountId, SystemMessages msgs, DatabaseConnection dc)
    {
        string candidateId = null;
        string backgroundCheckUrl = null;
        try
        {
            var token = CreateSterlingToken(credentials);
            candidateId = GetSterlingCandidateId(credentials, token.Token, webUser);

            if (candidateId.IsEmpty())
                candidateId = CreateCandidate(credentials, token.Token, webUser);

            backgroundCheckUrl = CreateScreen(credentials, token.Token, candidateId);

            var status = candidateId is null || backgroundCheckUrl is null
                ? BackgroundCheckStatus.BackgroundCheckRequested
                : BackgroundCheckStatus.EmailSentToVolunteer;

            var backgroundCheck = new BackgroundCheck
            {
                WebUserId = webUser.ID!.Value,
                RequestedForFiscalAccountId = accountId,
                BackgroundCheckStatusId = status,
                BackgroundCheckResultId = SterlingBackgroundCheckResult.Pending,
                SterlingCandidateId = candidateId,
                SterlingInviteUrl = backgroundCheckUrl,
                BackgroundCheckSourceId = BackgroundCheckSource.Sterling,
                SterlingEmailSentDate = backgroundCheckUrl is null ? (DateTime?) null : DateTime.UtcNow,
                ExpirationDate = backgroundCheckUrl is null ? (DateTime?) null : DateTime.UtcNow.AddYears(1),
                IsActive = true,
                CreateDate = DateTime.UtcNow,
            };

            if (msgs.HasNoErrors())
            {
                msgs.AddRange(backgroundCheck.Save(dc));
                EmailHelper.InitiateBackgroundCheckMail(webUser, participationObject, backgroundCheckUrl, msgs);
            }
        }
        catch (Exception ex)
        {
            msgs.Add(new SystemMessage(ex.Message, MessageType.Error));
        }
    }

    private static SterlingToken CreateSterlingToken(SterlingCredentials credentials)
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequest
        {
            Url = $"{credentials.Endpoint}/v2/oauth",
            Method = HttpMethod.POST,
        };
        request.AddBasicAuthentication(credentials.ClientId, credentials.ClientSecret);

        request.AddFormUrlEncodedBody(new Dictionary<string, string>
        {
            {"grant_type", "client_credentials"},
        });

        var response = httpClient.Execute(request);

        if (response.StatusCode != 200)
            throw new Exception($"Sterling token request failed with status code {response.StatusCode}; body: {response.Body}");

        var responseBody = JsonConvert.DeserializeObject<SterlingTokenResponse>(response.Body);
        return new SterlingToken
        {
            Token = responseBody.AccessToken,
            ExpiresIn = TimeSpan.FromSeconds(responseBody.ExpiresIn),
        };
    }

    private static string CreateCandidate(SterlingCredentials credentials, string bearerToken, WebUser webUser)
    {
        using var httpClient = new HttpClient();
        var sterlingRequest = new SterlingCandidateRequest
        {
            GivenName = webUser.FirstName,
            FamilyName = webUser.LastName,
            Email = webUser.Email,
            ClientReferenceId = webUser.Uid.ToString(),
        };
        var request = new HttpRequest
        {
            Url = $"{credentials.Endpoint}/v2/candidates",
            Method = HttpMethod.POST,
            Headers = new HttpHeaders()
                .Add("Authorization", $"Bearer {bearerToken}")
                .Add("Content-Type", "application/json"),
            Body = JsonConvert.SerializeObject(sterlingRequest),
        };

        var response = httpClient.Execute(request);

        if (response.StatusCode != 201)
            throw new Exception($"Sterling candidate request failed with status code {response.StatusCode}; body: {response.Body}");

        var responseBody = JsonConvert.DeserializeObject<SterlingCandidateResponse>(response.Body);
        return responseBody.Id;
    }

    private static string CreateScreen(SterlingCredentials credentials, string bearerToken, string candidateId)
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequest
        {
            Url = $"{credentials.Endpoint}/v2/screenings",
            Method = HttpMethod.POST,
            Headers = new HttpHeaders()
                .Add("Authorization", $"Bearer {bearerToken}")
                .Add("Content-Type", "application/json"),
        };
        var sterlingRequest = new SterlingScreeningRequest
        {
            CandidateId = candidateId,
            PackageId = credentials.PackageId,
            AccountId = credentials.AccountId,
            Invite = new SterlingScreeningInviteRequest { Method = "link" },
            Callback = new SterlingScreeningCallBackUri { uri = $"https://afterschoolhq.com/WebHooks/Sterling/Update" },
        };
        request.Body = JsonConvert.SerializeObject(sterlingRequest);

        var response = httpClient.Execute(request);

        if (response.StatusCode != 201)
            throw new Exception($"Sterling screening request failed with status code {response.StatusCode}; body: {response.Body}");

        var responseBody = JsonConvert.DeserializeObject<SterlingScreeningResponse>(response.Body);
        return responseBody.Invite.Link;
    }

    private static string GetSterlingCandidateId(SterlingCredentials credentials, string bearerToken, WebUser webUser)
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequest
        {
            Url = $"{credentials.Endpoint}/v2/candidates?email={HttpUtility.UrlEncode(webUser.Email)}",
            Method = HttpMethod.GET,
            Headers = new HttpHeaders()
                .Add("Authorization", $"Bearer {bearerToken}")
                .Add("Content-Type", "application/json"),
        };

        var response = httpClient.Execute(request);

        if (response.StatusCode == 404 || response.StatusCode == 400) return null;

        var responseBody = JArray.Parse(response.Body);

        if (response.StatusCode == 200)
            return JsonConvert.DeserializeObject<SterlingCandidateResponse>(responseBody[0].ToString()).Id;

        return null;
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

    public static void RunCallbackFix(SterlingCredentials credentials, SystemMessages msgs, DatabaseConnection dc)
    {
        var bgClients = BackgroundCheck.LoadByStatusAndActive(BackgroundCheckStatus.EmailSentToVolunteer, dc: dc, msgs: msgs);//.Where(c => c.SterlingCandidateId == "462eb932-e2ef-4e71-82f4-dfec38f7efff");
        //var bgClients = new List<BackgroundCheck>() { BackgroundCheck.LoadByCandidateIdActive("462eb932-e2ef-4e71-82f4-dfec38f7efff", dc: dc, msgs: msgs) };
        var sToken = CreateSterlingToken(credentials);
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
                if (clientScreening.Result == "Consider" || clientScreening.Result == "Passed")
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
            var responseBody = JsonConvert.DeserializeObject<Payload>(json);

            BackgroundCheck latestBackgroundCheck = BackgroundCheck.LoadByCandidateIdActive(responseBody.candidateId, msgs, dc);

            if (responseBody.result != "")
            {

                latestBackgroundCheck.SterlingReviewCompleteDate = DateTime.UtcNow;

                if (responseBody.result == "Passed")
                    VolunteerApprovalService.CreateNewApproval(latestBackgroundCheck.WebUserId,
                        latestBackgroundCheck.RequestedForFiscalAccountId,
                        ApprovalStatus.Approved,
                        "Approved By Sterling",
                        DateTime.UtcNow.AddYears(1),
                        null,
                        DateTime.UtcNow,
                        msgs,
                        dc
                    );

                else if (responseBody.result == "Consider")
                    VolunteerTaskService.CreateNewTask(latestBackgroundCheck.WebUserId,
                        TaskStatus.ReviewBackground,
                        ApplicationStatus.FailedBackgroundCheck,
                        latestBackgroundCheck.RequestedForFiscalAccountId,
                        null,
                        msgs,
                        dc);

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
    }

}







