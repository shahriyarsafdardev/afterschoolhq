using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class AttendanceService
    {
        public static void InitAttendance(this Attendance attendance, Activity activity, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            attendance.Date = DateTime.UtcNow.Date;
            attendance.IsCompleted = false;
            attendance.LastModifiedDate = attendance.CreateDate = DateTime.UtcNow;
            attendance.LastModifiedBy = attendance.CreatedBy = loggedInUser.ID;
            attendance.ActivityId = activity.ID.Value;

            var instances = activity
                .GetSchedule(msgs: msgs, dc: dc)
                .GetInstances(msgs: msgs, dc: dc);

            var closestDate = instances
                .Select(y => y.StartDateTime.Value)
                .ClosestDate(activity.GetAccount(msgs, dc).ConvertToAccountTimeZone(DateTime.UtcNow));

            attendance.ActivityInstanceId = instances.Where(x => x.StartDateTime.Value == closestDate).First().ID;

            msgs.AddRange(attendance.Save(dc));

            attendance.InitAttendanceRegistrants(activity, loggedInUser, msgs, dc);
        }

        private static void InitAttendanceRegistrants(this Attendance attendance, Activity activity, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = activity.GetRegistrantActivities(msgs, dc);
            var assignmentTypes = new RegistrationAssignmentType[]
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingPayment
            };

            registrantActivities.Where(registrant => registrant.AssignmentTypeId.In(assignmentTypes))
                .ToList()
                .ForEach(registrantActivity => {
                    AttendanceRegistrantService.CreateNew(attendance.ID.Value, registrantActivity.RegistrantId.Value, loggedInUser, msgs, dc);
            });
        }

        public static void UpdateAttendance(this Attendance attendance, int activityInstanceId, List<int> attendanceRegistrantIds, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    attendance.ActivityInstanceId = activityInstanceId;
                    attendance.IsCompleted = true;
                    attendance.LastModifiedBy = loggedInUser.ID;
                    attendance.LastModifiedDate = DateTime.UtcNow;
                    msgs.AddRange(attendance.Save(dc));

                    attendance.GetAttendanceRegistrants(msgs, dc).ForEach(attendanceRegistrant => {

                        attendanceRegistrant.NumberPresent = 0;

                        if (attendanceRegistrant.ID.Value.In(attendanceRegistrantIds))
                            attendanceRegistrant.NumberPresent = 1;

                        msgs.AddRange(attendanceRegistrant.Save(dc));
                    });
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Attendance saved", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );
        }
    }
}
