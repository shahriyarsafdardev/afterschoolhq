using System;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using AfterSchoolLibrary.Models;
using Emmersion.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AfterSchoolLibrary.Integrations
{
    public interface ISterlingCandidateQuery
    {
        Task<string> CreateAsync(SterlingCredentials credentials, string bearerToken, WebUser webUser);
        Task<string> GetByEmaileAsync(SterlingCredentials credentials, string bearerToken, WebUser webUser);
    }

    public class SterlingCandidateQuery : ISterlingCandidateQuery
    {
        private readonly IHttpClient httpClient;
        private readonly IJsonSerializer jsonSerializer;

        public SterlingCandidateQuery(IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            this.httpClient = httpClient;
            this.jsonSerializer = jsonSerializer;
        }

        public async Task<string> CreateAsync(SterlingCredentials credentials, string bearerToken, WebUser webUser)
        {
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
                Body = jsonSerializer.Serialize(sterlingRequest),
            };
            
            var response = await httpClient.ExecuteAsync(request);

            if (response.StatusCode != 201)
                throw new Exception($"Sterling candidate request failed with status code {response.StatusCode}; body: {response.Body}");

            var responseBody = jsonSerializer.Deserialize<SterlingCandidateResponse>(response.Body);
            return responseBody.Id;
        }
        
        public async Task<string> GetByEmaileAsync(SterlingCredentials credentials, string bearerToken, WebUser webUser)
        {
            var request = new HttpRequest
            {
                Url = $"{credentials.Endpoint}/v2/candidates?email={HttpUtility.UrlEncode(webUser.Email)}",
                Method = HttpMethod.GET,
                Headers = new HttpHeaders()
                    .Add("Authorization", $"Bearer {bearerToken}")
                    .Add("Content-Type", "application/json"),
            };
            
            Helpers.Utilities.SystemLog(request.ToString());

            var response = await httpClient.ExecuteAsync(request);
            
            Helpers.Utilities.SystemLog(response.ToString());

            if (response.StatusCode == 404 || response.StatusCode == 400)
            {
                return null;
                Helpers.Utilities.SystemLog(response.ToString());
            }
            
            JArray responseBody = JArray.Parse(response.Body);

            if (response.StatusCode == 200)
                return jsonSerializer.Deserialize<SterlingCandidateResponse>(responseBody[0].ToString()).Id;
            
            return null;
        }
        
    }

    public class SterlingCandidateRequest
    {
        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "familyName")]
        public string FamilyName { get; set; }

        [JsonProperty(PropertyName = "email")] public string Email { get; set; }

        [JsonProperty(PropertyName = "clientReferenceId")]
        public string ClientReferenceId { get; set; }
    }

    public class SterlingCandidateResponse
    {
        [JsonProperty(PropertyName = "id")] public string Id { get; set; }
    }
}