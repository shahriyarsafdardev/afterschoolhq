using System;
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
        public static string UserDetailUrl { get { return "https://api.clever.com/v3.0/me"; } }
        public static string UsersDetailUrl { get { return "https://api.clever.com/v3.0/users/"; } }
        public static string PortalUrl { get { return "https://clever.com/in/placid-chalk-6926"; } }

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



        public class CleverUserDistrictDto
        {
            public Data data { get; set; }
            public Link[] links { get; set; }
        }

        public class Data
        {
            public District_Contact district_contact { get; set; }
            public string error { get; set; }
            public DateTime last_sync { get; set; }
            public DateTime launch_date { get; set; }
            public string[] login_methods { get; set; }
            public string name { get; set; }
            public string portal_url { get; set; }
            public string sis_type { get; set; }
            public string state { get; set; }
            public string id { get; set; }
        }

        public class District_Contact
        {
            public string district { get; set; }
            public string email { get; set; }
            public Name name { get; set; }
            public string title { get; set; }
        }

        public class Name
        {
            public string first { get; set; }
            public string last { get; set; }
        }

        public class Link
        {
            public string rel { get; set; }
            public string uri { get; set; }
        }
    }
}