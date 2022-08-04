using System;
using System.Threading.Tasks;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using Emmersion.Http;
using Moq;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests.Integrations
{
    public class SterlingCreateCandidateQueryTests
    {
        [Test]
        public async Task SuccessfullyCreateSterlingCandidateTest()
        {
            HttpRequest capturedRequest = null;
            var response = new HttpResponse(201, new HttpHeaders(), "BODY");

            var http = new Mock<IHttpClient>();
            http.Setup(x => x.ExecuteAsync(It.IsAny<IHttpRequest>()))
                .Callback<IHttpRequest>(x => capturedRequest = (HttpRequest) x)
                .ReturnsAsync(response);

            const string candidateId = "sterling-candidate-id";
            SterlingCandidateRequest capturedCandidateRequest = null;
            var json = new Mock<IJsonSerializer>();
            json.Setup(x => x.Serialize(It.IsAny<SterlingCandidateRequest>()))
                .Callback<SterlingCandidateRequest>(x => capturedCandidateRequest = x)
                .Returns("requestBody");
            json.Setup(x => x.Deserialize<SterlingCandidateResponse>("BODY")).Returns(new SterlingCandidateResponse
            {
                Id = candidateId,
            });

            var credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");
            var registrant = new Registrant
            {
                FirstName = "first-name",
                LastName = "last-name",
                Email = "email",
                WebUserId = 789,
            };

            var query = new SterlingCreateCandidateQuery(http.Object, json.Object);
            const string token = "my-token";

            var result = await query.CreateAsync(credentials, token, registrant);

            Assert.That(result, Is.EqualTo(candidateId));

            Assert.That(capturedRequest.Url, Is.EqualTo("endpoint/v2/candidates"));
            Assert.That(capturedRequest.Method, Is.EqualTo(HttpMethod.POST));
            Assert.That(capturedRequest.Headers.GetValue("Authorization"), Is.EqualTo($"Bearer {token}"));
            Assert.That(capturedRequest.Headers.GetValue("Content-Type"), Is.EqualTo("application/json"));

            Assert.That(capturedCandidateRequest.GivenName, Is.EqualTo(registrant.FirstName));
            Assert.That(capturedCandidateRequest.FamilyName, Is.EqualTo(registrant.LastName));
            Assert.That(capturedCandidateRequest.Email, Is.EqualTo(registrant.Email));
            Assert.That(capturedCandidateRequest.ClientReferenceId, Is.EqualTo(registrant.WebUserId.ToString()));
        }

        [Test]
        public void ReceiveErrorCreatingSterlingCandidateTest()
        {
            var response = new HttpResponse(401, new HttpHeaders(), "BODY");
            var http = new Mock<IHttpClient>();
            http.Setup(x => x.ExecuteAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);
            var credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");
            var json = new Mock<IJsonSerializer>();
            var query = new SterlingCreateCandidateQuery(http.Object, json.Object);
            var registrant = new Registrant
            {
                FirstName = "first-name",
                LastName = "last-name",
                Email = "email",
                WebUserId = 789,
            };

            Assert.ThrowsAsync<Exception>(async () => await query.CreateAsync(credentials, "token", registrant));
        }

        [Test]
        public void DeserializingSterlingCandidateResponseTest()
        {
            var result = new JsonSerializer().Deserialize<SterlingCandidateResponse>(@"{
  ""id"": ""3e4f84e3-9078-4451-a8b8-5897f4f4f4e3"",
  ""clientReferenceId"": ""999124441"",
  ""email"": ""john.doe@example.com"",
  ""givenName"": ""John"",
  ""familyName"": ""Doe"",
  ""confirmedNoMiddleName"": true,
  ""dob"": ""XXXX-01-20"",
  ""ssn"": ""XXXXX6789"",
  ""phone"": ""+14041231234"",
  ""address"": {
    ""addressLine"": ""123 Main Street"",
    ""municipality"": ""Orlando"",
    ""regionCode"": ""US-FL"",
    ""postalCode"": ""12345"",
    ""countryCode"": ""US""
  },
  ""additionalAddresses"": [
    {
      ""addressLine"": ""123 PEACHTREE PLACE"",
      ""municipality"": ""ATLANTA"",
      ""regionCode"": ""US-GA"",
      ""postalCode"": ""30005"",
      ""countryCode"": ""US"",
      ""validFrom"": ""2003-08-01"",
      ""validTo"": ""2016-07-08""
    },
    {
      ""addressLine"": ""221B Baker Street"",
      ""municipality"": ""London"",
      ""regionCode"": ""GB-LND"",
      ""postalCode"": ""NW1 6XE"",
      ""countryCode"": ""GB"",
      ""validFrom"": ""2016-07-08"",
      ""validTo"": ""2017-10-27""
    }
  ],
  ""aliases"": [
    {
      ""givenName"": ""Robert"",
      ""familyName"": ""Saur"",
      ""middleName"": ""William"",
      ""confirmedNoMiddleName"": false
    },
    {
      ""givenName"": ""Simon"",
      ""familyName"": ""Duck"",
      ""confirmedNoMiddleName"": true
    }
  ],
  ""educationHistory"": [
    {
      ""schoolName"": ""UW"",
      ""schoolType"": ""University"",
      ""degree"": {
        ""major"": ""Software Systems"",
        ""degreeName"": ""B.S."",
        ""degreeType"": ""Bachelors"",
        ""graduationDate"": ""2010-06-01"",
        ""degreeCompleted"": true,
        ""comments"": ""something""
      },
      ""schoolIDNumber"": ""UW12345"",
      ""address"": {
        ""addressLine"": ""University District"",
        ""municipality"": ""SEATTLE"",
        ""regionCode"": ""US-WA"",
        ""postalCode"": ""98001"",
        ""countryCode"": ""US"",
        ""validFrom"": ""2008-08-01""
      },
      ""aliasGivenName"": ""aliasFirstName1"",
      ""aliasFamilyName"": ""aliasLastName1"",
      ""department"": ""Computer Science"",
      ""startDate"": ""2008-08-01"",
      ""endDate"": ""2010-05-01"",
      ""email"": ""uwsomeemail@uwsomeemail.com"",
      ""phone"": ""14041231234"",
      ""fax"": ""+1234567890"",
      ""notes"": ""nothing"",
      ""type"": ""default""
    }
  ],
  ""employmentHistory"": [
    {
      ""employerName"": ""SomeEmployer"",
      ""currentEmployer"": true,
      ""jobTitle"": ""SomeJobTitle"",
      ""startDate"": ""2008-10-02"",
      ""endDate"": ""2012-10-25"",
      ""employmentType"": ""FullTime"",
      ""department"": ""IT"",
      ""address"": {
        ""addressLine"": ""ABC company"",
        ""municipality"": ""SEATTLE"",
        ""regionCode"": ""US-WA"",
        ""postalCode"": ""98001"",
        ""countryCode"": ""US"",
        ""validFrom"": ""2008-08-01""
      },
      ""permissionToContact"": true,
      ""reasonForLeaving"": ""nothing"",
      ""salary"": {
        ""currency"": ""12345678"",
        ""startingSalary"": ""140,000"",
        ""endingSalary"": ""100,000""
      },
      ""verification"": {
        ""supervisorGivenName"": ""supervisorFirstName"",
        ""supervisorFamilyName"": ""supervisorLastName"",
        ""supervisorMiddleName"": ""supervisorMiddleName"",
        ""address"": {
          ""addressLine"": ""ABC company"",
          ""municipality"": ""SEATTLE"",
          ""regionCode"": ""US-WA"",
          ""postalCode"": ""98001"",
          ""countryCode"": ""US"",
          ""validFrom"": ""2008-08-01""
        },
        ""email"": ""abcsomeemail@abcsomeemail.com"",
        ""phone"": ""14041231234"",
        ""fax"": ""+1234567890""
      },
      ""verifyEmployer"": true,
      ""notes"": ""nothing"",
      ""type"": ""default""
    }
  ],
  ""licenses"": [
    {
      ""issuingAgency"": {
        ""name"": ""Some Institute"",
        ""address"": {
          ""addressLine"": ""ABC company"",
          ""municipality"": ""SEATTLE"",
          ""regionCode"": ""US-WA"",
          ""postalCode"": ""98001"",
          ""countryCode"": ""US"",
          ""validFrom"": ""2008-08-01""
        }
      },
      ""number"": ""S12345"",
      ""name"": ""Nursing Certification"",
      ""startDate"": ""2016-01-01"",
      ""status"": ""active"",
      ""notes"": ""some notes""
    }
  ],
  ""screeningIds"": [],
  ""driversLicense"": {
    ""type"": ""personal"",
    ""licenseNumber"": ""S1234567"",
    ""issuingAgency"": ""CA""
  }
}");

            Assert.That(result.Id, Is.EqualTo("3e4f84e3-9078-4451-a8b8-5897f4f4f4e3"));
        }

        [Test]
        public void SerializingSterlingCandidateRequestTest()
        {
            var result = new JsonSerializer().Serialize(new SterlingCandidateRequest
            {
                GivenName = "Jane",
                FamilyName = "Doe",
                Email = "jane@example.org",
                ClientReferenceId = "1234",
            });

            Assert.That(result, Is.EqualTo("{\"givenName\":\"Jane\",\"familyName\":\"Doe\",\"email\":\"jane@example.org\",\"clientReferenceId\":\"1234\"}"));
        }

        [Test]
        public void HttpRequestWithJsonBodyAndBearerTokenTest()
        {
            const string bearerToken = "my-bearer-token";
            var request = new HttpRequest
            {
                Url = "my-endpoint",
                Method = HttpMethod.POST,
                Headers = new HttpHeaders()
                    .Add("Authorization", $"Bearer {bearerToken}")
                    .Add("Content-Type", "application/json"),
                Body = "{\"id\":\"candidate-id\",\"other-id\":\"my-other-id\"}",
            };

            Assert.That(request.Url, Is.EqualTo("my-endpoint"));
            Assert.That(request.Method, Is.EqualTo(HttpMethod.POST));

            Assert.That(request.Headers.GetValue("Authorization"), Is.EqualTo($"Bearer {bearerToken}"));
            Assert.That(request.Headers.GetValue("Content-Type"), Is.EqualTo("application/json"));

            Assert.That(request.Body, Is.EqualTo("{\"id\":\"candidate-id\",\"other-id\":\"my-other-id\"}"));
        }
    }
}