using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountActivityGroupFactory
    {
        public static AccountActivityGroup Create(Account account, ActivityGroup activityGroup, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new AccountActivityGroup()
            {
                AccountId = account.ID,
                ActivityGroupId = activityGroup.ID
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
