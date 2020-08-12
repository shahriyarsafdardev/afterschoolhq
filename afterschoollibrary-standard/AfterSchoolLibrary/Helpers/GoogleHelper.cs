using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace AfterSchoolLibrary.Helpers
{
    namespace GoogleHelper
    {
        public class Recaptcha
        {
            public static string DataSiteKey { get { return ConfigurationManager.AppSettings["Google.Recaptcha.DataSiteKey"]; } }
        }
    }
}
