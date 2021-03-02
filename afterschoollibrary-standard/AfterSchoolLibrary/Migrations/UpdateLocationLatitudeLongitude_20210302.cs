using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateLocationLatitudeLongitude_20210302
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var locations = Location.Load();
                    foreach (var item in locations)
                    {
                        //.. get latitude and longitude for 
                    }
                },
                uniqueIdenfier: "UpdateLocationLatitudeLongitude_20210302",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
