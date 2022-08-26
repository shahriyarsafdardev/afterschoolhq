using System;
using System.Linq;
using System.Threading.Tasks;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using Moq;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests.Integrations
{
    public class BackgroundCheckWorkflowTests
    {
        private const string BackgroundCheckUrl = "url";
        private const string CandidateId = "candidate-id";
        private Mock<ISterlingCreateCandidateQuery> createCandidateQuery;
        private Mock<ISterlingCreateScreeningQuery> createScreeningQuery;
        private Mock<IBackgroundCheckRepository> backgroundCheckRepository;
        private SterlingCredentials credentials;
        private SystemMessages messages;
        private Registrant registrant;
        private Mock<ISterlingBearerTokenQuery> tokenQuery;
        private BackgroundCheckWorkflow workflow;

        [SetUp]
        public void SetUp()
        {
            credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId");
            tokenQuery = new Mock<ISterlingBearerTokenQuery>();
            createCandidateQuery = new Mock<ISterlingCreateCandidateQuery>();
            createScreeningQuery = new Mock<ISterlingCreateScreeningQuery>();
            backgroundCheckRepository = new Mock<IBackgroundCheckRepository>();
            workflow = new BackgroundCheckWorkflow(credentials, tokenQuery.Object, createCandidateQuery.Object, createScreeningQuery.Object,
                backgroundCheckRepository.Object);

            registrant = new Registrant();
            messages = new SystemMessages();
        }

        [Test]
        public async Task RequestBackgroundCheckIsSuccessfulTest()
        {
            var token = new SterlingToken
            {
                Token = "new-token",
                ExpiresIn = TimeSpan.FromHours(1),
            };
            BackgroundCheck capturedBackgroundCheck = null;
            tokenQuery.Setup(x => x.GetAsync(credentials)).ReturnsAsync(token);
            createCandidateQuery.Setup(x => x.CreateAsync(credentials, token.Token, registrant)).ReturnsAsync(CandidateId);
            createScreeningQuery.Setup(x => x.CreateAsync(credentials, token.Token, CandidateId)).ReturnsAsync(BackgroundCheckUrl);
            backgroundCheckRepository.Setup(x => x.Save(It.IsAny<BackgroundCheck>())).Callback<BackgroundCheck>(x => capturedBackgroundCheck = x);

            var result = await workflow.RequestBackgroundCheck(registrant, messages);

            Assert.That(result.BackgroundCheckUrl, Is.EqualTo(BackgroundCheckUrl));
            Assert.That(messages, Has.Count.EqualTo(0));
            Assert.That(messages.HasNoErrors());
        }

        [Test]
        public async Task RequestBackgroundCheckHasAnErrorTest()
        {
            const string exceptionText = "Unable to reach host";
            tokenQuery.Setup(x => x.GetAsync(credentials)).ThrowsAsync(new Exception(exceptionText));

            var result = await workflow.RequestBackgroundCheck(registrant, messages);

            Assert.That(result.BackgroundCheckUrl, Is.Null);
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages.HasErrors());

            var systemMessage = messages.ToList().First();
            Assert.That(systemMessage.MessageType, Is.EqualTo(MessageType.ApiError));
            Assert.That(systemMessage.Text, Is.EqualTo(exceptionText));
        }
    }
}