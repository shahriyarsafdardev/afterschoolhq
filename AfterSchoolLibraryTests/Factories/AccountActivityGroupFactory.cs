using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountActivityGroupFactory
    {
        public static AccountActivityGroup Create(Account account, ActivityGroup activityGroup, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountActivityGroup()
            {
                AccountId = account.ID,
                ActivityGroupId = activityGroup.ID
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
        public static AccountActivityGroup Create(Account account, ActivityGroup activityGroup, decimal? cost, InviteStatus inviteStatus, bool isPublished, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountActivityGroup()
            {
                AccountId = account.ID,
                ActivityGroupId = activityGroup.ID,
                Cost = cost,
                CostUnitTypeId = CostUnitType.PerRegistrant,
                InviteStatusId = inviteStatus,
                IsPublished = isPublished
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
