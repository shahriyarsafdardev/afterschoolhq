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
        private const int AccountId = 13;
        private const int WebUserId = 7;
        private Mock<IBackgroundCheckRepository> backgroundCheckRepository;
        private Mock<ISterlingCandidateQuery> createCandidateQuery;
        private Mock<ISterlingCreateScreeningQuery> createScreeningQuery;
        private SterlingCredentials credentials;
        private SystemMessages messages;
        private Mock<ISterlingBearerTokenQuery> tokenQuery;
        private WebUser webUser;
        private BackgroundCheckWorkflow workflow;

        [SetUp]
        public void SetUp()
        {
            credentials = new SterlingCredentials("endpoint", "clientId", "clientSecret", "packageId", "accountId");
            tokenQuery = new Mock<ISterlingBearerTokenQuery>();
            createCandidateQuery = new Mock<ISterlingCandidateQuery>();
            createScreeningQuery = new Mock<ISterlingCreateScreeningQuery>();
            backgroundCheckRepository = new Mock<IBackgroundCheckRepository>();
            workflow = new BackgroundCheckWorkflow(credentials, tokenQuery.Object, createCandidateQuery.Object, createScreeningQuery.Object,
                backgroundCheckRepository.Object);

            webUser = new WebUser {ID = WebUserId};
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
            createCandidateQuery.Setup(x => x.CreateAsync(credentials, token.Token, webUser)).ReturnsAsync(CandidateId);
            createScreeningQuery.Setup(x => x.CreateAsync(credentials, token.Token, CandidateId)).ReturnsAsync(BackgroundCheckUrl);
            backgroundCheckRepository
                .Setup(x => x.Save(It.IsAny<BackgroundCheck>()))
                .Callback<BackgroundCheck>(x => capturedBackgroundCheck = x)
                .Returns(new SystemMessages());

            var result = await workflow.RequestBackgroundCheck(webUser, AccountId, messages);

            Assert.That(result.BackgroundCheckUrl, Is.EqualTo(BackgroundCheckUrl));
            Assert.That(messages, Has.Count.EqualTo(0));
            Assert.That(messages.HasNoErrors());
            Assert.That(capturedBackgroundCheck.WebUserId, Is.EqualTo(WebUserId));
            Assert.That(capturedBackgroundCheck.RequestedForFiscalAccountId, Is.EqualTo(AccountId));
            Assert.That(capturedBackgroundCheck.BackgroundCheckStatusId, Is.EqualTo(BackgroundCheckStatus.EmailSentToVolunteer));
            Assert.That(capturedBackgroundCheck.BackgroundCheckSourceId, Is.EqualTo(BackgroundCheckSource.Sterling));
            Assert.That(capturedBackgroundCheck.SterlingCandidateId, Is.EqualTo(CandidateId));
            Assert.That(capturedBackgroundCheck.SterlingInviteUrl, Is.EqualTo(BackgroundCheckUrl));
            Assert.That(capturedBackgroundCheck.ExpirationDate, Is.EqualTo(DateTime.UtcNow.AddYears(1)).Within(1).Seconds);
            Assert.That(capturedBackgroundCheck.SterlingEmailSentDate, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
            Assert.That(capturedBackgroundCheck.VolunteerCompletedDate, Is.Null);
            Assert.That(capturedBackgroundCheck.SterlingReviewCompleteDate, Is.Null);
            Assert.That(capturedBackgroundCheck.ManualReviewCompleteDate, Is.Null);
        }

        [Test]
        public async Task RequestBackgroundCheckHasAnErrorAuthenticatingTest()
        {
            const string exceptionText = "Unable to reach host";
            BackgroundCheck capturedBackgroundCheck = null;
            tokenQuery.Setup(x => x.GetAsync(credentials)).ThrowsAsync(new Exception(exceptionText));
            backgroundCheckRepository
                .Setup(x => x.Save(It.IsAny<BackgroundCheck>()))
                .Callback<BackgroundCheck>(x => capturedBackgroundCheck = x)
                .Returns(new SystemMessages());

            var result = await workflow.RequestBackgroundCheck(webUser, AccountId, messages);

            Assert.That(result.BackgroundCheckUrl, Is.Null);
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages.HasErrors());

            var systemMessage = messages.ToList().First();
            Assert.That(systemMessage.MessageType, Is.EqualTo(MessageType.ApiError));
            Assert.That(systemMessage.Text, Is.EqualTo(exceptionText));
            Assert.That(capturedBackgroundCheck.WebUserId, Is.EqualTo(WebUserId));
            Assert.That(capturedBackgroundCheck.RequestedForFiscalAccountId, Is.EqualTo(AccountId));
            Assert.That(capturedBackgroundCheck.BackgroundCheckStatusId, Is.EqualTo(BackgroundCheckStatus.BackgroundCheckRequested));
            Assert.That(capturedBackgroundCheck.BackgroundCheckSourceId, Is.EqualTo(BackgroundCheckSource.Sterling));
            Assert.That(capturedBackgroundCheck.SterlingCandidateId, Is.Null);
            Assert.That(capturedBackgroundCheck.SterlingInviteUrl, Is.Null);
            Assert.That(capturedBackgroundCheck.ExpirationDate, Is.Null);
            Assert.That(capturedBackgroundCheck.SterlingEmailSentDate, Is.Null);
            Assert.That(capturedBackgroundCheck.VolunteerCompletedDate, Is.Null);
            Assert.That(capturedBackgroundCheck.SterlingReviewCompleteDate, Is.Null);
            Assert.That(capturedBackgroundCheck.ManualReviewCompleteDate, Is.Null);
        }

        [Test]
        public async Task RequestBackgroundCheckHasAnErrorSendingEmailTest()
        {
            var token = new SterlingToken
            {
                Token = "new-token",
                ExpiresIn = TimeSpan.FromHours(1),
            };
            const string exceptionText = "Unable to send email";
            BackgroundCheck capturedBackgroundCheck = null;
            tokenQuery.Setup(x => x.GetAsync(credentials)).ReturnsAsync(token);
            createCandidateQuery.Setup(x => x.CreateAsync(credentials, token.Token, webUser)).ReturnsAsync(CandidateId);
            createScreeningQuery.Setup(x => x.CreateAsync(credentials, token.Token, CandidateId)).ThrowsAsync(new Exception(exceptionText));
            backgroundCheckRepository
                .Setup(x => x.Save(It.IsAny<BackgroundCheck>()))
                .Callback<BackgroundCheck>(x => capturedBackgroundCheck = x)
                .Returns(new SystemMessages());

            var result = await workflow.RequestBackgroundCheck(webUser, AccountId, messages);

            Assert.That(result.BackgroundCheckUrl, Is.Null);
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages.HasErrors());

            var systemMessage = messages.ToList().First();
            Assert.That(systemMessage.MessageType, Is.EqualTo(MessageType.ApiError));
            Assert.That(systemMessage.Text, Is.EqualTo(exceptionText));
            Assert.That(capturedBackgroundCheck.WebUserId, Is.EqualTo(WebUserId));
            Assert.That(capturedBackgroundCheck.RequestedForFiscalAccountId, Is.EqualTo(AccountId));
            Assert.That(capturedBackgroundCheck.BackgroundCheckStatusId, Is.EqualTo(BackgroundCheckStatus.BackgroundCheckRequested));
            Assert.That(capturedBackgroundCheck.BackgroundCheckSourceId, Is.EqualTo(BackgroundCheckSource.Sterling));
            Assert.That(capturedBackgroundCheck.SterlingCandidateId, Is.EqualTo(CandidateId));
            Assert.That(capturedBackgroundCheck.SterlingInviteUrl, Is.Null);
            Assert.That(capturedBackgroundCheck.ExpirationDate, Is.Null);
            Assert.That(capturedBackgroundCheck.SterlingEmailSentDate, Is.Null);
            Assert.That(capturedBackgroundCheck.VolunteerCompletedDate, Is.Null);
            Assert.That(capturedBackgroundCheck.SterlingReviewCompleteDate, Is.Null);
            Assert.That(capturedBackgroundCheck.ManualReviewCompleteDate, Is.Null);
        }
    }
}