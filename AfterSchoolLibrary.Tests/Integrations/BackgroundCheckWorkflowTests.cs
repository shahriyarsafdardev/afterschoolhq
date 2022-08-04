using System;
using System.Threading.Tasks;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using Moq;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests.Integrations
{
    public class BackgroundCheckWorkflowTests
    {
        [Test]
        public async Task RequestBackgroundCheckTest()
        {
            var credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");

            var token = new SterlingToken
            {
                Token = "new-token",
                ExpiresIn = TimeSpan.FromHours(1),
            };
            var tokenQuery = new Mock<ISterlingBearerTokenQuery>();
            tokenQuery.Setup(x => x.GetAsync(credentials)).ReturnsAsync(token);

            var registrant = new Registrant();
            const string candidateId = "candidate-id";
            var createCandidateQuery = new Mock<ISterlingCreateCandidateQuery>();
            createCandidateQuery.Setup(x => x.CreateAsync(credentials, token.Token, registrant)).ReturnsAsync(candidateId);

            const string linkInviteUrl = "url";
            var createScreeningQuery = new Mock<ISterlingCreateScreeningQuery>();
            createScreeningQuery.Setup(x => x.CreateAsync(credentials, token.Token, candidateId)).ReturnsAsync(linkInviteUrl);

            var workflow = new BackgroundCheckWorkflow(credentials, tokenQuery.Object, createCandidateQuery.Object, createScreeningQuery.Object);

            var result = await workflow.RequestBackgroundCheck(registrant);

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.InviteLink, Is.EqualTo(linkInviteUrl));
        }
    }
}