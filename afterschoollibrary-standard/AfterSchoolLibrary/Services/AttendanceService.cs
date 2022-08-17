using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class AttendanceService
    {
        public static void InitAttendance(this Attendance attendance, Activity activity, WebUser loggedInUser, ActivityInstance activityInstance = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            attendance.IsCompleted = false;
            attendance.LastModifiedDate = attendance.CreateDate = DateTime.UtcNow;
            attendance.LastModifiedBy = attendance.CreatedBy = loggedInUser.ID;
            attendance.ActivityId = activity.ID.Value;

            if (activityInstance.IsNotEmpty())
            {
                attendance.ActivityInstanceId = activityInstance.ID.Value;
                attendance.Date = activityInstance.StartDateTime;
            }
            else
            {
                var closestActivityInstance = activity.GetClosestActivityInstance(msgs, dc);

                attendance.ActivityInstanceId = closestActivityInstance.ID;
                attendance.Date = closestActivityInstance.StartDateTime;
            }

            msgs.AddRange(attendance.Save(dc));

            attendance.InitAttendanceRegistrants(activity, loggedInUser, msgs, dc);
        }

        private static void InitAttendanceRegistrants(this Attendance attendance, Activity activity, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var registrantActivities = activity.GetRegistrantActivities(msgs, dc);

            registrantActivities.Where(registrant => registrant.AssignmentTypeId.In(Registration.ReservedStatuses))
                .ToList()
                .ForEach(registrantActivity =>
                {
                    AttendanceRegistrantService.CreateNew(attendance.ID.Value, registrantActivity.RegistrantId.Value, loggedInUser, msgs, dc);
                });
        }

        public static void UpdateAttendance(this Attendance attendance, int activityInstanceId, List<int> attendanceRegistrantIds, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    attendance.ActivityInstanceId = activityInstanceId;
                    attendance.IsCompleted = true;
                    attendance.LastModifiedBy = loggedInUser.ID;
                    attendance.LastModifiedDate = DateTime.UtcNow;
                    amsgs.AddRange(attendance.Save(adc));

                    attendance.GetAttendanceRegistrants(amsgs, adc).ForEach(attendanceRegistrant =>
                    {
                        attendanceRegistrant.UpdateIsPresent(attendanceRegistrant.ID.Value.In(attendanceRegistrantIds), loggedInUser, amsgs, adc);
                    });
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Attendance saved", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static ActivityInstance GetActivityInstance(this Attendance attendance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityInstance.Load(attendance.ActivityInstanceId.Value, msgs, dc);
        }

        public static List<AttendanceRegistrant> GetAttendanceRegistrants(this Attendance attendance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.LoadByAttendanceId(attendance.ID.Value, dc: dc);
        }

        public static AttendanceRegistrant GetAttendanceRegistrant(this Attendance attendance, int registrantId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(attendance.ID.Value, registrantId, msgs, dc).LastOrDefault();
        }

        public static int GetAttendanceRegistrantCount(this Attendance attendance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCount(attendance.ID.Value, dc: dc);
        }

        public static int GetPresentCount(this Attendance attendance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCount(attendance.ID.Value, msgs, dc);
        }

        public static Activity GetActivity(this Attendance attendance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (attendance.ActivityId.HasValue)
                return Activity.Load(attendance.ActivityId.Value, msgs, dc);

            return null;
        }
    }
}