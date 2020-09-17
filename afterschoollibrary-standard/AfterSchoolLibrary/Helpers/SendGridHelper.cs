using BasicLibrary;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    org_name = orgname,
                    email,
                    web_demo = 1
                }
            };

            var task = Task.Run(async () =>
            {
                return await sg.RequestAsync(
                    method: SendGridClient.Method.POST,
                    urlPath: "contactdb/recipients",
                    requestBody: contact.ToJSON());
            });
            task.Wait();

            dynamic response = task.Result;
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
            }
        }

        public static void AddOrUpdateContact(string firstName, string lastName, string email, string userId, string accountId, string accountEmail, string accountName, string accountPhone,
            string accountHomePageUrl, string accountCreateDate, string accountCreationDays, string lastActivityDate, string lastActivityCreationDays, string activityCount, string isAccountProfileExist,
            string isAccountProfileStarted, string isAccountProfileCompleted, string stripeConnected, string emailVerified, SystemMessages msgs)
        {
            var sg = new SendGridClient(SendGridApiKey);

            var contact = new List<object>()
            {
                new
                {
                    first_name = firstName.ReturnNullIfEmpty(),
                    last_name = lastName.ReturnNullIfEmpty(),
                    email = email.ReturnNullIfEmpty(),
                    owner_id = userId.ReturnNullIfEmpty(),
                    account_id = accountId.ReturnNullIfEmpty(),
                    program_email = accountEmail.ReturnNullIfEmpty(),
                    program_name = accountName.ReturnNullIfEmpty(),
                    program_phone = accountPhone.ReturnNullIfEmpty(),
                    activity_home_page_url = accountHomePageUrl.ReturnNullIfEmpty(), //program_profile is not available, instead we have activity_home_page_url
                    activity_home_page_url_2 = accountHomePageUrl.IsBlank() ? null : $"{Helpers.Utilities.BaseUrl()}/login?returnurl={HttpUtility.UrlEncode($"{accountHomePageUrl}?edit=1")}",//program_profile_edit_link
                    program_create_date = accountCreateDate.ReturnNullIfEmpty(),
                    days_since_account_created = accountCreationDays.ReturnNullIfEmpty(),
                    last_activity_create_date = lastActivityDate.ReturnNullIfEmpty(),
                    days_since_last_activity_created = lastActivityCreationDays.ReturnNullIfEmpty(),
                    activities_created = activityCount.ReturnNullIfEmpty(),
                    profile_exists = isAccountProfileExist.ReturnNullIfEmpty(),
                    profile_started = isAccountProfileStarted.ReturnNullIfEmpty(),
                    profile_completed = isAccountProfileCompleted.ReturnNullIfEmpty(),
                    stripe_connected = stripeConnected.ReturnNullIfEmpty(), // ADDED THIS NEW custom_fields
                    email_verified = emailVerified.ReturnNullIfEmpty(), // ADDED THIS NEW custom_field
                    web_demo = 1
                }
            };

            var task = Task.Run(async () =>
            {
                return await sg.RequestAsync(
                    method: SendGridClient.Method.POST,
                    urlPath: "contactdb/recipients",
                    requestBody: contact.ToJSON());
            });

            task.Wait();
            dynamic response = task.Result;

            if (response.StatusCode != System.Net.HttpStatusCode.Created) // we are getting status code as Created, Accepted not working
            {
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
            }

            //TODO: retrive id for newly created record else give error
        }
    }
}
