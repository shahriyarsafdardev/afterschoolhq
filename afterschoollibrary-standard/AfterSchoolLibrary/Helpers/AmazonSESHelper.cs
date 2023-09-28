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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Linq;

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
            //if (Utilities.IsLocalHost)
            //    msgs.AddRange(SMTPMailTrapHelper.SendMail(
            //    new EmailAddress(fromEmail),
            //    new EmailAddress(toEmail),
            //    subject,
            //    body,
            //    msgs,
            //    emailTemplateId: emailTemplateId
            //    ));
            //else
            //{
            if (!Utilities.IsProduction)
                subject = $"{Utilities.Environment} ENV: {subject}";

            using (var client = new AmazonSimpleEmailServiceClient(Utilities.AmazonAccessKeyId, Utilities.AmazonSecretAccessKeyId, Utilities.AmazonRegionEndPoint))
            {
                var templateDataObj = new { subject = subject.ToString(), body = body.ToString() };
                string templateData = JsonConvert.SerializeObject(templateDataObj);
                string sanitizedEmail = new string(toEmail.Email
                .Where(c => Char.IsLetterOrDigit(c) || c == '_' || c == '-')
                .ToArray());
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
                    //Tags = new List<MessageTag> { new MessageTag { Name = "ses_message_tag", Value = sanitizedEmail } }
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
            //}
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

            try
            {
                // Initialize your SES client
                var sesClient = new AmazonSimpleEmailServiceClient(Utilities.AmazonAccessKeyId, Utilities.AmazonSecretAccessKeyId, Utilities.AmazonRegionEndPoint);
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;
                MailMessage message = new MailMessage(fromEmail.Email.ToString(), toEmail.Email.ToString(), subject, body);
                var attachmentStream = new MemoryStream(attachmentByteData);
                var attachment = new System.Net.Mail.Attachment(attachmentStream, attachmentFilename);
               
                message.Attachments.Add(attachment);
                using (MemoryStream ms = new MemoryStream())
                {
                    ReadPdfIntoMemoryStream(ms,attachmentFilename, attachmentByteData);
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
                    var request = new SendRawEmailRequest { RawMessage = new RawMessage(messageStream), ConfigurationSetName = "ses_logs" };
                    //var task = await sesClient.SendRawEmailAsync(request);
                    // Create the SendRawEmailRequest
                    //var request = new SendRawEmailRequest
                    //{
                    //    RawMessage = new RawMessage
                    //    {
                    //        Data = CreateRawEmailData(
                    //            fromEmail.Email.ToString(),
                    //            toEmail.Email.ToString(),
                    //            subject,
                    //            body,
                    //            attachmentStream,
                    //            attachmentFilename
                    //        )
                    //    },
                    //    Destinations = new List<string> { toEmail.Email.ToString() }, // Destination email address
                    //    Source = fromEmail.Email.ToString(), // Sender email address
                    //                                         // Reply-To addresses can be added here if needed
                    //};

                    //using (MemoryStream memoryStream = new MemoryStream())
                    //{
                    //    message.Save(memoryStream);
                    //    SendRawEmailRequest request = new SendRawEmailRequest
                    //    {
                    //        RawMessage = new RawMessage { Data = memoryStream }
                    //    };
                    //    await client.SendRawEmailAsync(request);
                    //}

                    // Send the email with attachments
                    var task = Task.Run(async () => await sesClient.SendRawEmailAsync(request));
                    //var response = sesClient.SendRawEmailAsync(request);

                    dynamic response = task.Result;
                    if (response.HttpStatusCode != HttpStatusCode.OK)
                        msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error);
            }

            return msgs;
        }

        private static MemoryStream CreateRawEmailData(string from, string to, string subject, string body, Stream attachmentStream = null, string attachmentFilename = null)
        {
            var memoryStream = new MemoryStream();

            using (var writer = new StreamWriter(memoryStream))
            {
                writer.WriteLine($"From: {from}");
                writer.WriteLine($"To: {to}");
                writer.WriteLine($"Subject: {subject}");
                writer.WriteLine("MIME-Version: 1.0");
                writer.WriteLine("Content-Type: multipart/mixed; boundary=\"boundary-1\"");
                writer.WriteLine();
                writer.WriteLine("--boundary-1");
                writer.WriteLine("Content-Type: text/plain; charset=UTF-8");
                writer.WriteLine("Content-Transfer-Encoding: 7bit");
                writer.WriteLine();
                writer.WriteLine(body);

                if (attachmentStream != null)
                {
                    writer.WriteLine();
                    writer.WriteLine("--boundary-1");
                    writer.WriteLine($"Content-Type: application/octet-stream; name=\"{attachmentFilename}\"");
                    writer.WriteLine("Content-Description: Attachment");
                    writer.WriteLine($"Content-Disposition: attachment; filename=\"{attachmentFilename}\"; size={attachmentStream.Length};");
                    writer.WriteLine("Content-Transfer-Encoding: base64");
                    writer.WriteLine();
                    attachmentStream.Seek(0, SeekOrigin.Begin);
                    attachmentStream.CopyTo(writer.BaseStream);
                }

                writer.WriteLine();
                writer.WriteLine("--boundary-1--");
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
