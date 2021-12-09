using System.Collections.Generic;
using System.Configuration;

namespace AfterSchoolLibrary.Helpers
{
    public class CleverHelper
    {
        private static string SecretKey { get { return ConfigurationManager.AppSettings["Clever.ClientSecret"]; } }
        public static string ClientId { get { return ConfigurationManager.AppSettings["Clever.ClientId"]; } }
        public static string RedirectUrl { get { return Utilities.BaseUrl() + "/oauth/clever"; } }
        public static string TokenRetrieveUrl { get { return "https://clever.com/oauth/tokens"; } }
        public static string UserDetailUrl { get { return "https://api.clever.com/me"; } }

        /// <summary>
        /// Provides authorization token for clever oAuth token
        /// </summary>
        public static string BasicAuthorizationToken
        { 
            get 
            {
                return $"{ClientId}:{SecretKey}".EncodeBase64();
            }
        }

        public static string SSOLink
        {
            get
            {
                return $"https://clever.com/oauth/authorize?response_type=code&redirect_uri={RedirectUrl}&client_id={ClientId}";
            }
        }
    }
}

namespace Clever
{
    namespace Models
    {
        public class CleverUserDto
        {
            public string type { get; set; }
            public CleverUserData data { get; set; }
        }

        public class CleverUserData
        {
            public string id { get; set; }
            public string type { get; set; }
            public string email { get; set; }
            public CleverUser name { get; set; }
            public List<string> sections { get; set; }
        }

        public class CleverUser
        {
            public string first { get; set; }
            public string last { get; set; }
        }

        public class CleverTokenDto
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
        }
    }
}