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
        public static AttendanceRegistrant CreateNew(int attendanceId, int registrantId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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

        public static void UpdateIsPresent(this AttendanceRegistrant attendanceRegistrant, bool isPresent, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            attendanceRegistrant.NumberPresent = 0;

            if (isPresent)
                attendanceRegistrant.NumberPresent = 1;

            attendanceRegistrant.LastModifiedDate = DateTime.UtcNow;
            attendanceRegistrant.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);

            msgs.AddRange(attendanceRegistrant.Save(dc));
        }

        public static Attendance GetAttendance(this AttendanceRegistrant attendanceRegistrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.Load(attendanceRegistrant.AttendanceId.Value);
        }

        public static Registrant GetRegistrant(this AttendanceRegistrant attendanceRegistrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.Load(attendanceRegistrant.RegistrantId.Value, msgs, dc);
        }

        public static void SetTransaction(this AttendanceRegistrant attendanceRegistrant, Transaction transaction, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            attendanceRegistrant.SetTransaction(transaction.ID.Value, loggedInUser, msgs, dc);
        }

        public static void SetTransaction(this AttendanceRegistrant attendanceRegistrant, int transactionId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            attendanceRegistrant.TransactionId = transactionId;
            attendanceRegistrant.LastModifiedDate = DateTime.UtcNow;
            attendanceRegistrant.LastModifiedBy = loggedInUser?.ID;
            msgs.AddRange(attendanceRegistrant.Save(dc));
        }

        public static bool AddNextAvailablePrePayment(this AttendanceRegistrant attendanceRegistrant, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var prePayment = attendanceRegistrant.GetAvailablePrePayments(msgs, dc).FirstOrDefault();

            if (prePayment.IsNotEmpty())
            {
                attendanceRegistrant.SetTransaction(prePayment.TransactionId.Value, loggedInUser, msgs, dc);
                prePayment.SetAttendanceRegistrant(attendanceRegistrant, loggedInUser, msgs, dc);

                return attendanceRegistrant.IsAlreadyBilled;
            }

            return false;
        }

        public static List<SubscriptionPlanPrePayment> GetAvailablePrePayments(this AttendanceRegistrant attendanceRegistrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var prepayments = new List<SubscriptionPlanPrePayment>();

            if (!attendanceRegistrant.IsAlreadyBilled)
            {
                var attendance = attendanceRegistrant.GetAttendance(msgs, dc);
                var registrant = attendanceRegistrant.GetRegistrant(msgs, dc);
                var activity = attendance.GetActivity(msgs, dc);
                var participationObject = activity.GetParticipationObject(msgs, dc);
                var registration = registrant.GetRegistrationWithParticipationObject(participationObject);

                prepayments = SubscriptionPlanPrePayment.LoadUnused(
                                registration.ID.Value,
                                participationObject.ObjectID.Value,
                                participationObject.ObjectType,
                                msgs,
                                dc);
            }

            return prepayments;
        }

        public static List<AttendanceRegistrant> LoadAttendanceRegistrantsInSameBillingPeriod(this AttendanceRegistrant attendanceRegistrant, BillingPeriodType billingPeriodType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            switch (billingPeriodType)
            {
                case BillingPeriodType.Week:
                   var thisAttendance = Attendance.Load(attendanceRegistrant.AttendanceId.Value, msgs);
                   var attendancesInSameWeek = Attendance.LoadByActivityId(attendanceRegistrant.AttendanceActivityId.Value, msgs).Where( a => thisAttendance.Date.Value.DatesAreInSameWeek(a.Date.Value)).ToList();
                   var billAttendanceRegistrants = new List<AttendanceRegistrant>();
                   attendancesInSameWeek.ForEach(attendanceInSameWeek =>
                   {
                       var attenRegInSameWeek = AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(attendanceInSameWeek.ID.Value, attendanceRegistrant.RegistrantId.Value, msgs).FirstOrDefault();
                       if (attenRegInSameWeek.IsNotEmpty())
                           billAttendanceRegistrants.Add(attenRegInSameWeek);
                   });

                    return billAttendanceRegistrants;

                default:
                    return new List<AttendanceRegistrant>() { attendanceRegistrant };
            }
        }
    }
}
