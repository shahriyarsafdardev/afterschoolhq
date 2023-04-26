using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BasicLibrary;
using Emmersion.Http;
using Newtonsoft.Json;

namespace AfterSchoolLibrary.Integrations
{
    public interface ISterlingBearerTokenQuery
    {
        Task<SterlingToken> GetAsync(SterlingCredentials credentials);
    }

    public class SterlingBearerTokenQuery : ISterlingBearerTokenQuery
    {
        private readonly IHttpClient httpClient;
        private readonly IJsonSerializer jsonSerializer;

        public SterlingBearerTokenQuery(IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            this.httpClient = httpClient;
            this.jsonSerializer = jsonSerializer;
        }

        public async Task<SterlingToken> GetAsync(SterlingCredentials credentials)
        {
            var request = new HttpRequest
            {
                Url = $"{credentials.Endpoint}/v2/oauth",
                Method = HttpMethod.POST,
            };
            request.AddBasicAuthentication(credentials.ClientId, credentials.ClientSecret);
            
            await request.AddFormUrlEncodedBody(new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
            });

            var response = await httpClient.ExecuteAsync(request);

            if (response.StatusCode != 200)
                throw new Exception($"Sterling token request failed with status code {response.StatusCode}; body: {response.Body}");

            var responseBody = jsonSerializer.Deserialize<SterlingTokenResponse>(response.Body);
            return new SterlingToken
            {
                Token = responseBody.AccessToken,
                ExpiresIn = TimeSpan.FromSeconds(responseBody.ExpiresIn),
            };
        }
    }

    public class SterlingTokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class SterlingToken
    {
        public string Token { get; set; }
        public TimeSpan ExpiresIn { get; set; }
    }
}