using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibrary
{
    public class StripeHelper
    {
        public static string TestSecretKey { get { return "sk_test_gfeMjl5rgl16L1HHITh3YT5s"; } }
        public static string TestPublishableKey { get { return "pk_test_wuL50O8i1mAzrmV2xDAXuClJ"; } }
        public static string LiveSecretKey { get { return "sk_live_SDa559erkpDCJ9BkrPS58GrC"; } }
        public static string LivePublishableKey { get { return "pk_live_4q5JuU2JIxsjWfQauJ7bNISK"; } }

        public static string DevPlatformClientId { get { return "ca_7kmNt22jLhXZWtyVV1884Kng6mgs79Jv"; } }
        public static string LivePlatformClientId { get { return "ca_7kmNqL4oMYfH5z7tY2N9riALjFpkMJAQ"; } }

        public static string SecretKey
        {
            get
            {
                if(Utilities.IsLiveServer)
                {
                    return LiveSecretKey;
                }
                else
                {
                    return TestSecretKey;
                }
            }
        }
        public static string PublishableKey
        {
            get
            {
                if (Utilities.IsLiveServer)
                {
                    return LivePublishableKey;
                }
                else
                {
                    return TestPublishableKey;
                }
            }
        }
        public static string PlatformClientId
        {
            get
            {
                if (Utilities.IsLiveServer)
                {
                    return LivePlatformClientId;
                }
                else
                {
                    return DevPlatformClientId;
                }
            }
        }

    }
}
