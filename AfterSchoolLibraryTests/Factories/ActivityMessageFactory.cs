using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityMessageFactory
    {
        public static ActivityMessage Create(Account account, Activity activity, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityMessage() { AccountId = account.ID, ActivityId = activity.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
