using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountFactory
    {
        public static Account Create(WebUser user, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Account Create(WebUser user, AccountType accountType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com", AccountTypeId = accountType };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
