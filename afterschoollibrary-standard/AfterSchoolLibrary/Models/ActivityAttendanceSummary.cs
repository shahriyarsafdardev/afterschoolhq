using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    public class ActivityAttendanceSummary
    {
        public ActivityAttendanceSummary() { }

        // create class properties string ActivityName, int TotalStudentsRegistered, int Reserved, int Waitlisted, int Applied, int Withdrawn, int NumberOfClasses, double AvgAttendedPerClass, double PercentAttended, int TotalActive, double PercentActive
        public string ActivityName { get; set; }
        public int TotalStudentsRegistered { get; set; }
        public int Reserved { get; set; }
        public int Waitlisted { get; set; }
        public int Applied { get; set; }
        public int Withdrawn { get; set; }
        public int NumberOfClasses { get; set; }
        public double AvgAttendedPerClass { get; set; }
        public double PercentAttended { get; set; }
        public int TotalActive { get; set; }
        public double PercentActive { get; set; }
        public int TotalAttended { get; set; }


        public static List<ActivityAttendanceSummary> LoadAttendanceSummary(int accountId, DateTime? startDate, DateTime? endDate, List<int> activities, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var reservedTypesString = $"{(int) RegistrationAssignmentType.Reserved}, {(int) RegistrationAssignmentType.ReservedPendingParentalConsent}, {(int) RegistrationAssignmentType.ReservedPendingPayment}";

            if (!startDate.HasValue)
                startDate = ("2017/12/27").ToDateTime();

            if (!endDate.HasValue)
                endDate = DateTime.UtcNow;

            var activityString = activities != null && activities.Count > 0 ? $" AND activity.id in ({string.Join(",", activities)}) " : "";

            string sql = @$"
                select 
                      ActivityName, TotalStudentsRegistered, Reserved, Waitlisted, Applied, Withdrawn, 
                      NumberOfClasses,
                      (TotalAttended/IIF(NumberOfClasses > 0, NumberOfClasses, 1)) as 'AvgAttendedPerClass',
                      (CONVERT(FLOAT, TotalAttended)/CONVERT(FLOAT, IIF(Reserved * NumberOfClasses > 0, Reserved * NumberOfClasses, 1))) * 100 as PercentAttended,
                      NumberActive,
                      (CONVERT(FLOAT, NumberActive)/CONVERT(FLOAT, IIF(Reserved > 0, Reserved, 1))) * 100 as PercentActive,
                      TotalAttended
                from 
                    (select 
                      Activity.Name as ActivityName, 
                      (select count(*) from RegistrantActivity ra where ra.ParticipationObjectId = activity.id and ra.ParticipationObjectTypeId = 1) as TotalStudentsRegistered,
                      (select count(*) from RegistrantActivity ra where ra.ParticipationObjectId = activity.id and ra.ParticipationObjectTypeId = 1 and AssignmentTypeId in ({reservedTypesString})) as Reserved,
                      (select count(*) from RegistrantActivity ra where ra.ParticipationObjectId = activity.id and ra.ParticipationObjectTypeId = 1 and AssignmentTypeId in ({(int) RegistrationAssignmentType.WaitingList})) as Waitlisted,
                      (select count(*) from RegistrantActivity ra where ra.ParticipationObjectId = activity.id and ra.ParticipationObjectTypeId = 1 and AssignmentTypeId in ({(int) RegistrationAssignmentType.Applied})) as Applied,
                      (select count(*) from RegistrantActivity ra where ra.ParticipationObjectId = activity.id and ra.ParticipationObjectTypeId = 1 and AssignmentTypeId in ({(int) RegistrationAssignmentType.Withdrawn})) as Withdrawn,
                      (select count(*) from attendance where activityid = activity.id and attendance.Date between '{startDate}' AND '{endDate}') as NumberOfClasses,
                      (select count(*) from attendanceregistrant ar inner join attendance att on ar.attendanceid = att.id where numberpresent > 0 and att.activityid = activity.id and att.Date between '{startDate}' AND '{endDate}') as TotalAttended,
                      (select count(*) from RegistrantActivity ra where ra.ParticipationObjectId = activity.id and ra.ParticipationObjectTypeId = 1 and AssignmentTypeId in ({reservedTypesString}) and (select count(*) from attendanceregistrant ar inner join attendance att on ar.attendanceid = att.id where ar.registrantid = ra.registrantid and numberpresent > 0 and att.activityid = activity.id and att.Date between '{startDate}' AND '{endDate}') > 0) as NumberActive
                from 
                    activity
                where 
                    accountid = {accountId} and
                    IsDeleted = 0 and
                    ActivityTypeId = {(int) ActivityType.Activity}
                    {activityString}
                ) as A";

            var dq = Account.get_DatabaseQuery(Account.ObjectMap, dc);
            var dt = dq.ExecuteSelect(sql, msgs);
            var list = new List<ActivityAttendanceSummary>();

            foreach(System.Data.DataRow item in dt.Rows)
            {
                // load each row into a new AttendanceSummaryReport object
                var report = new ActivityAttendanceSummary();
                report.ActivityName = item["ActivityName"].ToString();
                report.TotalStudentsRegistered = Convert.ToInt32(item["TotalStudentsRegistered"]);
                report.Reserved = Convert.ToInt32(item["Reserved"]);
                report.Waitlisted = Convert.ToInt32(item["Waitlisted"]);
                report.Applied = Convert.ToInt32(item["Applied"]);
                report.Withdrawn = Convert.ToInt32(item["Withdrawn"]);
                report.NumberOfClasses = Convert.ToInt32(item["NumberOfClasses"]);
                report.AvgAttendedPerClass = Convert.ToDouble(item["AvgAttendedPerClass"]);
                report.PercentAttended = Convert.ToDouble(item["PercentAttended"]);
                report.TotalActive = Convert.ToInt32(item["NumberActive"]);
                report.PercentActive = Convert.ToDouble(item["PercentActive"]);
                report.TotalAttended = Convert.ToInt32(item["TotalAttended"]);
                list.Add(report);
            }

            return list;

        }
    }
}
