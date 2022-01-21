using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests
{
    internal static class TestHelpers
    {
        public static DatabaseCredentials TestDatabaseCredentials()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location.ToLower().Contains("work")
                ? Database.TestCredentials
                : Database.LocalTestCredentials;
        }
    }
}