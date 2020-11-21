using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityInstanceService
    {
        public static Attendance GetAttendance(this ActivityInstance activityInstance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityInstanceId(activityInstance.ID.Value, msgs, dc).FirstOrDefault();
        }
    }
}
