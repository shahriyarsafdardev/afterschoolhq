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

namespace AfterSchoolAdHoc;

internal class InititiateVolunteerBackgroundCheck
{
    public static void Run(SterlingCredentials credentials, SystemMessages msgs, DatabaseConnection dc)
    {
        var accountId = 196;
        var createdDate = DateTime.UtcNow.ToString("07/01/2023").ToDateTime();
        var volunteerRegistrants = Registrant.GetAllSelfRegisteredVolunteers(accountId, (int) RegistrantPersonType.Volunteer,
            createdDate, false, msgs, dc);

        volunteerRegistrants.ForEach(registrant =>
        {
            var registration = registrant.GetRegistrations(msgs, dc).FirstOrDefault();
            var participationObject = registration.GetFirstParticipationObject(msgs, dc);
            var webUser = registrant.GetWebUser(msgs, dc);
            var backgroundCheck = BackgroundCheck.LoadByWebUserId((int) webUser.ID, true, msgs, dc)
                .OrderByDescending(x => x.CreateDate).FirstOrDefault();

            if (registration.IsNotEmpty() && participationObject.IsNotEmpty() && participationObject.OwnerAccount.FiscalAccountId.IsNotEmpty()
                && backgroundCheck.IsEmpty() && !registration.ESignature.Equals("IMPORTED"))
                InitiateBackgroundCheck(credentials, webUser, participationObject, accountId, msgs, dc);
        });
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
            Invite = new SterlingScreeningInviteRequest {Method = "link"},
            Callback = new SterlingScreeningCallBackUri {uri = $"https://{AfterSchoolLibrary.Helpers.Utilities.HostName}/WebHooks/Sterling/Update"},
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
}