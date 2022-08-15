using System;
using System.Threading.Tasks;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibrary.Integrations
{
    public interface IBackgroundCheckWorkflow
    {
        Task<BackgroundCheckRequestResult> RequestBackgroundCheck(Registrant registrant, SystemMessages messages);
    }

    public class BackgroundCheckWorkflow : IBackgroundCheckWorkflow
    {
        private readonly ISterlingCreateCandidateQuery createCandidateQuery;
        private readonly ISterlingCreateScreeningQuery createScreeningQuery;
        private readonly SterlingCredentials credentials;
        private readonly ISterlingBearerTokenQuery tokenQuery;

        public BackgroundCheckWorkflow(SterlingCredentials credentials, ISterlingBearerTokenQuery tokenQuery,
            ISterlingCreateCandidateQuery createCandidateQuery, ISterlingCreateScreeningQuery createScreeningQuery)
        {
            this.credentials = credentials;
            this.tokenQuery = tokenQuery;
            this.createCandidateQuery = createCandidateQuery;
            this.createScreeningQuery = createScreeningQuery;
        }

        public async Task<BackgroundCheckRequestResult> RequestBackgroundCheck(Registrant registrant, SystemMessages messages)
        {
            try
            {
                var token = await tokenQuery.GetAsync(credentials);
                var candidateId = await createCandidateQuery.CreateAsync(credentials, token.Token, registrant);
                var backgroundCheckUrl = await createScreeningQuery.CreateAsync(credentials, token.Token, candidateId);
                return new BackgroundCheckRequestResult {BackgroundCheckUrl = backgroundCheckUrl};
            }
            catch (Exception ex)
            {
                messages.Add(new SystemMessage(ex.Message, MessageType.ApiError));
                return new BackgroundCheckRequestResult {BackgroundCheckUrl = null};
            }
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