using Amazon.SimpleEmail.Model;
using Amazon.SimpleEmail;
using BasicLibrary;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;
using System.IO;
using System;
using AfterSchoolLibrary.Helpers;
using System.Net.Mail;
using MimeKit;

namespace AfterSchoolLibrary
{
    internal class AmazonSESHelper
    {
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
            if (Utilities.IsLocalHost)
                msgs.AddRange(SMTPMailTrapHelper.SendMail(
                fromEmail,
                toEmail,
                subject,
                body,
                msgs,
                emailTemplateId: emailTemplateId
                ));
            else
            {
                if (!Utilities.IsProduction)
                subject = $"{Utilities.Environment} ENV: {subject}";

                using (var client = new AmazonSimpleEmailServiceClient(Utilities.AmazonAccessKeyId, Utilities.AmazonSecretAccessKeyId, Utilities.AmazonRegionEndPoint))
                {
                    var templateDataObj = new { subject = subject.ToString(), body = body.ToString() };
                    string templateData = JsonConvert.SerializeObject(templateDataObj);
                    var mail = new SendTemplatedEmailRequest
                    {
                        Source = fromEmail.Email.ToString(),
                        Template = "AHQGenericTemplate6",
                        Destination = new Destination
                        {
                            ToAddresses = new List<string> { toEmail.Email }
                        },
                        TemplateData = templateData,
                        ConfigurationSetName = "ses_logs",
                        Tags = new List<MessageTag> { new MessageTag { Name = "ses_message_tag", Value = "afterschoolhq" } }
                    };
                    if (replyTo != null)
                        mail.ReplyToAddresses = new List<string> { replyTo.Email.ToString() };

                    if (ccEmail != null)
                        mail.Destination.CcAddresses = new List<string> { ccEmail.Email.ToString() };

                    var task = Task.Run(async () => await client.SendTemplatedEmailAsync(mail));
                    dynamic response = task.Result;
                    if (response.HttpStatusCode != HttpStatusCode.OK)
                        msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
                }
            }
            return msgs;
        }

        public static void ReadPdfIntoMemoryStream(MemoryStream memoryStream, string fileName, byte[] attachmentBytes)
        {
            memoryStream.Write(attachmentBytes, 0, attachmentBytes.Length);
        }

        public static  SystemMessages SendMailWithAttachments(EmailAddress fromEmail, EmailAddress toEmail,
            string subject, string body, byte[] attachmentByteData, string attachmentFilename)
        {
            var msgs = new SystemMessages();

            if (Utilities.IsLocalHost)
                msgs.AddRange(SMTPMailTrapHelper.SendMailwithAttachment(fromEmail,
                    toEmail,
                    subject,
                    body,
                    attachmentByteData,
                    attachmentFilename,
                    msgs));
            else
            {
                try
                {
                    var sesClient = new AmazonSimpleEmailServiceClient(Utilities.AmazonAccessKeyId, Utilities.AmazonSecretAccessKeyId, Utilities.AmazonRegionEndPoint);
                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = body;
                    MailMessage message = new MailMessage(fromEmail.Email.ToString(), toEmail.Email.ToString(), subject, body);
                    var attachmentStream = new MemoryStream(attachmentByteData);
                    var attachment = new System.Net.Mail.Attachment(attachmentStream, attachmentFilename);

                    message.Attachments.Add(attachment);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ReadPdfIntoMemoryStream(ms, attachmentFilename, attachmentByteData);
                        ms.Seek(0, System.IO.SeekOrigin.Begin);
                        bodyBuilder.Attachments.Add(attachmentFilename, ms, new ContentType("application", "pdf"));
                    }
                    var mimeMessage = new MimeMessage();
                    mimeMessage.From.Add(new MailboxAddress(fromEmail.Name, fromEmail.Email));
                    mimeMessage.To.Add(new MailboxAddress(toEmail.Name, toEmail.Email));
                    mimeMessage.Subject = subject;
                    mimeMessage.Body = bodyBuilder.ToMessageBody();

                    using (var messageStream = new MemoryStream())
                    {
                        Task.Run(async () => await mimeMessage.WriteToAsync(messageStream));
                        var request = new SendRawEmailRequest { RawMessage = new RawMessage(messageStream), ConfigurationSetName = "ses_logs",
                                      Tags = new List<MessageTag> { new MessageTag { Name = "ses_message_tag", Value = "afterschoolhq" } }};
                        var task = Task.Run(async () => await sesClient.SendRawEmailAsync(request));

                        dynamic response = task.Result;
                        if (response.HttpStatusCode != HttpStatusCode.OK)
                            msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
                    }
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error);
                }
            }
            return msgs;
        }
    }
}
