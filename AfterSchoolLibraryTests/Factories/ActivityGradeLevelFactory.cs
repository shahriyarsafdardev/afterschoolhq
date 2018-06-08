using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityGradeLevelFactory
    {
        public static ActivityGradeLevel Create(Activity activity, GradeLevel gradeLevel, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivityGradeLevel() { ActivityId = activity.ID, GradeLevelId = gradeLevel.ID };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
