using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220823_InitializeActivityDetailsTable {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    
                    amsgs.AddRange(dq.ExecuteSql("insert into ActivityDetails (ActivityId, Value, CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy) select ID, Details, CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy from Activity where Details is not null"));
                },
                uniqueIdenfier: "Migration_20220823_InitializeActivityDetailsTable",
                msgs: msgs,
                dc: dc
            );
        }
    }
}