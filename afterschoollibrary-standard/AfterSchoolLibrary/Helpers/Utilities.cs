using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using AfterSchoolLibrary.Models;
using Amazon;
using BasicLibrary;
using Newtonsoft.Json;
using Rollbar;
using SendGrid.Helpers.Mail;
using File = System.IO.File;

namespace AfterSchoolLibrary.Helpers
{
    public class Utilities
    {
        public static int ImageUploadMaxBytes => 1024 * 1024 * 4;

        public static string ConnectionString => Database.Credentials.ConnectionString;

        public static string SurveyBaseUrl
        {
            get
            {
                if (IsQA)
                    return "app-dev.afterschoolhq.com";
                if (IsStaging)
                    return "app-staging.afterschoolhq.com";
                if (IsProduction)
                    return "app.afterschoolhq.com";
                return "http://localhost:54849";
            }
        }

        public static string Environment
        {
            get
            {
                if (!IsLocalHost)
                    return ConfigurationManager.AppSettings["NET_ENV"];

                return "Local";
            }
        }

        public static bool IsProduction => ConfigurationManager.AppSettings["NET_ENV"] == "Production";

        public static bool IsStaging => ConfigurationManager.AppSettings["NET_ENV"] == "Staging";

        public static bool IsTest => ConfigurationManager.AppSettings["NET_ENV"] == "Test";

        public static bool IsCI => ConfigurationManager.AppSettings["NET_ENV"] == "CI";

        public static bool IsQA => ConfigurationManager.AppSettings["NET_ENV"] == "QA";

        public static bool IsLocalHost => !IsProduction && !IsStaging && !IsCI && !IsQA;

