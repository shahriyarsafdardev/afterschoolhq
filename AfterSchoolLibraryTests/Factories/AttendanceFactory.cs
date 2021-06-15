using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Linq;
using System;

namespace AfterSchoolLibraryTests.Factories
{
    public class AttendanceFactory
    {
        public static Attendance Create(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Attendance()
            {
                Date = DateTime.UtcNow,
                IsCompleted = false,
                IsCharged = false,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Attendance Create(Activity activity, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Attendance()
            {
                ActivityId = activity.ID.Value,
                Date = DateTime.UtcNow,
                IsCompleted = false,
                IsCharged = false,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

    }
}
