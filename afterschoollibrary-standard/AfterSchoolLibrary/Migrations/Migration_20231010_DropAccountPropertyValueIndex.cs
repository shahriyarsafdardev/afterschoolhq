using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20231010_DropAccountPropertyValueIndex
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    amsgs.AddRange(dq.ExecuteSql("DROP INDEX IX_AccountProperty_AccountId_AccountPropertyTypeId_Value ON [AccountProperty]"));

                },
                uniqueIdenfier: "Migration_20231010_DropAccountPropertyValueIndex",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
