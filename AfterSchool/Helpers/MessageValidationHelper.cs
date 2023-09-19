using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Helpers
{
    public static class MessageValidationHelper
    {
        public static void ValidateAttachments(List<HttpPostedFileBase> importedFiles, SystemMessages msgs)
        {
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
        }
    }
}
