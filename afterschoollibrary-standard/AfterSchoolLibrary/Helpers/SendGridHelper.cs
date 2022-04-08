using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using SendGrid;
using SendGrid.Helpers.Mail;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary
{
    public class SendGridHelper
    {
        public const string EnrollHQGenericTemplateId = "c672e668-1b54-4721-84be-50fe3cde7cb2";
        public const string AfterSchoolHQGenericTemplateId = "70362a81-6361-4140-9d5b-25b28e97ceca";
        public const string MarketingUrlPath = "marketing/contacts";
        public static string ProductionSendGridApiKey => ConfigurationManager.AppSettings["SendGrid.ProductionApiKey"];

        public static string DevelopmentSendGridApiKey =>
            ConfigurationManager.AppSettings["SendGrid.DevelopmentApiKey"];

        public static string SendGridApiKey
        {
            get
            {
                if (Utilities.IsProduction)
                    return ProductionSendGridApiKey;

                return DevelopmentSendGridApiKey;
            }
        }

        public static string GetGenericTemplateId(bool isAfterSchool = true)
        {
            if (isAfterSchool)
                return AfterSchoolHQGenericTemplateId;

            return EnrollHQGenericTemplateId;
        }

        public static SystemMessages SendMail(string fromEmail, string toEmail, string subject, string body,
            string replyTo = null, string emailTemplateId = null)
        {
            EmailAddress replyToEmail = null;
            if (replyTo.IsNotBlank())
                replyToEmail = new EmailAddress(replyTo);

            return SendMail(new EmailAddress(fromEmail), new EmailAddress(toEmail), subject, body, replyToEmail,
                emailTemplateId);
        }

        public static SystemMessages SendMail(EmailAddress fromEmail, EmailAddress toEmail, string subject, string body,
            EmailAddress replyTo = null, string emailTemplateId = null, EmailAddress ccEmail = null)
        {
            var msgs = new SystemMessages();

            var sg = new SendGridClient(SendGridApiKey);
            // var content = new Content("text/html", body);

            if (!Utilities.IsProduction)
                subject = $"{Utilities.Environment} ENV: {subject}";

            var mail = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, null, body);
            mail.ReplyTo = replyTo;
            mail.TemplateId = emailTemplateId ?? EnrollHQGenericTemplateId;
            if (ccEmail != null)
                mail.AddCc(ccEmail);

            var task = Task.Run(async () => await sg.SendEmailAsync(mail));
            task.Wait();

            dynamic response = task.Result;
            if (response.StatusCode != HttpStatusCode.Accepted)
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);

            return msgs;
        }

        public static void AddContactFromDemo(string orgname, string firstName, string email, SystemMessages msgs)
        {
            var sg = new SendGridClient(SendGridApiKey);

            var contact = new List<object>
            {
                new
                {
                    first_name = firstName,
                    email,
                    custom_fields = new List<object>
                    {
                        new
                        {
                            org_name = orgname,
                            web_demo = 1,
                        },
                    },
                },
            };

            var task = Task.Run(async () =>
            {
                return await sg.RequestAsync(
                    BaseClient.Method.PUT,
                    urlPath: MarketingUrlPath,
                    requestBody: contact.ToJSON());
            });
            task.Wait();

            dynamic response = task.Result;
            if (response.StatusCode != HttpStatusCode.Accepted)
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
        }

        public static void AddOrUpdateContact(Account account, WebUser user, SystemMessages messages)
        {
            if (account.IsNotEmpty() && user.IsNotEmpty())
            {
                var lastestActivity = Activity
                    .LoadActivity(account.ID.Value, "CreateDate", OrderByDirectionEnum.Descending, msgs: messages)
                    ?.FirstOrDefault();
                var accountProfile = account.GetAccountProfile();

                AddOrUpdateContact(
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.ID.IsNotEmpty() ? user.ID.Value : 0,
                    account.ID.IsNotEmpty() ? account.ID.Value : 0,
                    account.Email,
                    account.Name,
                    account.Phone ?? "",
                    account.HomePageUrl,
                    account.CreateDate.Value.ToRfc3339String(),
                    (DateTime.UtcNow - account.CreateDate).IsNotEmpty()
                        ? (DateTime.UtcNow - account.CreateDate).Value.Days
                        : 0,
                    lastestActivity?.CreateDate.Value.ToRfc3339String(),
                    (DateTime.UtcNow - lastestActivity?.CreateDate).IsNotEmpty()
                        ? (DateTime.UtcNow - lastestActivity?.CreateDate).Value.Days
                        : 0,
                    account.GetActivityCount(),
                    accountProfile.IsNotEmpty() ? 1 : 0,
                    account.OnboardingStepDone != OnboardingStepType.NotSet ? 1 : 0,
                    account.OnboardingStepDone == OnboardingStepType.All ? 1 : 0,
                    account.StripeChargesEnabled ? 1 : 0,
                    user.VerifiedAccount ? 1 : 0,
                    Utilities.IsProduction ? "0" : "1",
                    messages);
            }
        }

        public static void AddOrUpdateContact(string firstName, string lastName, string email, int userId,
            int accountId, string accountEmail, string accountName, string accountPhone,
            string accountHomePageUrl, string accountCreateDate, int accountCreationDays, string lastActivityDate,
            int lastActivityCreationDays, int activityCount, int isAccountProfileExist,
            int isAccountProfileStarted, int isAccountProfileCompleted, int stripeConnected, int emailVerified,
            string isTest, SystemMessages msgs)
        {
            var sg = new SendGridClient(SendGridApiKey);

            var contact = new
            {
                contacts = new List<object>
                {
                    new
                    {
                        first_name = firstName,
                        last_name = lastName,
                        email,
                        custom_fields = new
                        {
                            e12_N = accountId, //account_id
                            e13_T = accountName, //program_name
                            e14_D = accountCreateDate, // program_create_date
                            e15_T = accountPhone, //program_phone
                            e16_T = accountEmail, //program_email
                            e17_N = userId, // owner_id
                            e19_N = isAccountProfileExist, // profile_exists
                            e20_N = isAccountProfileStarted, // profile_started
                            e21_N = isAccountProfileCompleted, // profile_completed
                            e22_N = activityCount, // activities_created
                            e23_D = lastActivityDate ?? "", // last_activity_create_date
                            e24_N = lastActivityCreationDays.IsNotEmpty()
                                ? lastActivityCreationDays
                                : 0, // days_since_last_activity_created
                            e25_N = accountCreationDays, // days_since_account_created
                            e28_T = isTest, // is_test
                            e29_T =
                                accountHomePageUrl, // activity_home_page_url  (program_profile is not available, instead we have activity_home_page_url)
                            e30_T = accountHomePageUrl.IsBlank()
                                ? ""
                                : $"{Utilities.BaseUrl()}/login?returnurl={HttpUtility.UrlEncode($"{accountHomePageUrl}?edit=1")}", //program_profile_edit_link
                            e31_N = stripeConnected, // stripe_connected
                            e32_N = emailVerified, // email_verified
                        },
                    },
                },
            };

            var task = Task.Run(async () =>
            {
                return await sg.RequestAsync(
                    BaseClient.Method.PUT,
                    urlPath: MarketingUrlPath,
                    requestBody: contact.ToJSON()
                );
            });

            task.Wait();
            var response = task.Result;
            if (!response.IsNotEmpty() || response.StatusCode != HttpStatusCode.Accepted)
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
        }
    }
}