using AfterSchoolLibrary.Helpers;
using BasicLibrary;
using Clever.Models;
using Newtonsoft.Json;
using RestSharp;

namespace AfterSchoolLibrary.Services
{
    public static class CleverIntegrationService
    {
        public static CleverTokenDto GetToken(string code, string redirectUrl)
        {
            var client = new RestSharp.RestClient(CleverHelper.TokenRetrieveUrl);
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Basic " + CleverHelper.BasicAuthorizationToken);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", redirectUrl);
            request.AddParameter("grant_type", "authorization_code");

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<CleverTokenDto>(response.Content);
        }

        public static CleverUserDto GetCleverUserIdentity(string token)
        {
            var client = new RestSharp.RestClient(CleverHelper.UserIdentityUrl);
            client.Timeout = -1;

            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + token);

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<CleverUserDto>(response.Content);
        }

        public static CleverUserDistrictDto GetCleverUserDetails(string id, string token)
        {
            var client = new RestSharp.RestClient(CleverHelper.GetUsersDetailUrl(Id: id));
            client.Timeout = -1;

            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + token);

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<CleverUserDistrictDto>(response.Content);
        }
    }
}
