using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230918_StudentNotes
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var webUser = WebUser.Load(1, amsgs, adc);

                    // update packages, set ManageStudentNotes feature to zero
                    Package.LoadByIsActive(true, amsgs, adc).ForEach(package => package.AddFeature(FeatureType.ManageStudentNotes, 0, webUser, amsgs, adc));
                },
                uniqueIdenfier: "Migration_20230918_StudentNotes",
                msgs: msgs,
                dc: dc
            );
        }
    }
}


