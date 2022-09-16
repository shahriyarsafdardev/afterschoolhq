using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AfterSchoolLibrary.Integrations;
using Emmersion.Http;
using Moq;
using NUnit.Framework;
using HttpRequest = Emmersion.Http.HttpRequest;
using HttpResponse = Emmersion.Http.HttpResponse;

namespace AfterSchoolLibrary.Tests.Integrations
{
    public class SterlingBearerTokenQueryTests
    {
        [Test]
        public async Task SuccessfullyGetSterlingBearerTokenTest()
        {
            HttpRequest request = null;
            var response = new HttpResponse(200, new HttpHeaders(), "BODY");
            var http = new Mock<IHttpClient>();
            http.Setup(x => x.ExecuteAsync(It.IsAny<IHttpRequest>()))
                .Callback<IHttpRequest>(x => { request = (HttpRequest) x; })
                .ReturnsAsync(response);

            const string accessToken = "ACCESS_TOKEN";
            const int expiresIn = 1234;
            var json = new Mock<IJsonSerializer>();
            json.Setup(x => x.Deserialize<SterlingTokenResponse>("BODY")).Returns(new SterlingTokenResponse
            {
                AccessToken = accessToken,
                ExpiresIn = expiresIn,
            });

            var query = new SterlingBearerTokenQuery(http.Object, json.Object);

            var credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");
            var result = await query.GetAsync(credentials);

            Assert.That(request.Url, Is.EqualTo("endpoint/v2/oauth"));
            Assert.That(request.Method, Is.EqualTo(HttpMethod.POST));

            var requestParameters = HttpUtility.ParseQueryString(await request.GetContent().ReadAsStringAsync());
            Assert.That(requestParameters["grant_type"], Is.EqualTo("client_credentials"));

            Assert.That(result.Token, Is.EqualTo(accessToken));
            Assert.That(result.ExpiresIn, Is.EqualTo(TimeSpan.FromSeconds(expiresIn)));
        }

        [Test]
        public void ReceiveErrorGettingSterlingBearerTokenTest()
        {
            var response = new HttpResponse(401, new HttpHeaders(), "BODY");
            var http = new Mock<IHttpClient>();
            http.Setup(x => x.ExecuteAsync(It.IsAny<IHttpRequest>())).ReturnsAsync(response);

            var json = new Mock<IJsonSerializer>();

            var query = new SterlingBearerTokenQuery(http.Object, json.Object);

            var credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");
            Assert.ThrowsAsync<Exception>(async () => await query.GetAsync(credentials));
        }

        [Test]
        public void DeserializingSterlingTokenResponseTest()
        {
            var result = new JsonSerializer().Deserialize<SterlingTokenResponse>(@"{
  ""access_token"": ""my-cool-access-token"",
  ""token_type"": ""bearer"",
  ""expires_in"": ""36000""
}");

            Assert.That(result.AccessToken, Is.EqualTo("my-cool-access-token"));
            Assert.That(result.ExpiresIn, Is.EqualTo(36000));
        }

        [Test]
        public async Task HttpRequestWithFormUrlEncodedBodyAndBasicAuthTest()
        {
            var request = new HttpRequest
            {
                Url = "my-endpoint",
                Method = HttpMethod.POST,
            };
            const string clientId = "ClientId";
            const string clientSecret = "ClientSecret";
            request.AddBasicAuthentication(clientId, clientSecret);
            await request.AddFormUrlEncodedBody(new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
            });

            Assert.That(request.Url, Is.EqualTo("my-endpoint"));
            Assert.That(request.Method, Is.EqualTo(HttpMethod.POST));

            var expectedEncodedAuthorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
            Assert.That(request.Headers.GetValue("Authorization"), Is.EqualTo($"Basic {expectedEncodedAuthorization}"));
            Assert.That(request.Headers.GetValue("Content-Type"), Is.EqualTo("application/x-www-form-urlencoded"));

            var requestParameters = HttpUtility.ParseQueryString(await request.GetContent().ReadAsStringAsync());
            Assert.That(requestParameters["grant_type"], Is.EqualTo("client_credentials"));
        }
    }
}