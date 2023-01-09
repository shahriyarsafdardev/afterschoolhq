using System;
using System.Threading.Tasks;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibrary.Integrations
{
    public interface IBackgroundCheckWorkflow
    {
        Task<BackgroundCheckRequestResult> RequestBackgroundCheck(WebUser webUser, int accountId, SystemMessages messages);
    }

    public class BackgroundCheckWorkflow : IBackgroundCheckWorkflow
    {
        private readonly IBackgroundCheckRepository backgroundCheckRepository;
        private readonly ISterlingCandidateQuery candidateQuery;
        private readonly ISterlingCreateScreeningQuery createScreeningQuery;
        private readonly SterlingCredentials credentials;
        private readonly ISterlingBearerTokenQuery tokenQuery;

        public BackgroundCheckWorkflow(SterlingCredentials credentials, ISterlingBearerTokenQuery tokenQuery,
            ISterlingCandidateQuery candidateQuery, ISterlingCreateScreeningQuery createScreeningQuery,
            IBackgroundCheckRepository backgroundCheckRepository)
        {
            this.credentials = credentials;
            this.tokenQuery = tokenQuery;
            this.candidateQuery = candidateQuery;
            this.createScreeningQuery = createScreeningQuery;
            this.backgroundCheckRepository = backgroundCheckRepository;
        }

        public async Task<BackgroundCheckRequestResult> RequestBackgroundCheck(WebUser webUser, int accountId, SystemMessages messages)
        {
            string candidateId = null;
            string backgroundCheckUrl = null;
            try
            {
                var token = await tokenQuery.GetAsync(credentials);
                candidateId = await candidateQuery.GetByEmaileAsync(credentials, token.Token, webUser);
                
                if(candidateId.IsEmpty())
                    candidateId = await candidateQuery.CreateAsync(credentials, token.Token, webUser);
                
                backgroundCheckUrl = await createScreeningQuery.CreateAsync(credentials, token.Token, candidateId);
            }
            catch (Exception ex)
            {
                messages.Add(new SystemMessage(ex.Message, MessageType.ApiError));
            }

            var status = candidateId is null || backgroundCheckUrl is null
                ? BackgroundCheckStatus.BackgroundCheckRequested
                : BackgroundCheckStatus.EmailSentToVolunteer;
            
            var result = candidateId is null || backgroundCheckUrl is null
                ? BackgroundCheckResult.BackgroundCheckRequested
                : BackgroundCheckResult.EmailSentToVolunteer;

            var backgroundCheck = new BackgroundCheck
            {
                WebUserId = webUser.ID!.Value,
                RequestedForFiscalAccountId = accountId,
                BackgroundCheckStatusId = status,
                BackgroundCheckResultId = result,
                SterlingCandidateId = candidateId,
                SterlingInviteUrl = backgroundCheckUrl,
                BackgroundCheckSourceId = BackgroundCheckSource.Sterling,
                SterlingEmailSentDate = backgroundCheckUrl is null ? (DateTime?) null : DateTime.UtcNow,
                ExpirationDate = backgroundCheckUrl is null ? (DateTime?) null : DateTime.UtcNow.AddYears(1),
            };
            
            if(messages.HasNoErrors())
                messages.AddRange(backgroundCheckRepository.Save(backgroundCheck));
            
            return new BackgroundCheckRequestResult {BackgroundCheckUrl = backgroundCheckUrl , msg = messages};
        }
    }

    public class BackgroundCheckRequestResult
    {
        public string BackgroundCheckUrl { get; set; }
        public SystemMessages msg { get; set; }
    }

    public class SterlingCredentials
    {
        public SterlingCredentials(string endpoint, string clientId, string clientSecret, string packageId, string accountId)
        {
            Endpoint = endpoint;
            ClientId = clientId;
            ClientSecret = clientSecret;
            PackageId = packageId;
            AccountId = accountId;
        }

        public string Endpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string PackageId { get; set; }
        public string AccountId { get; set; }
    }
}