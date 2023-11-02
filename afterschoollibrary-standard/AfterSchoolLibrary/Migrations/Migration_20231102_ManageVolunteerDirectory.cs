using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20231102_ManageVolunteerDirectory
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var webUser = WebUser.Load(1, amsgs, adc);

                    // update packages, set ManageVolunteerDirectory feature to zero
                    Package.LoadByIsActive(true, amsgs, adc).ForEach(package => package.AddFeature(FeatureType.ManageVolunteerDirectory, 0, webUser, 
                        amsgs, adc));
                },
                uniqueIdenfier: "Migration_20231102_ManageVolunteerDirectory",
                msgs: msgs,
                dc: dc
            );
        }
    }
}


