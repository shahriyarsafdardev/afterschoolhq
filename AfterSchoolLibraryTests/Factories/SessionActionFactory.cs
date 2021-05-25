using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class SessionActionFactory
    {
        public static SessionAction Create(Account account, Session session, Activity activity, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SessionAction() { SessionId = session.ID.Value, SessionActionTypeId = SessionActionType.ActivityCreate, AccountId = account.ID, ActivityId = activity.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
