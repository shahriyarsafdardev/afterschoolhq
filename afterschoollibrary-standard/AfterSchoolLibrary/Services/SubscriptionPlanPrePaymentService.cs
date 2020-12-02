using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionPlanPrePaymentService
    {
        public static void SetAttendanceRegistrant(this SubscriptionPlanPrePayment prePayment, AttendanceRegistrant attendanceRegistrant, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            prePayment.AttendanceRegistrantId = attendanceRegistrant.ID;
            prePayment.LastModifiedDate = DateTime.UtcNow;
            prePayment.LastModifiedBy = loggedInUser?.ID;
            msgs.AddRange(prePayment.Save(dc));
        }
    }
}
