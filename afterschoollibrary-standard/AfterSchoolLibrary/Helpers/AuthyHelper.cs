using System.Configuration;

namespace AfterSchoolLibrary.Helpers
{
    public class AuthyHelper
    {
        private static string ProductionKey { get { return ConfigurationManager.AppSettings["Authy.ProductionKey"]; } }


        
    }
}
