using AfterSchoolLibrary.Billing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    public class StripeUploadFile
    {
        public Stream Document { get; set; }
        public string FileName { get; set; }
        public StripeFilePurpose Purpose { get; set; }

        public StripeUploadFile(Stream document, string fileName, StripeFilePurpose purpose)
        {
            Document = document;
            FileName = fileName;
            Purpose = purpose;
        }
    }
}
