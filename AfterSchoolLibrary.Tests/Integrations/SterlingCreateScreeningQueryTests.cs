using System;
using System.Threading.Tasks;
using AfterSchoolLibrary.Integrations;
using Emmersion.Http;
using Moq;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests.Integrations
{
    public class SterlingCreateScreeningQueryTests
    {
        [Test]
        public async Task SuccessfullyCreateSterlingScreeningTest()
        {
            HttpRequest capturedRequest = null;
            var response = new HttpResponse(201, new HttpHeaders(), "BODY");

            var http = new Mock<IHttpClient>();
            http.Setup(x => x.ExecuteAsync(It.IsAny<IHttpRequest>()))
                .Callback<IHttpRequest>(x => capturedRequest = (HttpRequest) x)
                .ReturnsAsync(response);

            const string inviteLink = "invite-link";
            SterlingScreeningRequest capturedScreeningRequest = null;
            var json = new Mock<IJsonSerializer>();
            json.Setup(x => x.Serialize(It.IsAny<SterlingScreeningRequest>()))
                .Callback<SterlingScreeningRequest>(x => capturedScreeningRequest = x)
                .Returns("requestBody");
            json.Setup(x => x.Deserialize<SterlingScreeningResponse>("BODY")).Returns(new SterlingScreeningResponse
            {
                Invite = new SterlingScreeningInviteResponse
                {
                    Link = inviteLink,
                },
            });

            var credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");

            var query = new SterlingCreateScreeningQuery(http.Object, json.Object);
            const string token = "my-token";
            var candidateId = Guid.NewGuid().ToString();

            var result = await query.CreateAsync(credentials, token, candidateId);

            Assert.That(result, Is.EqualTo(inviteLink));

            Assert.That(capturedRequest.Url, Is.EqualTo("endpoint/v2/screenings"));
            Assert.That(capturedRequest.Method, Is.EqualTo(HttpMethod.POST));
            Assert.That(capturedRequest.Headers.GetValue("Authorization"), Is.EqualTo($"Bearer {token}"));
            Assert.That(capturedRequest.Headers.GetValue("Content-Type"), Is.EqualTo("application/json"));

            Assert.That(capturedScreeningRequest.CandidateId, Is.EqualTo(candidateId));
            Assert.That(capturedScreeningRequest.PackageId, Is.EqualTo(credentials.PackageId));
            Assert.That(capturedScreeningRequest.Invite.Method, Is.EqualTo("link"));
        }

        [Test]
        public void ReceiveErrorCreatingSterlingScreeningTest()
        {
            var response = new HttpResponse(401, new HttpHeaders(), "BODY");
            var http = new Mock<IHttpClient>();
            http.Setup(x => x.ExecuteAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);
            var credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");
            var json = new Mock<IJsonSerializer>();
            var query = new SterlingCreateScreeningQuery(http.Object, json.Object);
            var candidateId = Guid.NewGuid().ToString();

            Assert.ThrowsAsync<Exception>(async () => await query.CreateAsync(credentials, "token", candidateId));
        }

        [Test]
        public void DeserializingSterlingScreeningResponseTest()
        {
            var result = new JsonSerializer().Deserialize<SterlingScreeningResponse>(@"{
  ""id"": ""1801020641"",
  ""packageId"": ""92979"",
  ""trustId"": ""ac2eb527-a135-4290-a702-7517762d3acb"",
  ""accountId"": ""44771"",
  ""candidateId"": ""3e4f84e3-9078-4451-a8b8-5897f4f4f4e3"",
  ""status"": ""PreDraft"",
  ""result"": ""Pending"",
  ""invite"": {
    ""method"": ""link"",
    ""link"": ""https://qaworkforce.sterlingdirect.com/Account/Begin?PORTALDATA=Dsr7EGCn1HevF87Ddgs0FvbSAcef006RiivHdKMUJWyeXVQvfsfCCXo10syHs3rIMizMkEvWfj00Ceu7%7cJ1D2hPfah426I%2f4tgLxiRkpF6w%3d""
  },
  ""jobPosition"": ""Education Package"",
  ""links"": {
    ""admin"": {
      ""web"": ""https://int.sterlingcheck.app/interactive-summary/#/order/wFrJ20uTcM0tDS7qxPY4NA%3d%3d""
    }
  },
  ""callback"": {
    ""uri"": ""https://my-company.com/screenings-callback""
  },
  ""reportItems"": [],
  ""updatedAt"": ""2021-05-27T00:02:00Z"",
  ""estimatedCompletionTime"": ""2021-05-27T00:02:00Z""
}");

            Assert.That(result.Invite.Link,
                Is.EqualTo(
                    "https://qaworkforce.sterlingdirect.com/Account/Begin?PORTALDATA=Dsr7EGCn1HevF87Ddgs0FvbSAcef006RiivHdKMUJWyeXVQvfsfCCXo10syHs3rIMizMkEvWfj00Ceu7%7cJ1D2hPfah426I%2f4tgLxiRkpF6w%3d"));
        }

        [Test]
        public void SerializingSterlingScreeningRequestTest()
        {
            var result = new JsonSerializer().Serialize(new SterlingScreeningRequest
            {
                CandidateId = "3e4f84e3-9078-4451-a8b8-5897f4f4f4e3",
                PackageId = "92979",
                Invite = new SterlingScreeningInviteRequest {Method = "link"},
            });

            Assert.That(result,
                Is.EqualTo("{\"candidateId\":\"3e4f84e3-9078-4451-a8b8-5897f4f4f4e3\",\"packageId\":\"92979\",\"invite\":{\"method\":\"link\"}}"));
        }
    }
}