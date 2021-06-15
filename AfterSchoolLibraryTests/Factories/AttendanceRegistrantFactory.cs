using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Linq;

namespace AfterSchoolLibraryTests.Factories
{
    public class AttendanceRegistrantFactory
    {
        public static AttendanceRegistrant Create(Attendance attendance, Registrant registrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AttendanceRegistrant()
            {
                RegistrantId = registrant.ID.Value,
                AttendanceId = attendance.ID.Value,
                IsPresent = false,
                IsLate = false,
                NumberPresent = 0,
                NumberLate = 0,
                NumberPossible = 0,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
       
    }
}
