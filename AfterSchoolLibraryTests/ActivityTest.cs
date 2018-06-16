using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class ActivityTest : BaseTest
    {
        public ActivityTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LoadTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.NotNull(Activity.Load(activity.ID.Value, dc: dc));
        }

        [Fact]
        public void GetCountTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.Equal(1, Activity.GetCount(null, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            Assert.True(activity.Save(dc: dc).HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            activity.Delete(dc: dc);
            Assert.Null(Activity.Load(activity.ID.Value, dc: dc));
        }

        [Fact]
        public void CreateTest()
        {
            Assert.True(Activity.Create(dc: dc).HasNoErrors());
        }

        [Fact]
        public void AlterTest()
        {
            Assert.True(Activity.Alter(dc: dc).HasNoErrors());
        }


        [Fact]
        public void ReloadTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            Assert.NotNull(activity.Reload(dc: dc));
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            Assert.Single(Activity.LoadByAccountId(account.ID.Value, dc: dc));
        }

        [Fact]
        public void LoadByAccountIdByIsPublishedTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            activity.IsPublished = true;
            activity.Save(dc: dc);
            Assert.Single(Activity.LoadByAccountIdByIsPublished(account.ID.Value, true, dc: dc));
        }

        [Fact]
        public void LoadByLocationIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var location = new Location() { };
            location.Save(dc: dc);
            activity.LocationId = location.ID.Value;
            activity.Save(dc: dc);
            Assert.Single(Activity.LoadByLocationId(activity.LocationId.Value, dc: dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);

            Assert.NotNull(Activity.LoadByUid(activity.Uid, dc: dc));
        }

        [Fact]
        public void LoadByActivityGroupIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc: dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            activity.Save(dc: dc);
            Assert.Single(Activity.LoadByActivityGroupId(activity.ActivityGroupId.Value, dc: dc));
        }

        [Fact]
        public void LoadByActivityScheduleIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            activity.ActivityScheduleId = 12;
            activity.Save(dc: dc);
            Assert.Single(Activity.LoadByActivityScheduleId(activity.ActivityScheduleId.Value, dc: dc));
        }

        [Fact]
        public void LoadByUniqueNameByAccountIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            activity.UniqueName = "test";
            activity.Save(dc: dc);
            Assert.NotNull(Activity.LoadByUniqueNameByAccountId(activity.UniqueName, account.ID.Value, dc: dc));
        }

        [Fact]
        public void LoadListTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);

            List<int> list = new List<int>();
            list.Add(activity.ID.Value);


            Assert.Single(Activity.LoadList(list, dc: dc));
        }

        [Fact]
        public void GetCountByAccountIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc: dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            activity.Save(dc: dc);
            Assert.Equal(1, Activity.GetCountByAccountId(account.ID.Value, activity.ActivityGroupId.Value, false, false, dc: dc));
        }

        [Fact]
        public void LoadByAccountIdLongTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            Assert.NotNull(Activity.LoadByAccountId(account.ID.Value, null, true, false, 0, 100, dc: dc));
        }

        [Fact]
        public void SetIsPublishedTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            activity.SetIsPublished(true, account.ID.Value, dc: dc);
            Assert.True(activity.IsPublished);
        }

        /*
        [Fact]
        public void GetParentActivityTest()
        {
            var a = new Activity() { ParentActivityId = 2 };
            a.Save(dc: dc);

            Assert.NotNull(a.GetParentActivity(dc: dc));
        }
        */

        [Fact]
        public void GetDaysTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);

            var d = new ActivityDay() { ActivityId = activity.ID.Value };
            d.Save(dc: dc);
            Assert.Equal(1, activity.GetDays(dc: dc).Count);
        }

        [Fact]
        public void GetDaysOfTheWeekTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc: dc);

            var s = new DateTime(1, 1, 1, 1, 1, 1);
            var e = new DateTime(7, 7, 7, 7, 7, 7);

            schedule.StartDate = s;
            schedule.EndDate = e;

            var activityShedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc: dc);
            activity.ActivityScheduleId = activityShedule.ID.Value;
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, activityShedule, msgs, dc: dc);
            activityInstance.StartDateTime = s;
            var d = new ActivityDay() { ActivityId = activity.ID.Value };
            d.Save(dc: dc);
            d.ActivityId = activity.ID.Value;

            Assert.Single(activity.GetDaysOfTheWeek(dc: dc));
        }

        [Fact]
        public void GetRegistrantsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            var x = activity.GetRegistrants(dc: dc);
            Assert.Single(activity.GetRegistrants(dc: dc));
        }
        
        [Fact]
        public void GetRegistrantsActivitiesLongTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.Single(activity.GetRegistrantActivities(true, dc: dc));
        }

        [Fact]
        public void GetRegistrantsCountTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
            activity.SeatsAvailable = 1;
            activity.Save(dc: dc);

            Assert.NotEqual(0, activity.GetRegistrantCount(dc: dc));
        }

        [Fact]
        public void GetRegistrantCountByApplicationTypeTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            var n = new RegistrantAssignment() { RegistrantId = registrant.ID.Value };

            Assert.Equal(1, activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Invited, dc: dc));
        }
        //weird error with DeBugging
        [Fact]
        public void isFullTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
            activity.SeatsAvailable = 1;
            activity.Save(dc: dc);
            var x = activity.IsFull();
            Assert.False(activity.IsFull(dc: dc));
        }

        [Fact]
        public void isWaitingListFullTest()
        {
            var a = new Activity() { };
            a.Save(dc: dc);
            Assert.True(a.IsWaitingListFull(dc: dc)); //0 open with 0 on waiting list means full
        }

        // difference between grade and grade level?
        [Fact]
        public void HasGradeLevelTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc: dc);

            var activityGrade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc: dc);

            var x = activity.HasGradeLevel(gradeLevel, dc: dc);

            Assert.True(activity.HasGradeLevel(gradeLevel, dc: dc));
        }

        [Fact]
        public void GetGradeLevelsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc: dc);
            gradeLevel.ID = 12;
            gradeLevel.Save(dc: dc);

            var activityGrade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc: dc);

            Assert.Single(activity.GetGradeLevels(dc: dc));
        }

        [Fact]
        public void GetTeamMembersTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var User = Factories.WebUserFactory.Create(msgs, dc: dc);
            var activityWebUser = Factories.ActivityWebUserFactory.Create(activity, User, msgs, dc: dc);

            Assert.Single(activity.GetTeamMembers(dc: dc));
        }


        [Fact]
        public void GetFormsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);

            Assert.Single(activity.GetForms(dc: dc));
        }



        [Fact]
        public void GetActiveFormTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            activityForm.IsActive = true;
            activityForm.Save(dc: dc);

            Assert.NotNull(activity.GetActiveForm(dc: dc));
        }

        [Fact]
        public void GetActivityFormsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);

            Assert.Single(activity.GetActivityForms(dc: dc));
        }


        [Fact]
        public void GetAttendanceTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);

            Assert.Single(activity.GetAttendance(dc: dc));
        }

        [Fact]
        public void GetCompletedAttendanceTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.IsCompleted = true;
            attendance.Save(dc: dc);

            Assert.Single(activity.GetCompletedAttendance(dc: dc));
        }

        [Fact]
        public void GetAttendancePresentCountTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = Factories.RegistrantFactory.Create(account, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);

            Assert.Equal(1, activity.GetAttendancePresentCount(dc: dc));
        }

        [Fact]
        public void GetAttendancePossibleCountTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = Factories.RegistrantFactory.Create(account, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);

            Assert.Equal(1, activity.GetAttendancePossibleCount(dc: dc));
        }

        [Fact]
        public void GetAttendanceByDateTest()
        {
            var date = new DateTime(2018, 10, 10) { };
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc: dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = Factories.RegistrantFactory.Create(account, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);
            attendance.Date = date;
            attendance.Save(dc: dc);


            var count = activity.GetAttendanceByDate(date, dc: dc);

            Assert.Single(activity.GetAttendanceByDate(date, dc: dc));
        }

        [Fact]
        public void GetActivityRegistrationPeriodsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetActivityRegistrationPeriods(dc: dc));
        }

        [Fact]
        public void GetRegistrationPeriodsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetRegistrationPeriods(dc: dc));
        }

        // HELP
        [Fact]
        public void GetUpcommingRegistrationPeriodsTest()
        {
            var date = new DateTime(2018, 10, 10);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);
            registrationPeriod.StartDateTime = date;
            registrationPeriod.Save(dc: dc);
            //activityRegistrationPeriod.RegistrationPeriodStartDateTime = date;
            activityRegistrationPeriod.Save(dc: dc);
            activity.StartDateTime = date;
            activity.Save(dc: dc);

            Assert.Single(activity.GetUpcomingRegistrationPeriods(dc: dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);
            registrationPeriod.StartDateTime = new DateTime(18, 10, 10);
            registrationPeriod.EndDateTime = new DateTime(2018, 10, 10);

            Assert.NotNull(activity.GetCurrentRegistrationPeriod(dc: dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);
            registrationPeriod.StartDateTime = new DateTime(18, 10, 10);
            registrationPeriod.EndDateTime = new DateTime(2018, 10, 10);

            Assert.NotNull(activity.GetCurrentOrNextRegistrationPeriod(dc: dc));
        }

        [Fact]
        public void GetActiveRegistrationPeriodTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);
            activityRegistrationPeriod.IsActive = true;
            activityRegistrationPeriod.Save(dc: dc);

            Assert.NotNull(activity.GetActiveRegistrationPeriod(dc: dc));
        }

        [Fact]
        public void GetAccountTest()
        {

            var account = new Account() { };
            account.Save(dc: dc);
            var aid = account.ID.Value;

            var a = new Activity() { AccountId = aid };
            a.Save(dc: dc);

            Assert.NotNull(a.GetAccount(dc: dc));
        }

        [Fact]
        public void CopyTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);

            Assert.NotNull(activity.Copy("newActivity", false, user.ID.Value, msgs, dc: dc));
        }

        [Fact]
        public void CopyDefaultFormTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            account.DefaultFormId = form.ID.Value;
            account.Save(dc);

            Assert.True(activity.CopyDefaultForm(user.ID.Value, dc).HasNoErrors());
        }

        [Fact]
        public void AddFormAndActivateTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);

            Assert.NotNull(activity.AddFormAndActivitate(form, user.ID.Value, false, msgs, dc));
        }

        [Fact]
        public void HasAdressTest()
        {
            var a = new Activity() { };
            a.Save(dc: dc);

            Assert.False(a.HasAddress);
        }

        [Fact]
        public void EditExistingTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc: dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Account() { };
            a.Save(dc: dc);
            var aid = a.ID.Value;

            var act = new Activity() { };
            a.Save(dc: dc);
            var actid = act.ID.ToString();

            var nv = new System.Collections.Specialized.NameValueCollection() { };

            Assert.NotNull(Activity.EditExisting(loggedInUserId, a, actid, nv, msgs));
        }

        [Fact]
        public void CreateNewTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc: dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Account() { };
            a.Save(dc: dc);
            var aid = a.ID.Value;

            var act = new Activity() { };
            a.Save(dc: dc);
            var actid = act.ID.ToString();

            var nv = new System.Collections.Specialized.NameValueCollection() { };

            Assert.NotNull(Activity.CreateNew(loggedInUserId, a, nv, msgs, dc: dc));
        }

        [Fact]
        public void AddNewSheduleTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            activity.Save(dc);

            var nv = new System.Collections.Specialized.NameValueCollection() { };

            var start = new DateTime(2017, 06, 06);
            var end = new DateTime(2017, 10, 10);
            var startTime = new DateTime(2018, 1, 1, 1, 1, 1);
            var endtime = new DateTime(2018, 3, 3, 3, 3, 3);

            var x = activity.AddNewSchedule("newShed", "det", true, start, end, startTime, endtime, "Monday", account.ID.Value, user.ID.Value, msgs, dc);

            Assert.NotNull(x);
        }

        [Fact]
        public void AddNewRegistrationPeriodTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            var start = new DateTime(2017, 06, 06);
            var end = new DateTime(2017, 10, 10);

            Assert.NotNull(activity.AddNewRegistrationPeriod("newreg", start, end, account.ID.Value, user.ID.Value, msgs, dc));
        }

        [Fact]
        public void AddTeamMemberTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var otherUser = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(loggedInUser, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.NotNull(activity.AddTeamMember(otherUser.ID.Value, true, loggedInUser.ID.Value, msgs, dc));
        }

        [Fact]
        public void RemoveGradeTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc);
            gradeLevel.ID = 12;
            gradeLevel.Save(dc);
            var grade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc);


            Assert.True(activity.RemoveGrade(gradeLevel, dc).HasNoErrors());
        }

        [Fact]
        public void AddGradeTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc);
            gradeLevel.ID = 12;
            gradeLevel.Save(dc);
            var grade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc);

            Assert.NotNull(activity.AddGrade(gradeLevel.ID.Value, user.ID.Value, msgs, dc));
        }


        [Fact]
        public void MarkAsDeletedTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.True(activity.MarkAsDeleted(user.ID.Value, dc).HasNoErrors());
        }

        [Fact]
        public void MoveToArchiveTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.True(activity.MoveToArchive(user.ID.Value, dc).HasNoErrors());
        }

        [Fact]
        public void MoveFromArchiveTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.True(activity.MoveFromArchive(user.ID.Value, dc).HasNoErrors());
        }


        [Fact]
        public void GetActivityGroupTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, activityGroup, msgs, dc);

            Assert.NotNull(activity.GetActivityGroup(dc));
        }


        [Fact]
        public void AddOrUpdateActivityPropertyTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);

            Assert.NotNull(activity.AddOrUpdateActivityProperty(AccountPropertyType.LandingPageText, "value", user.ID.Value, msgs, dc));
        }

        [Fact]
        public void HasSheduleTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activityShedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, activityShedule, msgs, dc);

            Assert.True(activity.HasSchedule);
        }

        [Fact]
        public void GetSheduleTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            Assert.NotNull(activity.GetSchedule(false, dc ));
        }

        [Fact]
        public void GetActivityPropertyTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            var accountProperty = Factories.AccountPropertyFactory.Create(account, AccountPropertyType.ApplicationThankYouEmail, msgs, dc);
            accountProperty.ActivityId = activity.ID.Value;
            accountProperty.Save(dc);


            Assert.NotNull(activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, dc));
        }

        [Fact]
        public void AddMessageTest()
        {

            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);

            List<int> iList = new List<int>();
                iList.Add(registrant.ID.Value);
                iList.Add(2);
                iList.Add(3);

            Assert.NotNull(activity.AddMessage("subj", "Hello Class", iList, user.ID.Value, msgs, dc));
        }

        [Fact]
        public void GetActivitiesByRegistrantByDayTest()
        {
            var date = new DateTime(2018, 6, 11);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(account, msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            activityInstance.StartDateTime = date;
            activityInstance.Save(dc);
            registrantActivity.NumberOfSeats = 1;
            registrantActivity.Save(dc);
            schedule.AccountId = account.ID.Value;
            schedule.StartDate = date;
            schedule.Save(dc);

            Assert.Single(Activity.GetActivitiesByRegistrantByDay(registrant.ID.Value, DayOfWeekASL.NotSet, schedule.StartDate.Value, dc));
        }

        [Fact]
        public void GetByActivitiesByRegistrantsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);

            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var activityShedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            registrantActivity.NumberOfSeats = 1;
            List<int> regList = new List<int>();
            regList.Add(registrant.ID.Value);

            List<int> actList = new List<int>();
            regList.Add(activity.ID.Value);

            Assert.Single(Activity.GetByActivitiesByRegistrants(actList,regList,0,RegistrationStatus.Current,null,dc));
        }

        [Fact]
        public void GetAllowedGendersTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            activity.GenderRestriction = GenderRestrictionType.None;

            Assert.Equal(2,activity.GetAllowedGenders().Count);
        }

        [Fact]
        public void IsInSessionTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            activity.ActivityScheduleId = schedule.ID.Value;
            activity.Save(dc);

            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            activityInstance.StartDateTime = new DateTime(1, 1, 1);
            activityInstance.StartDateTime = new DateTime(2020, 1, 1);
            activityInstance.ActivityScheduleId = schedule.ID.Value;
            activityInstance.Save(dc);

            Assert.True(activity.IsInSession());
        }

        [Fact]
        public void WillBeInSessionTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            var start = new DateTime(2018, 10, 10, 10, 10, 10) { };
            schedule.StartDate = start;
            msgs.AddRange(schedule.Save(dc));

            activity.StartDateTime = start;
            activityInstance.StartDateTime = start;
            msgs.AddRange(activityInstance.Save(dc));

            Assert.True(activity.WillBeInSession());
        }

        [Fact]
        public void IsOutOfSessionTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            

            Assert.True(activity.IsOutOfSession(dc) && 1 == 0);
        }

        [Fact]
        public void GetFirstDateTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);

            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            Assert.NotNull(activity.GetFirstDate(dc));
        }

        [Fact]
        public void GetLastDateTest()
        {
            var start = new DateTime(1, 1, 1, 1, 1, 1) { };
            var end = new DateTime(2, 2, 2, 2, 2, 2) { };


            var act = new Activity() { StartDateTime = start, EndDateTime = end };
            act.Save(dc: dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc: dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };

            Assert.NotNull(act.GetLastDate(dc: dc));
        }

        [Fact]
        public void SendRegistrationSummaryReportsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.NotNull(Activity.SendRegistrationSummaryReport(activity.ID.Value,"test",dc).HasNoErrors());
        }
    }
}
