using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Web;
using AfterSchoolLibrary.Models;
using Amazon;
using BasicLibrary;
using Newtonsoft.Json;
using Rollbar;
using SendGrid.Helpers.Mail;

namespace AfterSchoolLibrary.Helpers
{
    public class Utilities
    {
        private const string Production = "Production";
        private const string Staging = "Staging";
        private const string Qa = "QA";
        private const string Local = "Local";

        public static string[] FullTextSearchWordsToIgnore =
        {
            "an", "and", "as", "at", "be", "by", "do", "for", "from", "if", "in", "is", "it", "of", "on", "or", "so",
            "the", "to", "was",
            "with",
        };

        public static int ImageUploadMaxBytes => 1024 * 1024 * 4;

        public static string SurveyBaseUrl => Environment switch
        {
            Production => "app.afterschoolhq.com",
            Staging => "app-staging.afterschoolhq.com",
            Qa => "app-staging.afterschoolhq.com",
            _ => "http://localhost:54849",
        };

        public static string Environment
        {
            get
            {
                var environment = ConfigurationManager.AppSettings["NET_ENV"];
                switch (environment)
                {
                    case Production:
                    case Staging:
                    case Qa:
                        return environment;
                    default:
                        return Local;
                }
            }
        }

        public static bool IsProduction => Environment == Production;

        public static bool IsStaging => Environment == Staging;

        public static bool IsLocalHost => Environment == Local;

        public static string RollbarAccessToken => ConfigurationManager.AppSettings["Rollbar.AccessToken"];

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

        public static string AppNameEnrollHQ { get; set; }
        public static string AppNameAfterSchoolHQ { get; set; }

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

        public static void SystemErrorLog(SystemMessages msgs)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                {Environment = GetRollbarEnvironment()});
            RollbarLocator.RollbarInstance.Error(msgs.RenderMessageList(MessageOutputType.Text));
        }

        public static void SystemErrorLog(Exception ex)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                {Environment = GetRollbarEnvironment()});
            RollbarLocator.RollbarInstance.Error(ex.Message + " - " + ex.InnerException + " - " + ex.StackTrace);
        }

        public static void SystemLog(SystemMessages msgs)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                {Environment = GetRollbarEnvironment()});
            RollbarLocator.RollbarInstance.Info(msgs.RenderMessageList(MessageOutputType.Text));
        }

        public static void SystemLog(string msg)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(RollbarAccessToken)
                {Environment = GetRollbarEnvironment()});
            RollbarLocator.RollbarInstance.Info(msg);
        }

        public static string GetRollbarEnvironment()
        {
            return Environment.ToLower();
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
            return
                $"{BaseUrl()}/login{(redirectUrl.IsNotEmpty() ? $"?returnurl={HttpUtility.UrlEncode(redirectUrl)}" : "")}";
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

        public static string AppName(bool isAfterSchool = true)
        {
            if (isAfterSchool)
                return AppNameAfterSchoolHQ;

            return AppNameEnrollHQ;
        }

        public static void InitializeApp()
        {
            AmazonRegionEndPoint = RegionEndpoint.USEast2;

            Email.ReplyHostName = "afterschoolhq.com";
            Email.SenderHostName = "afterschoolhq.com";

            AppNameAfterSchoolHQ = "AfterSchool HQ";
            AppNameEnrollHQ = "Enroll HQ";
        }

        public static AmazonHelper GetNewAmazonHelper()
        {
            return new AmazonHelper(
                AmazonBucketName,
                AmazonAccessKeyId,
                AmazonSecretAccessKeyId,
                AmazonRegionEndPoint);
        }

        public static bool IsAlreadyJsonString(string s)
        {
            try
            {
                if (s.IsEmpty())
                    return false;

                var stringInput = s.Trim();
                if (stringInput.StartsWith("{") && stringInput.EndsWith("}"))
                    return true; //For json object
                if (stringInput.StartsWith("[") && stringInput.EndsWith("]"))
                    return true; //For json array

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<KeyValuePair> GetKeyValuePairs(string s)
        {
            var data = new List<KeyValuePair>();

            if (s.IsEmpty())
                return data;

            var stringInput = s.Trim();
            if (stringInput.StartsWith("{") && stringInput.EndsWith("}"))
                stringInput = "[" + stringInput + "]"; //For json object
            if (stringInput.StartsWith("[") && stringInput.EndsWith("]"))
                data.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair>>(stringInput)); //For json array

            return data;
        }

        public static bool IsSmartlookDefined(Account currentAccount, WebUser originalLoggedInUser)
        {
            return IsProduction && originalLoggedInUser.IsEmpty() && (currentAccount.IsEmpty() || currentAccount.AccountTypeId != AccountType.AfterSchool);
        }

        public static bool IsSmartlookIdentifyEnabled(Account currentAccount, WebUser currentUser, WebUser originalLoggedInUser)
        {
            return IsProduction && currentAccount.IsNotEmpty() && currentAccount.AccountTypeId != AccountType.AfterSchool && currentUser.IsNotEmpty() && originalLoggedInUser.IsEmpty();
        }
    }

    public class Email
    {
        public static EmailAddress SendGridSupportEmailAddress => new EmailAddress(AppSupportEmailAddress);

        public static string AppSupportEmailAddress =>
            $"{(Utilities.IsProduction ? "support" : "dev")}@{ReplyHostName}";

        public static string AppSalesEmailAddress => $"{(Utilities.IsProduction ? "sales" : "dev")}@{ReplyHostName}";


        public static string AppNoReplyEmailAddress => $"noreply@{SenderHostName}";

        public static string AppCustomersEmailAddress =>
            $"{(Utilities.IsProduction ? "customers" : "dev")}@{ReplyHostName}";

        public static string AppReplyAboveLineText => "--- Reply ABOVE THIS LINE to add a comment ---";

        public static string AppReplyAboveLineTextFormatted =>
            "<p style=\"font-size:9px;\">" + AppReplyAboveLineText + "</p><p></p>";

        public static string ReplyHostName { get; set; }

        public static string SenderHostName { get; set; }

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

        public static string AppEmailFromName(bool isAfterSchool)
        {
            return Utilities.AppName(isAfterSchool);
        }

        public static string AppSignature(bool isAfterSchool)
        {
            return string.Format("<p>{0}<br /><a href=\"{1}\">{1}</a></p>", AppEmailFromName(isAfterSchool), BaseUrl());
        }

        public static string GetAppSignature(bool isAfterSchool, string utm_campaign,
            string utm_source = "afterschoolhq",
            string utm_medium = "email")
        {
            return
                $"<p>Powered by <a href=\"{BaseUrl()}?utm_source={utm_source}&utm_medium={utm_medium}&utm_campaign={utm_campaign}\">{AppEmailFromName(isAfterSchool)}</a></p>";
        }
        
        public static EmailAddress SendGridSalesEmailAddress => new EmailAddress(AppSalesEmailAddress);
    }

    public class KeyValuePair
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class FilesCollection
    {
        public int ContentLength { get; set; }
        public string FileName { get; set; }
        public Stream InputStream { get; set; }
    }
}