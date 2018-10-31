using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class AttendanceRegistrantService
    {
        public static AttendanceRegistrant CreateNew(int attendanceId, int registrantId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var attendanceRegistrant = new AttendanceRegistrant
            {
                AttendanceId = attendanceId,
                IsPresent = false,
                NumberPresent = 0,
                IsLate = false,
                NumberLate = 0,
                NumberPossible = 1,
                RegistrantId = registrantId,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser.ID,
                CreatedBy = loggedInUser.ID
            };
            msgs.AddRange(attendanceRegistrant.Save(dc));

            return attendanceRegistrant;
        }
    }
}