        public static void SystemErrorLog(SystemMessages msgs)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                { Environment = GetRollbarEnvironment() });
            RollbarLocator.RollbarInstance.Error(msgs.RenderMessageList(MessageOutputType.Text));
        }

        public static void SystemErrorLog(Exception ex)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                { Environment = GetRollbarEnvironment() });
            RollbarLocator.RollbarInstance.Error(ex.Message + " - " + ex.InnerException + " - " + ex.StackTrace);
        }

        public static void SystemLog(SystemMessages msgs)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                { Environment = GetRollbarEnvironment() });
            RollbarLocator.RollbarInstance.Info(msgs.RenderMessageList(MessageOutputType.Text));
        }

        public static void SystemLog(string msg)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                { Environment = GetRollbarEnvironment() });
            RollbarLocator.RollbarInstance.Info(msg);
        }

        public static string RollbarAccessToken => ConfigurationManager.AppSettings["Rollbar.AccessToken"];

        public static string GetRollbarEnvironment()
        {
            if (IsProduction)
            {
                return "production";
            }
            if (IsStaging)
            {
                return "staging";
            }
            if (IsCI)
            {
                return "ci";
            }
            if (IsQA)
            {
                return "qa";
            }
            return "local";
        }

        public static string[] FullTextSearchWordsToIgnore =
        {
            "an", "and", "as", "at", "be", "by", "do", "for", "from", "if", "in", "is", "it", "of", "on", "or", "so", "the", "to", "was",
            "with"
        };

        public static string AppApiKey => MasterAPIKey;

        public static string AppApiSecret => MasterAPISecret;

        public static string MasterAPIKey { get; set; }
        public static string MasterAPISecret { get; set; }

        public static string AmazonAccessKeyId
        {
            get
            {
                if (IsLocalHost)
                    return "AKIAJYT3PYXGXTSEMSAQ";

                return ConfigurationManager.AppSettings["AmazonAccessKeyId"];
            }
        }

        public static string AmazonSecretAccessKeyId
        {
            get
            {
                if (IsLocalHost)
                    return "rTkFaZuJ/65sc4QcpjA0tBNEaoIYOXCoUjvlVRBD";

                return ConfigurationManager.AppSettings["AmazonSecretAccessKeyId"];
            }
        }

        public static string ReCaptchaPrivateKey { get; set; }
        public static string ReCaptchaPublicKey { get; set; }

        private static string AppDocumentRoot;

        public static byte[] AppAESKey { get; set; }

        private static byte[] _AppIV;

        public static byte[] AppIV
        {
            get
            {
                if (_AppIV.IsEmpty())
                {
                    string filepath = AppDocumentRoot + "\\app\\";
                    if (!Directory.Exists(filepath))
                    {
                        Directory.CreateDirectory(filepath);
                    }

                    filepath += "appiv";
                    if (!File.Exists(filepath))
                    {
                        File.WriteAllBytes(filepath, Encryption.GetNewIV());
                    }

                    _AppIV = File.ReadAllBytes(filepath);
                }

                return _AppIV;
            }
        }

        public static string HostName
        {
            get
            {
                if (IsLocalHost)
                    return "localhost:54849";

                return ConfigurationManager.AppSettings["HostName"];
            }
        }

        public static string ShortHostName
        {
            get
            {
                if (IsProduction)
                    return ConfigurationManager.AppSettings["ShortHostName"];

                return HostName;
            }
        }

        public static string ShortBaseUrl()
        {
            return $"http{(UseSSL() ? "s" : "")}://{ShortHostName}";
        }

        public static string BaseUrl()
        {
            return $"http{(UseSSL() ? "s" : "")}://{HostName}";
        }

        public static string LoginUrl(string redirectUrl = null)
        {
            return $"{BaseUrl()}/login{(redirectUrl.IsNotEmpty() ? $"?returnurl={HttpUtility.UrlEncode(redirectUrl)}" : "")}";
        }

        public static string CompleteSignUpUrl(Guid currentUserUid, string redirectUrl = null)
        {
            return
                $"{BaseUrl()}/signup/confirm/{currentUserUid.ToString()}{(redirectUrl.IsNotEmpty() ? $"?returnurl={HttpUtility.UrlEncode(redirectUrl)}" : "")}";
        }

        public static bool UseSSL()
        {
            if (IsLocalHost)
                return false;

            return true;
        }

        public static string AppNameEnrollHQ { get; set; }
        public static string AppNameAfterSchoolHQ { get; set; }

        public static string AppName(bool isAfterSchool = true)
        {
            if (isAfterSchool)
                return AppNameAfterSchoolHQ;

            return AppNameEnrollHQ;
        }

        public static void InitializeApp()
        {
            // Utilities.UseSSL = true; //can put in a if statement depending on if is live, etc.
            // AfterSchoolLibrary.Helpers.Utilities.AppName = "Enroll HQ"; //used in email signatures, etc
            AppAESKey
                = Encoding.UTF8.GetBytes(
                    "9dj3nshcnshf73heydsg1nshdyciefh7"); //32 alphanumeric chars - only set once per app, never changed afterwards, required

            MasterAPIKey = ""; //at least 45 random alphanumeric characters
            MasterAPISecret = ""; //at least 65 random alphanumeric characters

            AmazonRegionEndPoint = RegionEndpoint.USEast2;

            Email.ReplyHostName = "afterschoolhq.com"; // can be modified based on live/dev/test server
            Email.SenderHostName = "afterschoolhq.com"; // can be modified based on live/dev/test server

            AppNameAfterSchoolHQ = "AfterSchool HQ";
            AppNameEnrollHQ = "Enroll HQ";

            ReCaptchaPrivateKey = "6Lfk694SAAAAAH5e3gWv3JckrYzz2scslo_rop6M"; //can be used as standard re-captcha
            ReCaptchaPublicKey = "6Lfk694SAAAAAIrtt3Hwiz7j4emDRlYuLp73i9VY";
        }

        public static AmazonHelper GetNewAmazonHelper()
        {
            return new AmazonHelper(
                AmazonBucketName,
                AmazonAccessKeyId,
                AmazonSecretAccessKeyId,
                AmazonRegionEndPoint);
        }

        public static RegionEndpoint AmazonRegionEndPoint { get; set; }

        public static string AmazonBucketName
        {
            get
            {
                if (IsLocalHost)
                    return "afterschoolhq-local";

                return ConfigurationManager.AppSettings["AmazonBucketName"];
            }
        }

        public static bool IsAlreadyJsonString(string s)
        {
            try
            {
                if (s.IsEmpty())
                    return false;

                var stringInput = s.Trim();
                if ((stringInput.StartsWith("{") && stringInput.EndsWith("}")))
                    return true; //For json object
                if (stringInput.StartsWith("[") && stringInput.EndsWith("]"))
                    return true; //For json array

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static List<KeyValuePair> GetKeyValuePairs(string s)
        {
            if (s.IsEmpty())
                return new List<KeyValuePair>();

            return JsonConvert.DeserializeObject<List<KeyValuePair>>(s);
        }
    }

    public class Email
    {
        public static string BaseUrl()
        {
            return $"{Utilities.BaseUrl()}/";
        }

        public static string HostName()
        {
            return Utilities.HostName;
        }

        public static MailAddress AppFromAddress(bool isAfterSchool = true)
        {
            return new MailAddress(AppNoReplyEmailAddress, AppEmailFromName(isAfterSchool));
        }

        public static EmailAddress SendGridNoReplyEmailAddress(bool isAfterSchool = true)
        {
            return new EmailAddress(AppNoReplyEmailAddress, Utilities.AppName(isAfterSchool));
        }

        public static EmailAddress SendGridSupportEmailAddress => new EmailAddress(AppSupportEmailAddress);

        public static string AppSupportEmailAddress => $"{(Utilities.IsProduction ? "support" : "dev")}@{ReplyHostName}";

        public static string AppSalesEmailAddress => $"{(Utilities.IsProduction ? "sales" : "dev")}@{ReplyHostName}";


        public static string AppNoReplyEmailAddress => $"noreply@{SenderHostName}";

        public static string AppCustomersEmailAddress => $"{(Utilities.IsProduction ? "customers" : "dev")}@{ReplyHostName}";

        public static string AppEmailFromName(bool isAfterSchool)
        {
            return Utilities.AppName(isAfterSchool);
        }

        public static string AppSignature(bool isAfterSchool)
        {
            return string.Format("<p>{0}<br /><a href=\"{1}\">{1}</a></p>", AppEmailFromName(isAfterSchool), BaseUrl());
        }

        public static string GetAppSignature(bool isAfterSchool, string utm_campaign, string utm_source = "afterschoolhq",
            string utm_medium = "email")
        {
            return
                $"<p>Powered by <a href=\"{BaseUrl()}?utm_source={utm_source}&utm_medium={utm_medium}&utm_campaign={utm_campaign}\">{AppEmailFromName(isAfterSchool)}</a></p>";
        }

        public static string AppReplyAboveLineText => "--- Reply ABOVE THIS LINE to add a comment ---";

        public static string AppReplyAboveLineTextFormatted => "<p style=\"font-size:9px;\">" + AppReplyAboveLineText + "</p><p></p>";

        public static string ReplyHostName { get; set; }

        public static string SenderHostName { get; set; }
    }

    public class KeyValuePair
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}