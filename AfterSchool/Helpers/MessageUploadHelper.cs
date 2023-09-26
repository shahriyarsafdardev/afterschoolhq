using System.Collections.Generic;
using BasicLibrary;
using System.Web;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Helpers
{
    public static class MessageUploadHelper
    {
        public static string UploadFilesAndGetLinks(List<HttpPostedFileBase> importedFiles, Account currentAccount, SystemMessages msgs)
        {
            var body = string.Empty;

            if (importedFiles.Count >= 1 && importedFiles[0].IsNotEmpty())
            {
                foreach (var file in importedFiles)
                {
                    var fileLink = currentAccount.UploadFile(file.InputStream, file.FileName, msgs);
                    body += fileLink + "\n";
                }
            }
            return body;
        }
        public static string UploadFileAndGetLink(HttpPostedFileBase importedFile, Account currentAccount, SystemMessages msgs)
        {
            var body = string.Empty;
            var fileLink = currentAccount.UploadFile(importedFile.InputStream, importedFile.FileName, msgs);
            return fileLink;
        }
    }
}
