using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AfterSchoolLibrary
{
    public class SendGridHelper
    {
        public const string SendGridApiKey = "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";
        public const string EnrollHQGenericTemplateId = "c672e668-1b54-4721-84be-50fe3cde7cb2";
        public const string AfterSchoolHQGenericTemplateId = "70362a81-6361-4140-9d5b-25b28e97ceca";

        public static string GetGenericTemplateId(bool isAfterSchool = true)
        {
            if (isAfterSchool)
                return AfterSchoolHQGenericTemplateId;

            return EnrollHQGenericTemplateId;
        }

        public static SystemMessages SendMail(string fromEmail, string toEmail, string subject, string body, string replyTo = null, string emailTemplateId = null)
        {
            EmailAddress replyToEmail = null;
            if (replyTo.IsNotBlank())
                replyToEmail = new EmailAddress(replyTo);

            return SendMail(new EmailAddress(fromEmail), new EmailAddress(toEmail), subject, body, replyToEmail, emailTemplateId);
        }

        public static SystemMessages SendMail(EmailAddress fromEmail, EmailAddress toEmail, string subject, string body, EmailAddress replyTo = null, string emailTemplateId = null)
        {
            var msgs = new SystemMessages();

            var sg = new SendGridClient(SendGridApiKey);
            // var content = new Content("text/html", body);

            if (!Helpers.Utilities.IsProduction)
                subject = $"{Helpers.Utilities.Environment} ENV: {subject}";

            var mail = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, null, body);
            mail.ReplyTo = replyTo;
            mail.TemplateId = emailTemplateId ?? EnrollHQGenericTemplateId;

            var task = Task.Run(async () => { return await sg.SendEmailAsync(mail); });
            task.Wait();

            dynamic response = task.Result;
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);

            return msgs;
        }

        public static void AddContactFromDemo(string orgname, string firstName, string email, SystemMessages msgs)
        {
            var sg = new SendGridClient(SendGridApiKey);

            var contact = new List<object>()
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
                            web_demo = 1
                        }
                    }
                  
                }
            };

            var task = Task.Run(async () =>
            {
                return await sg.RequestAsync(
                    method: SendGridClient.Method.PUT,
                    urlPath: "marketing/contactss",
                    requestBody: contact.ToJSON());
            });
            task.Wait();

            dynamic response = task.Result;
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
            }
        }

        public static void AddOrUpdateContact(Account account, WebUser user, SystemMessages messages)
        {
            var lastestActivity = Activity.LoadActivity(AccountId: account.ID.Value, OrderByProperty: "CreateDate", orderByDirection: OrderByDirectionEnum.Descending, msgs: messages)?.FirstOrDefault();
            var accountProfile = account.GetAccountProfile();

            AddOrUpdateContact(
                firstName: user.FirstName,
                lastName: user.LastName,
                email: user.Email,
                userId: user.ID.IsNotEmpty() ? user.ID.Value.ToString() : "",
                accountId: account.ID.IsNotEmpty() ? account.ID.Value.ToString() : "",
                accountEmail: account.Email,
                accountName: account.Name,
                accountPhone: account.Phone.IsNotEmpty() ? account.Phone : "",
                accountHomePageUrl: account.HomePageUrl,
                accountCreateDate: account.CreateDate.Value.ToRfc3339String(),
                accountCreationDays: (DateTime.UtcNow - account.CreateDate)?.Days.ToString(),
                lastActivityDate: lastestActivity?.CreateDate.Value.ToRfc3339String(),
                lastActivityCreationDays: (DateTime.UtcNow - lastestActivity?.CreateDate)?.Days.ToString(),
                activityCount: account.GetActivityCount().ToString(),
                isAccountProfileExist: accountProfile.IsNotEmpty() ? "1" : "0",
                isAccountProfileStarted: account.OnboardingStepDone != OnboardingStepType.NotSet ? "1" : "0",
                isAccountProfileCompleted: account.OnboardingStepDone == OnboardingStepType.All ? "1" : "0",
                stripeConnected: account.StripeChargesEnabled ? "1" : "0",
                emailVerified: user.VerifiedAccount ? "1" : "0",
                isTest: Helpers.Utilities.IsProduction ? "0" : "1",
                msgs: messages);
        }

        public static void AddOrUpdateContact(string firstName, string lastName, string email, string userId, string accountId, string accountEmail, string accountName, string accountPhone,
            string accountHomePageUrl, string accountCreateDate, string accountCreationDays, string lastActivityDate, string lastActivityCreationDays, string activityCount, string isAccountProfileExist,
            string isAccountProfileStarted, string isAccountProfileCompleted, string stripeConnected, string emailVerified, string isTest, SystemMessages msgs)
        {
            var sg = new SendGridClient(SendGridApiKey);

            var contact =  new
               {
                    contacts = new List<object>
                    {
                        new
                        {
                            first_name = firstName,
                            last_name = lastName,
                            email = email,
                            custom_fields = new
                            {
                                e17_N = userId, // owner_id
                                e12_N = accountId,  //account_id
                                e16_T = accountEmail, //program_email
                                e13_T = accountName,  //program_name
                                e15_T = accountPhone, //program_phone
                                e29_T = accountHomePageUrl, // activity_home_page_url  (program_profile is not available, instead we have activity_home_page_url)
                                e30_T = accountHomePageUrl.IsBlank() ? "" : $"{Helpers.Utilities.BaseUrl()}/login?returnurl={HttpUtility.UrlEncode($"{accountHomePageUrl}?edit=1")}", //program_profile_edit_link
                                e14_D = accountCreateDate,  // program_create_date
                                e25_N = accountCreationDays,  // days_since_account_created
                                e23_D = lastActivityDate.IsNotEmpty() ? lastActivityDate : "",  // last_activity_create_date
                                e24_N = lastActivityCreationDays.IsNotEmpty() ? lastActivityCreationDays : "",  // days_since_last_activity_created
                                e22_N = activityCount,   // activities_created
                                e19_N = isAccountProfileExist,  // profile_exists
                                e20_N = isAccountProfileStarted,  // profile_started
                                e21_N = isAccountProfileCompleted, // profile_completed
                                e31_N = stripeConnected, // stripe_connected
                                e32_N = emailVerified,  // email_verified
                                e28_T = isTest // is_test
                                
                            }
                        }
                    }

            };

            var task = Task.Run(async () =>
            {
                return await sg.RequestAsync(
                    method: SendGridClient.Method.PUT,
                    urlPath: "marketing/contacts",
                    requestBody: contact.ToJSON()
                );
            });

            task.Wait();
            var response = task.Result;
            if (!response.IsNotEmpty() || response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
            }
        }
    }
}
