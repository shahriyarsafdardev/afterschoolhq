using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void UpdateIsPresent(this AttendanceRegistrant attendanceRegistrant, bool isPresent, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            attendanceRegistrant.NumberPresent = 0;

            if (isPresent)
                attendanceRegistrant.NumberPresent = 1;

            attendanceRegistrant.LastModifiedDate = DateTime.UtcNow;
            attendanceRegistrant.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);

            msgs.AddRange(attendanceRegistrant.Save(dc));

            //if (isPresent)
            //{
            //    var activity = Activity.Load(attendanceRegistrant.AttendanceActivityId.Value, msgs, dc);
            //    var postPayPlan = activity.GetPostPayPlan(msgs, dc);

            //    if (postPayPlan.IsNotEmpty())
            //    {
            //        var registrant = Registrant.Load(attendanceRegistrant.RegistrantId.Value, msgs, dc);
            //        var registration = registrant.GetRegistrations(msgs, dc).First();
            //        var masterRegistrant = registrant.GetMasterRegistrant(msgs, dc);
            //        var payingAccount = masterRegistrant.GetAccount(msgs, dc);

            //        var charge = Billing.StripeHelper.CreateCharge(
            //                        payingAccount.StripeCustomerId,
            //                        registration.StripeSourceId,
            //                        registration.CostPerClass.Value,
            //                        "Test charge",
            //                        "AHQ Charge",
            //                        null,
            //                        msgs);
            //    }
            //}
        }
    }
}
