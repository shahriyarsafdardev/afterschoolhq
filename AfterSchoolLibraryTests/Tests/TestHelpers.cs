using System.Reflection;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests
{
    internal static class TestHelpers
    {
        public static DatabaseCredentials TestDatabaseCredentials()
        {
            return new DatabaseCredentials("localhost", "AfterSchoolTest", "AfterSchoolUser", "Nj8Ed$wpLfJ");
        }
    }
}