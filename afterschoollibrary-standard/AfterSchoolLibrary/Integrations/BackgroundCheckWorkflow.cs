using System;
using System.Threading.Tasks;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibrary.Integrations
{
    public interface IBackgroundCheckWorkflow
    {
        Task<BackgroundCheckRequestResult> RequestBackgroundCheck(WebUser webUser, SystemMessages messages);
    }

    public class BackgroundCheckWorkflow : IBackgroundCheckWorkflow
    {
        private readonly IBackgroundCheckRepository backgroundCheckRepository;
        private readonly ISterlingCreateCandidateQuery createCandidateQuery;
        private readonly ISterlingCreateScreeningQuery createScreeningQuery;
        private readonly SterlingCredentials credentials;
        private readonly ISterlingBearerTokenQuery tokenQuery;

        public BackgroundCheckWorkflow(SterlingCredentials credentials, ISterlingBearerTokenQuery tokenQuery,
            ISterlingCreateCandidateQuery createCandidateQuery, ISterlingCreateScreeningQuery createScreeningQuery,
            IBackgroundCheckRepository backgroundCheckRepository)
        {
            this.credentials = credentials;
            this.tokenQuery = tokenQuery;
            this.createCandidateQuery = createCandidateQuery;
            this.createScreeningQuery = createScreeningQuery;
            this.backgroundCheckRepository = backgroundCheckRepository;
        }

        public async Task<BackgroundCheckRequestResult> RequestBackgroundCheck(WebUser webUser, SystemMessages messages)
        {
            var authenticated = false;
            string candidateId = null;
            string backgroundCheckUrl = null;
            try
            {
                var token = await tokenQuery.GetAsync(credentials);
                authenticated = true;
                candidateId = await createCandidateQuery.CreateAsync(credentials, token.Token, webUser);
                backgroundCheckUrl = await createScreeningQuery.CreateAsync(credentials, token.Token, candidateId);
            }
            catch (Exception ex)
            {
                messages.Add(new SystemMessage(ex.Message, MessageType.ApiError));
            }

            SterlingProcessStep currentSterlingProcessStep;
            if (!authenticated)
                currentSterlingProcessStep = SterlingProcessStep.FailedToAuthenticate;
            else if (candidateId is null)
                currentSterlingProcessStep = SterlingProcessStep.FailedToCreateCandidate;
            else if (backgroundCheckUrl is null)
                currentSterlingProcessStep = SterlingProcessStep.FailedToCreateScreening;
            else
                currentSterlingProcessStep = SterlingProcessStep.SuccessfullyCreatedScreening;

            var backgroundCheck = new BackgroundCheck
            {
                WebUserId = webUser.ID!.Value,
                SterlingProcessStepId = currentSterlingProcessStep,
                BackgroundCheckStatusId = BackgroundCheckStatus.PendingReview,
                ExternalCandidateId = candidateId,
                SterlingInviteUrl = backgroundCheckUrl,
                BackgroundCheckSourceId = BackgroundCheckSource.Sterling,
            };
            messages.AddRange(backgroundCheckRepository.Save(backgroundCheck));
            return new BackgroundCheckRequestResult {BackgroundCheckUrl = backgroundCheckUrl};
        }
    }

    public class BackgroundCheckRequestResult
    {
        public string BackgroundCheckUrl { get; set; }
    }

    public class SterlingCredentials
    {
        public SterlingCredentials(string endpoint, string clientId, string clientSecret, string packageId)
        {
            Endpoint = endpoint;
            ClientId = clientId;
            ClientSecret = clientSecret;
            PackageId = packageId;
        }

        public string Endpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string PackageId { get; set; }
    }
}