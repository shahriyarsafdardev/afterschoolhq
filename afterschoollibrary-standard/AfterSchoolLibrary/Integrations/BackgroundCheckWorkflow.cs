using System.Threading.Tasks;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Integrations
{
    public interface IBackgroundCheckWorkflow
    {
        Task<BackgroundCheckRequestResult> RequestBackgroundCheck(Registrant registrant);
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

        public async Task<BackgroundCheckRequestResult> RequestBackgroundCheck(Registrant registrant)
        {
            var token = await tokenQuery.GetAsync(credentials);
            var candidateId = await createCandidateQuery.CreateAsync(credentials, token.Token, registrant);
            var inviteLink = await createScreeningQuery.CreateAsync(credentials, token.Token, candidateId);

            return new BackgroundCheckRequestResult
            {
                IsSuccessful = true,
                InviteLink = inviteLink,
            };
        }
    }

    public class BackgroundCheckRequestResult
    {
        public bool IsSuccessful { get; set; }
        public string InviteLink { get; set; }
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