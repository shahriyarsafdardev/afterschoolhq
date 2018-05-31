using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchool
{
    public class AppConfig
    {
        public static void Init()
        {
            RollbarSharp.Configuration.DefaultEnvironment = AfterSchoolLibrary.Utilities.GetRollbarEnvironment();

            AfterSchoolLibrary.Utilities.UseSSL = true; //can put in a if statement depending on if is live, etc.
            // AfterSchoolLibrary.Utilities.AppName = "Enroll HQ"; //used in email signatures, etc
            AfterSchoolLibrary.Utilities.AppAESKey = System.Text.Encoding.UTF8.GetBytes("9dj3nshcnshf73heydsg1nshdyciefh7"); //32 alphanumeric chars - only set once per app, never changed afterwards, required

            AfterSchoolLibrary.Utilities.MasterAPIKey = ""; //at least 45 random alphanumeric characters
            AfterSchoolLibrary.Utilities.MasterAPISecret = ""; //at least 65 random alphanumeric characters

            AfterSchoolLibrary.Utilities.AmazonBucketNameDev = "enrollhq.dev"; //all files are stored in amazon bucket
            AfterSchoolLibrary.Utilities.AmazonBucketNameTest = "enrollhq.test";
            AfterSchoolLibrary.Utilities.AmazonBucketNameLive = "enrollhq.live";
            AfterSchoolLibrary.Utilities.AmazonBucketNameLocal = "enrollhq.local";

            AfterSchoolLibrary.Utilities.AmazonAccessKeyId = "AKIAIRWKZE6AEP57RE2Q";
            AfterSchoolLibrary.Utilities.AmazonSecretAccessKeyId = "j8rbG/g32C2f15U0xLy4os5RNkypQgFxxVqCcS9N";

            AfterSchoolLibrary.Email.AppAdminEmailAddress = "support@enrollhq.com"; //all app admin emails are sent to this email address
            AfterSchoolLibrary.Email.ReplyHostName = "enrollhq.com"; // can be modified based on live/dev/test server
            AfterSchoolLibrary.Email.SenderHostName = "mailer.enrollhq.com"; // can be modified based on live/dev/test server

            AfterSchoolLibrary.Email.HostNameLiveEnrollHQ = "enrollhq.com"; //since the hostname may not be able to be picked up from the actual url that is processing the request
            AfterSchoolLibrary.Email.HostNameTestEnrollHQ = "staging.enrollhq.com";
            AfterSchoolLibrary.Email.HostNameDevEnrollHQ = "dev.enrollhq.com";
            AfterSchoolLibrary.Email.HostNameLocalEnrollHQ = "localhost:54849";

            AfterSchoolLibrary.Email.HostNameLiveAfterSchoolHQ = "afterschoolhq.com"; //since the hostname may not be able to be picked up from the actual url that is processing the request
            AfterSchoolLibrary.Email.HostNameTestAfterSchoolHQ = "staging.afterschoolhq.com";
            AfterSchoolLibrary.Email.HostNameDevAfterSchoolHQ = "dev.afterschoolhq.com";
            AfterSchoolLibrary.Email.HostNameLocalAfterSchoolHQ = "localhost:54849";

            AfterSchoolLibrary.Utilities.ReCaptchaPrivateKey = "6Lfk694SAAAAAH5e3gWv3JckrYzz2scslo_rop6M"; //can be used as standard re-captcha
            AfterSchoolLibrary.Utilities.ReCaptchaPublicKey = "6Lfk694SAAAAAIrtt3Hwiz7j4emDRlYuLp73i9VY";
        }
    }
}
