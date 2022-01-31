using System;
using System.Collections.Generic;
using System.Configuration;

namespace AfterSchoolLibrary.Helpers
{
    public class CleverHelper
    {
        private static string SecretKey => ConfigurationManager.AppSettings["Clever.ClientSecret"];
        public static string ClientId => ConfigurationManager.AppSettings["Clever.ClientId"];
        public static string RedirectUrlForIntegration => Utilities.BaseUrl() + "/oauth/cleverintegration";
        public static string RedirectUrlForLogin => Utilities.BaseUrl() + "/oauth/cleverlogin";
        public static string RedirectUrlForAccount => Utilities.BaseUrl() + "/oauth/cleverloginfromaccounttopbar";
        public static string TokenRetrieveUrl => "https://clever.com/oauth/tokens";
        /// <summary>
        /// This will return just a Id of user with some other details like user type
        /// </summary>
        public static string UserIdentityUrl => "https://api.clever.com/v3.0/me";
        /// <summary>
        /// This will return details of user like name, email etc but it will require User Id
        /// </summary>
        public static string UserDetailUrl => "https://api.clever.com/v3.0/users/{UserId}";
        public static string PortalUrl => "https://clever.com/in/placid-chalk-6926";

        /// <summary>
        /// Provides authorization token for clever oAuth token
        /// </summary>
        public static string BasicAuthorizationToken => $"{ClientId}:{SecretKey}".EncodeBase64();

        public static string SSOLink => $"https://clever.com/oauth/authorize?response_type=code&redirect_uri={RedirectUrlForIntegration}&client_id={ClientId}";

        /// <summary>
        /// This will be used at login page to redirect user based on account/user types after clever login completes
        /// </summary>
        public static string SSOLinkLogin => $"https://clever.com/oauth/authorize?response_type=code&redirect_uri={RedirectUrlForLogin}&client_id={ClientId}";

        /// <summary>
        /// This will be used at account topbar to redirect user back to same account page after clever login completes
        /// </summary>
        public static string SSOLinkAccount => $"https://clever.com/oauth/authorize?response_type=code&redirect_uri={RedirectUrlForAccount}&client_id={ClientId}";

        public static string GetUsersDetailUrl(string Id) => UserDetailUrl.Replace("{UserId}", Id);
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

        public class CleverUserDetailsDto
        {
            public Data data { get; set; }
            public Link[] links { get; set; }
        }

        public class Link
        {
            public string rel { get; set; }
            public string uri { get; set; }
        }

        public class Data
        {
            public string id { get; set; }
            public string email { get; set; }
            public DateTime created { get; set; }
            public string district { get; set; }            
            public DateTime last_modified { get; set; }
            public Name name { get; set; }
            public Roles roles { get; set; }            
        }

        public class Name
        {
            public string first { get; set; }
            public string last { get; set; }
            public string middle { get; set; }
        }

        public class Roles
        {
            public Teacher teacher { get; set; }

            public DistrictAdmin district_admin { get; set; }

            public Student student { get; set; }
        }

        public class Teacher
        {
            public List<object> schools { get; set; }
            public string legacy_id { get; set; }
        }

        public class DistrictAdmin
        {
            public string legacy_id { get; set; }
        }

        public class Student
        {
            public object enrollments { get; set; }
            public List<object> schools { get; set; }
        }
    }
}