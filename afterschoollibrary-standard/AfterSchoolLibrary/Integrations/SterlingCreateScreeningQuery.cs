using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;
using Emmersion.Http;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail.Model;

namespace AfterSchoolLibrary.Integrations
{
    public interface ISterlingCreateScreeningQuery
    {
        Task<string> CreateAsync(SterlingCredentials credentials, string bearerToken, string candidateId);
    }

    public class SterlingCreateScreeningQuery : ISterlingCreateScreeningQuery
    {
        private readonly IHttpClient httpClient;
        private readonly IJsonSerializer jsonSerializer;

        public SterlingCreateScreeningQuery(IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            this.httpClient = httpClient;
            this.jsonSerializer = jsonSerializer;
        }

        public async Task<string> CreateAsync(SterlingCredentials credentials, string bearerToken, string candidateId)
        {
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
                Callback = new SterlingScreeningCallBackUri {uri = $"https://{Helpers.Utilities.HostName}/WebHooks/Sterling/Update"}
            };
            request.Body = jsonSerializer.Serialize(sterlingRequest);
            
            var response = await httpClient.ExecuteAsync(request);
            
            if (response.StatusCode != 201)
                throw new Exception($"Sterling screening request failed with status code {response.StatusCode}; body: {response.Body}");

            var responseBody = jsonSerializer.Deserialize<SterlingScreeningResponse>(response.Body);
            return responseBody.Invite.Link;
        }
    }

    public class SterlingScreeningRequest
    {
        [JsonProperty(PropertyName = "candidateId")]
        public string CandidateId { get; set; }

        [JsonProperty(PropertyName = "packageId")]
        public string PackageId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "invite")]
        public SterlingScreeningInviteRequest Invite { get; set; }
        public SterlingScreeningCallBackUri Callback { get; set; }
    }

    public class SterlingScreeningInviteRequest
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
    }
    
    public class SterlingScreeningCallBackUri
    {
        [JsonProperty(PropertyName = "uri")]
        public string uri { get; set; }
    }

    public class SterlingScreeningResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "invite")]
        public SterlingScreeningInviteResponse Invite { get; set; }

        [JsonProperty(PropertyName = "callback")]
        public SterlingScreeningCallBackUri Callback { get; set; }

        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        
        [JsonProperty(PropertyName = "reportItems")]
        
        public List<SterlingScreenReportItems> ReportItems { get; set; }
        
        [JsonProperty(PropertyName = "updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class SterlingScreenReportItems
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

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

    public class SterlingScreeningInviteResponse
    {
        [JsonProperty(PropertyName = "link")] public string Link { get; set; }
    }
}