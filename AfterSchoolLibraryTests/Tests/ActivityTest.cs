using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchool.Models;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityTest : BaseTest
    {
        public ActivityTest(ITestOutputHelper output) : base(output)
        {
        }

        /// <summary>
        /// loads an Activity from the database, in order to properly create an activity
        /// it is dependant on having an account ID and a number of seats set, in the
        /// base method the automatic number of seats avaible is 4
        /// </summary>
        [Fact]
        public void LoadTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            Assert.NotNull(Activity.Load(activity.ID.Value, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);

            Assert.True(activity.Save(dc: dc).HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            activity.Delete(dc: dc);
            Assert.Null(Activity.Load(activity.ID.Value, dc: dc));
        }

        [Fact]
        public void ReloadTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            Assert.NotNull(activity.Reload(dc: dc));
        }

        [Fact]
        public void LoadByIdByAccountIdTest()
        {
            // Arrange
            var user = WebUserFactory.Create(msgs: msgs, dc: dc);
            var account = AccountFactory.Create(user: user, msgs: msgs, dc: dc);
            var activity = ActivityFactory.Create(account: account, msgs: msgs, dc: dc);

            // Act
            var expected = Activity.LoadByIdByAccountId(Id: activity.ID.Value, AccountId: account.ID.Value, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: activity.ID);
            Assert.Equal(expected: expected.Uid, actual: activity.Uid);
            Assert.Equal(expected: expected.Name, actual: activity.Name);
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            ActivityFactory.Create(account, msgs, dc: dc);
            Assert.Single(Activity.LoadByAccountId(account.ID.Value, dc: dc));
        }

        [Fact]
        public void LoadByAccountIdByIsPublishedTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            activity.IsPublished = true;
            activity.Save(dc: dc);
            Assert.Single(Activity.LoadByAccountIdByIsPublished(account.ID.Value, true, dc: dc));
        }

        [Fact]
        public void LoadByLocationIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var location = LocationFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, location, msgs, dc: dc);
            Assert.Single(Activity.LoadByLocationId(activity.LocationId.Value, dc: dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);

            Assert.NotNull(Activity.LoadByUid(activity.Uid, dc: dc));
        }

        [Fact]
        public void LoadByActivityGroupIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc: dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            activity.Save(dc: dc);
            Assert.Single(Activity.LoadByActivityGroupId(activity.ActivityGroupId.Value, dc: dc));
        }

        [Fact]
        public void LoadByActivityScheduleIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            activity.ActivityScheduleId = 12;
            activity.Save(dc: dc);
            Assert.Single(Activity.LoadByActivityScheduleIdByAccountId(activity.ActivityScheduleId.Value, account.ID.Value, dc: dc));
        }

        [Fact]
        public void LoadByUniqueNameByAccountIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            activity.UniqueName = "test";
            activity.Save(dc: dc);
            Assert.NotNull(Activity.LoadByUniqueNameByAccountId(activity.UniqueName, account.ID.Value, dc: dc));
        }

        [Fact]
        public void LoadListTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);

            List<int> list = new List<int>();
            list.Add(activity.ID.Value);


            Assert.Single(Activity.LoadList(list, dc: dc));
        }
        // One account can set up multiple activities
        // *the only required fields are the accounts ID and the DB connection,
        // you can filter the results using the additional fields*
        /// <summary>
        /// Should get the number of Activities under one Account *you can filter the results using the additional fields*
        /// </summary>
        [Fact]
        public void GetCountByAccountIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc: dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            msgs.AddRange(activity.Save(dc));
            Assert.Equal(1, Activity.GetCountByAccountId(account.ID.Value, "", activity.ActivityGroupId.Value, null, null, null, null, msgs, dc));
        }

        [Fact]
        public void LoadByAccountIdLongTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            ActivityFactory.Create(account, msgs, dc: dc);
            Assert.Single(Activity.LoadByAccountId(account.ID.Value, null, null, null, false, false, null, 0, 100,"", msgs, dc));
        }

        [Fact]
        public void SetIsPublishedTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            activity.SetIsPublished(true, account.ID.Value, dc: dc);
            Assert.True(activity.IsPublished);
        }

        [Fact]
        public void GetDaysOfTheWeekTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc: dc);

            var start = DateTime.Now.AddDays(1);
            var end = DateTime.Now.AddDays(3);

            schedule.StartDate = start;
            schedule.EndDate = end;

            var activityShedule = ActivityScheduleFactory.Create(account, msgs, dc: dc);
            activity.ActivityScheduleId = activityShedule.ID.Value;
            var activityInstance = ActivityInstanceFactory.Create(activity, activityShedule, msgs, dc: dc);
            activityInstance.StartDateTime = start;
            var d = new ActivityDay { ActivityId = activity.ID.Value };
            d.Save(dc: dc);
            d.ActivityId = activity.ID.Value;

            Assert.Single(activity.GetDaysOfTheWeek(dc: dc));
        }

        [Fact]
        public void GetRegistrantsTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(msgs, dc: dc);
            RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            Assert.Single(activity.GetRegistrants(msgs, dc));
        }

        [Fact]
        public void GetRegistrantsActivitiesLongTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(msgs, dc: dc);
            RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.Single(activity.GetRegistrantActivities(true, dc: dc));
        }

        [Fact]
        public void GetRegistrantsCountTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, 12, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.Equal(1, activity.GetRegistrantCount(msgs, dc));
        }

        [Theory]
        [InlineData(RegistrationAssignmentType.NotSet)]
        [InlineData(RegistrationAssignmentType.Invited)]
        public void GetRegistrantCountByApplicationTypeTest(RegistrationAssignmentType registrationAssignmentType)
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(msgs, dc: dc);
            RegistrantActivityFactory.Create(activity, registrant, registrationAssignmentType, msgs, dc: dc);

            Assert.Equal(1, activity.GetRegistrantCountByAssignmentType(registrationAssignmentType, dc: dc));
        }

        [Fact]
        public void isFullTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(msgs, dc: dc);
            RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.False(activity.IsFull(dc: dc));
        }

        [Fact]
        public void GetTeamMembersTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var user = WebUserFactory.Create(msgs, dc: dc);
            var activityWebUser = ActivityWebUserFactory.Create(activity, user, msgs, dc: dc);
            activity.GetTeamMembers(dc: dc).ElementAt(0);

            Assert.Equal(activityWebUser.ToString(), activity.GetTeamMembers(dc: dc).ElementAt(0).ToString());
        }

        [Fact]
        public void GetFormsTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);

            Assert.Single(activity.GetForms(dc: dc));
        }

        [Fact]
        public void GetActiveFormTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            var activityForm = ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            activityForm.IsActive = true;
            msgs.AddRange(activityForm.Save(dc));

            Assert.NotNull(activity.GetActiveForm(msgs, dc));
        }

        [Fact]
        public void GetActivityFormsTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);

            Assert.Single(activity.GetActivityForms(dc: dc));
        }

        [Fact]
        public void GetAttendanceTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var attendance = new Attendance { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);

            Assert.Single(activity.GetAttendance(dc: dc));
        }

        [Fact]
        public void GetCompletedAttendanceTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.IsCompleted = true;
            attendance.Save(dc: dc);

            Assert.Single(activity.GetCompletedAttendance( dc: dc));
        }

        [Theory]
        [InlineData("Date", OrderByDirectionEnum.Descending, 0, 10)]
        public void GetCompletedAttendanceWithPagingTest(string orderByParam, OrderByDirectionEnum direction, int pageNumber, int recordsPerPage)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.IsCompleted = true;
            attendance.Save(dc: dc);

            Assert.Single(activity.GetCompletedAttendanceWithPaging(orderByParam, direction, pageNumber, recordsPerPage, msgs, dc: dc));
        }

        [Fact]
        public void GetAttendancePresentCountTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.IsCompleted = true;
            msgs.AddRange(attendance.Save(dc: dc));

            var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            RegistrantActivityFactory.Create(activity, registrant, RegistrationAssignmentType.Reserved, msgs, dc);
            var attendanceRegistrant = new AttendanceRegistrant { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            msgs.AddRange(attendanceRegistrant.Save(dc: dc));

            var presentCount = activity.GetAttendancePresentCount(msgs, dc: dc);
            Assert.Equal(1, presentCount);
        }

        [Fact]
        public void GetAttendancePossibleCountTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.IsCompleted = true;
            msgs.AddRange(attendance.Save(dc: dc));

            var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            RegistrantActivityFactory.Create(activity, registrant, RegistrationAssignmentType.Reserved, msgs, dc);
            var attendanceRegistrant = new AttendanceRegistrant { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            msgs.AddRange(attendanceRegistrant.Save(dc: dc));

            Assert.Equal(1, activity.GetAttendancePossibleCount(dc: dc));
        }

        [Fact]
        public void GetAttendanceByDateTest()
        {
            var date = new DateTime(2018, 10, 10);
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc: dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);
            attendance.Date = date;
            attendance.Save(dc: dc);


            activity.GetAttendanceByDate(date, dc: dc);
            Assert.Single(activity.GetAttendanceByDate(date, dc: dc));
        }

        [Fact]
        public void GetActivityRegistrationPeriodsTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc: dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetActivityRegistrationPeriods(dc: dc));
        }

        [Fact]
        public void GetRegistrationPeriodsTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc: dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetRegistrationPeriods(msgs, dc));
        }

        [Fact]
        public void GetUpcomingRegistrationPeriodsTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);
            registrationPeriod.StartDateTime = DateTime.UtcNow.AddDays(1);
            registrationPeriod.EndDateTime = DateTime.UtcNow.AddDays(2);
            msgs.AddRange(registrationPeriod.Save(dc));

            Assert.Single(activity.GetUpcomingRegistrationPeriods(msgs, dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTestPast()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-2), DateTime.UtcNow.AddWeeks(-1), msgs, dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Null(activity.GetCurrentRegistrationPeriod(msgs, dc: dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTestPresent()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-2), DateTime.UtcNow.AddWeeks(2), msgs, dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.NotNull(activity.GetCurrentRegistrationPeriod(msgs, dc: dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTestFuture()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(1), DateTime.UtcNow.AddWeeks(2), msgs, dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Null(activity.GetCurrentRegistrationPeriod(msgs, dc: dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTestPast()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-2), DateTime.UtcNow.AddWeeks(-1), msgs, dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.Null(activity.GetCurrentOrNextRegistrationPeriod(dc: dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTestPresent()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-1), DateTime.UtcNow.AddWeeks(1), msgs, dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.NotNull(activity.GetCurrentOrNextRegistrationPeriod(msgs, dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTestFuture()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(1), DateTime.UtcNow.AddWeeks(2), msgs, dc);
            ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.NotNull(activity.GetCurrentOrNextRegistrationPeriod(dc: dc));
        }

        [Fact]
        public void GetActiveRegistrationPeriodTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc: dc);
            var activityRegistrationPeriod = ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);
            activityRegistrationPeriod.IsActive = true;
            activityRegistrationPeriod.Save(dc: dc);

            Assert.NotNull(activity.GetActiveRegistrationPeriod(dc: dc));
        }

        // returns the account that started the
        [Fact]
        public void GetAccountTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);

            Assert.True(activity.GetAccount(msgs, dc).IsNotEmpty() && activity.GetAccount(msgs, dc).ID == account.ID);
        }

        [Fact]
        public void CopyTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);

            account.DefaultFormId = form.ID;
            msgs.AddRange(account.Save(dc));

            Assert.NotNull(activity.Copy("newActivity", false, accountOwner.ID.Value, msgs, dc: dc));
        }

        [Fact]
        public void CopyWithoutScheduleTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            account.DefaultFormId = form.ID;
            msgs.AddRange(account.Save(dc));

            // Act
            var copiedActivity = activity.CopyWithoutSchedule("newActivity", false, accountOwner.ID.Value, msgs, dc: dc);

            // Assert
            Assert.NotNull(copiedActivity);
            Assert.Null(copiedActivity.ActivityScheduleId);
        }

        [Fact]
        public void AddFormAndActivateTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var form = FormFactory.Create(account, msgs, dc);

            Assert.NotNull(activity.AddFormAndActivate(form, accountOwner.ID.Value, false, msgs, dc));
        }

        [Fact]
        public void AddNewScheduleTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, DateTime.Now, DateTime.Now.AddDays(10), msgs, dc);
            ActivityFactory.Create(account, schedule, 100, msgs, dc: dc);

            Assert.NotNull(schedule);
        }

        [Fact]
        public void AddNewRegistrationPeriodTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            var start = new DateTime(2017, 06, 06);
            var end = new DateTime(2017, 10, 10);

            Assert.NotNull(activity.AddNewRegistrationPeriod("newreg", start, end, account.ID.Value, user.ID.Value, msgs, dc));
        }

        [Fact]
        public void AddTeamMemberTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var otherUser = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(loggedInUser, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            Assert.NotNull(activity.AddTeamMember(otherUser.ID.Value, true, loggedInUser.ID.Value, msgs, dc));
        }

        [Fact]
        public void MarkAsDeletedTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            Assert.True(activity.MarkAsDeleted(user.ID.Value, dc).HasNoErrors());
        }

        [Fact]
        public void MoveToArchiveTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            Assert.True(activity.MoveToArchive(user.ID.Value, dc).HasNoErrors());
        }

        [Fact]
        public void MoveFromArchiveTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            Assert.True(activity.MoveFromArchive(user.ID.Value, dc).HasNoErrors());
        }


        [Fact]
        public void GetActivityGroupTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var activity = ActivityFactory.Create(account, activityGroup, msgs, dc);

            Assert.NotNull(activity.GetActivityGroup(msgs, dc));
        }


        [Fact]
        public void AddOrUpdateActivityPropertyTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            Assert.NotNull(activity.AddOrUpdateActivityProperty(AccountPropertyType.LandingPageText, "value", user.ID.Value, msgs, dc));
        }

        [Fact]
        public void HasScheduleTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activitySchedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, activitySchedule, msgs, dc);

            Assert.True(activity.HasSchedule);
        }

        [Fact]
        public void GetScheduleTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);
            ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            Assert.NotNull(activity.GetSchedule(msgs, dc));
        }

        [Fact]
        public void GetActivityPropertyTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var accountProperty = AccountPropertyFactory.Create(account, AccountPropertyType.ApplicationThankYouEmail, msgs, dc);
            accountProperty.ActivityId = activity.ID.Value;
            accountProperty.Save(dc);


            Assert.NotNull(activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc));
        }

        [Fact]
        public void AddMessageTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);

            var registrantIds = new List<int> { registrant.ID.Value };

            Assert.NotNull(activity.AddMessage("subj", "Hello Class", registrantIds, false, false, accountOwner.ID.Value, msgs, dc));
        }

        [Fact]
        public void GetAllowedGendersTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            FormFactory.Create(account, msgs, dc);
            activity.GenderRestriction = GenderRestrictionType.None;

            Assert.Equal(2,activity.GetAllowedGenders().Count);
        }
        /// <summary>
        /// Tests that an Activity is currently in session
        /// </summary>
        [Fact]
        public void IsInSessionTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);

            ActivityInstanceFactory.Create(
                activity,
                schedule,
                DateTime.UtcNow.AddWeeks(-1),
                DateTime.UtcNow.AddWeeks(1),
                msgs,
                dc);

            Assert.True(activity.IsInSession(msgs, dc));
        }
        /// <summary>
        ///  Tests that an activity will be in session
        /// </summary>
        [Fact]
        public void WillBeInSessionTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            var start = new DateTime(2118, 10, 10, 10, 10, 10);
            schedule.StartDate = start;
            msgs.AddRange(schedule.Save(dc));

            activityInstance.StartDateTime = start;
            msgs.AddRange(activityInstance.Save(dc));

            Assert.True(activity.WillBeInSession(msgs, dc));
        }
        /// <summary>
        /// Tests that an activity is no longer session
        /// </summary>
        [Fact]
        public void IsOutOfSessionTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);
            ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            Assert.True(activity.IsOutOfSession(msgs, dc));
        }

        [Fact]
        public void GetFirstDateTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);

            ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            Assert.NotNull(activity.GetFirstDate(msgs, dc));
        }

        [Fact]
        public void GetLastDateTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);

            var instances = new List<ActivityInstance>();

            for (var i = 0; i < 3; i++)
                instances.Add(ActivityInstanceFactory.Create(
                    activity,
                    schedule,
                    DateTime.UtcNow.AddDays(i),
                    DateTime.UtcNow.AddDays(i).AddHours(2),
                    msgs,
                    dc));

            // if you don't reload from the database the micro-seconds are not the same on the start date time object
            // which may need to be figured out later.
            // Otherwise it works and gets the actual last date

            Assert.Equal(instances.Last().StartDateTime.ToShortDateTimeString(), activity.GetLastDate(msgs, dc).ToShortDateTimeString());
        }

        [Fact]
        public void SendRegistrationSummaryReportsTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var form = account.GetOrCreateDefaultStudentForm(user, msgs, dc);
            var questions = form.GetActiveQuestions(msgs, dc);
            var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(account, registrant, RegistrationCompletionStatus.Completed, msgs, dc);
            RegistrantActivityFactory.Create(activity, registrant, registration, msgs, dc: dc);
            FormQuestionAnswerFactory.Create(formQuestionId: questions[0].ID.Value,
                activityId: activity.ID.Value,
                registrantId: registrant.ID.Value,
                registrationId: registration.ID.Value,
                questionText: questions[0].Text,
                answerText: "This is answer.",
                questionDataTypeId: questions[0].QuestionDataTypeId,
                msgs: msgs,
                dc: dc);

            Assert.True(ActivityService.SendRegistrationSummaryReport(activity.ID.Value, "test", false, dc).HasNoErrors());
        }

        [Fact]
        public void IsPublicallyViewableTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            activity.Publish(user, msgs, dc);

            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());

        }

        [Fact]
        public void IsPubliclyViewableFailingTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(user.ID.Value, dc));
            activity.Publish(user, msgs, dc);

            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());

        }

        [Fact]
        public void ActivitySearchCountTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activityName = "Activity_" + DateTime.Now.Ticks;
            var activity = ActivityFactory.Create(account, activityName, msgs, dc);

            var getActivityCount = account.GetActivityCount(
                    UserType.TechSupport,
                    user,
                    activityName,
                    ActivityType.NotSet,
                    false,
                    false,
                    false,
                    msgs,
                    dc);

            Assert.Equal(1, getActivityCount);
        }

        [Fact]
        public void ActivitySearchCountWithCustomTypeAndNameTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activityName = "TestCountActivity_" + DateTime.Now.Ticks;
            var activityNameSecond = "TestCountActivity_" + DateTime.Now.Ticks;
            var activity = ActivityFactory.CreateWithNameAndType(account, activityName, ActivityType.Activity, msgs, dc);
            var activitySecond = ActivityFactory.CreateWithNameAndType(account, activityNameSecond, ActivityType.Activity, msgs, dc);

            var getActivityCount = account.GetActivityCount(
                    UserType.TechSupport,
                    user,
                    "TestCountActivity_", //search term
                    ActivityType.Activity,
                    false,
                    false,
                    false,
                    msgs,
                    dc);

            Assert.Equal(2, getActivityCount);
        }


        [Fact]
        public void CheckActivityPaymentErrorTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);

            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var activity = ActivityFactory.Create(schoolAccount, location, 12, msgs, dc);

            activity.ProviderAccountId = account.ID.Value;
            msgs.AddRange(activity.Save(dc));

            var errorMsgs = activity.IsPaymentMethodAddedForExternalAccount(account, msgs, dc);

            Assert.Equal(1, errorMsgs.Count);
        }

        [Fact]
        public void InstructorActivityAccessTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(user, msgs, dc);
            var userType = AccountWebUserFactory.Create(account, user, UserType.Instructor, true, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var activityWebUser = ActivityWebUserFactory.Create(activity, user, msgs, dc);

            var isAllowed = false;

            if (user.HasAccessToActivity(activity, msgs, dc))
                isAllowed = true;

            Assert.True(isAllowed);
        }

        [Fact]
        public void ActivityCheckLogOut()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            ActivityWebUserFactory.Create(activity, user, msgs, dc);

            Assert.True(!activity.IsPublished && user.HasAccessToActivity(activity, msgs, dc));
        }

        [Fact]
        public void ActivityCheckLogOutFailedCase1()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            activity.IsPublished = true;
            activity.Save(dc);
            ActivityWebUserFactory.Create(activity, user, msgs, dc);

            Assert.False(!activity.IsPublished && user.HasAccessToActivity(activity, msgs, dc));
        }

        [Fact]
        public void CreateActivityWithCE()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.CreateWithCE(account, true, msgs, dc);
            activity.Publish(user, msgs, dc);

            Assert.True(activity.IsPublished);
            Assert.True(activity.IsPubliclyViewable);
        }

        [Fact]
        public void UpdateActivityWithCETest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);
            ActivityFactory.UpdateCE(account, activity, true, msgs,dc);
            activity.Publish(user, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.True(activity.IsPublished);
            Assert.True(activity.IsPubliclyViewable);
        }
    }
}
