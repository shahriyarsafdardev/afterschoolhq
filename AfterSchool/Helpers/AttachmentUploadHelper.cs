using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Helpers
{
    public static class AttachmentUploadHelper
    {
        public static string UploadFile(Account account, Stream file, string filename, SystemMessages msgs)
        {
            var ah = Utilities.GetNewAmazonHelper();
            var regEx = new Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx,
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );
            var accountFolder = $"{account.UniqueName.Replace(regEx, "")}-{account.ID}";
            var amazonFilePath = $"{accountFolder}/{filename}".ToLower();

            msgs.AddRange(ah.AddFile(file, amazonFilePath, true));

            return ah.GetFileLink(amazonFilePath);
        }
        public static string UploadAttachments(Account account, List<HttpPostedFileBase> importedFiles, string body, SystemMessages msgs)
        {
            body = body.Replace("\n", "<br>");
            if (importedFiles.Count >= 1 && importedFiles[0].IsNotEmpty())
            {
                var attachmentLabel = importedFiles.Count == 1 ? "Attachment" : "Attachments";
                var downloadLabel = importedFiles.Count == 1 ? "Download" : "Download All";
                body += $@"<div class=""attachments text-left""><p> <span class=""text-muted "">{importedFiles.Count} {attachmentLabel}</span> <span class=""color-green margin-l-10 download-link"">{downloadLabel}</span></p>";
                foreach (var file in importedFiles)
                {
                    var fileSize = ((double)file.ContentLength / 1048576);
                    var formattedSize = fileSize.ToString("F2") + "mb";
                    if (fileSize == 0)
                    {
                        fileSize = (double)file.ContentLength / 1024;
                        formattedSize = fileSize.ToString("F2") + "kb";
                    }
                    var fileLink = UploadFile(account, file.InputStream, file.FileName, msgs);
                    body += $@"<div class=""file-box""><div class=""file-box-icon""><i class=""icon icon-left s7-file attachment-icon""></i></div><div class=""file-box-info""><a class=""file-link"" href=""{fileLink}"" data-file-name=""{file.FileName}""  TARGET = ""_blank"">{file.FileName}</a><span class=""text-muted"">{formattedSize}</span></div></div>";
                }
                body += @"</div>";
            }
            return body;
        }
        public static SystemMessages ValidateAttachments(List<HttpPostedFileBase> importedFiles)
        {
            var msgs = new SystemMessages();
            var allowedFiles = new string[] { ".png", ".jpg", ".jpeg", ".pdf" };

            if (importedFiles.Count >= 1 && importedFiles[0].IsNotEmpty())
            {
                var totalFilesSize = importedFiles.Sum(file => file.ContentLength);
                if (totalFilesSize > Utilities.MessageTotalFileSize)
                    msgs.Add($"Total attached files size should be less than {Utilities.MessageTotalFileSize} bytes", MessageType.UserError);
                if (msgs.HasNoErrors())
                {
                    foreach (var file in importedFiles)
                    {
                        if (!file.FileName.ToLower().Contains(allowedFiles) ||
                            file.ContentLength >= Utilities.MessageFileSize)
                        {
                            msgs.Add("Please upload a valid jpg, png, or pdf file that is less than 25MB",
                                MessageType.UserError);
                            break;
                        }
                    }
                }
            }
            return msgs;
        }
    }
}
