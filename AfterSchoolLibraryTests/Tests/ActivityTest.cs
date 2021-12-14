using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary.DataAccess;

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
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.NotNull(Activity.Load(activity.ID.Value, dc: dc));
        }
        /// <summary>
        /// Gets the number of Activiites in the DB
        /// </summary>
        [Fact]
        public void GetCountTest()
        {
            var startCount = Activity.GetCount(msgs: msgs, dc: dc);
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.Equal(startCount + 1, Activity.GetCount(msgs: msgs, dc: dc));
        }
        /// <summary>
        /// Saves the Account in the DB, returns messages that will send errors if this is not recorded 
        /// in the DB properly
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);

            Assert.True(activity.Save(dc: dc).HasNoErrors());
        }
        /// <summary>
        /// Removes the Activity from the DB, it should not be able to be loaded from the DB afterwards
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            activity.Delete(dc: dc);
            Assert.Null(Activity.Load(activity.ID.Value, dc: dc));
        }        

        /// <summary>
        /// Loads the activity from the DB (using it's ID) and returns it.
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            Assert.NotNull(activity.Reload(dc: dc));
        }

        [Fact]
        public void LoadByIdByAccountIdTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }            
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
            var location = LocationFactory.Create(account, msgs, dc);
            var activity = ActivityFactory.Create(account, location, msgs, dc: dc);
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
        /// <summary>
        /// activity groups are collections of activities, this methos will load a list of 
        /// the activities in a group from the DB
        /// </summary>
        [Fact]
        public void LoadByActivityGroupIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc: dc);
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
            Assert.Single(Activity.LoadByActivityScheduleIdByAccountId(activity.ActivityScheduleId.Value, account.ID.Value, dc: dc));
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
        /// <summary>
        /// Loads using a List of Activity Ids
        /// </summary>
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
        // One account can set up multiple activities 
        // *the only required fields are the accounts ID and the DB conection,
        // you can filter the results using the additional feilds*
        /// <summary>
        /// Should get the number of Activitiess under one Account *you can filter the results using the additional feilds*
        /// </summary>
        [Fact]
        public void GetCountByAccountIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc: dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            msgs.AddRange(activity.Save(dc));
            Assert.Equal(1, Activity.GetCountByAccountId(account.ID.Value, activity.ActivityGroupId.Value, null, null, null, null, msgs, dc));
        }

        /// <summary>
        /// loads the Activities under one Account and should return a list of one Account
        /// </summary>
        [Fact]
        public void LoadByAccountIdLongTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            Assert.Single(Activity.LoadByAccountId(account.ID.Value, null, null, null, false, false, null, 0, 100, msgs, dc));
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
        
        /// <summary>
        /// Gets the days of the week that the activity repeats on weekly
        /// </summary>
        [Fact(Skip = "Need to update")]
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
        
        /// <summary>
        /// Returns a list of the registrants that have "signed up" for the activity
        /// </summary>
        [Fact]
        public void GetRegistrantsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            Assert.Single(activity.GetRegistrants(msgs, dc));
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

        /// <summary>
        ///  Returns a list of all of the RegistrantActivities associated with the activity
        /// </summary>
        [Fact]
        public void GetRegistrantsCountTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, 12, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
         
            Assert.Equal(1, activity.GetRegistrantCount(msgs, dc));
        }

        // will count the number of registrants for the particular activity of a given registrantion type
        [Fact(Skip = "Need to fix")]
        public void GetRegistrantCountByApplicationTypeTest()
        {
            //var user = Factories.WebUserFactory.Create(msgs, dc);
            //var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            //var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            //var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            //var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            //var n = new RegistrantAssignment() { RegistrantId = registrant.ID.Value };
            //var debug = activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Invited, dc: dc);
            //Assert.Equal(1, activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Invited, dc: dc));
        }

        /// <summary>
        /// returns if an activity is full or not (checks the SeatsAvaible feild)
        /// </summary>

        //TODO weird error with DeBugging
        [Fact]
        public void isFullTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.False(activity.IsFull(dc: dc));
        }

        /// <summary>
        /// returns boolean (if Waitinglistseats = 10 and registration type)
        /// </summary>
        //TODO add registrant to waitlist
        [Fact]
        public void isWaitingListFullTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, 5, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            Assert.True(activity.IsWaitingListFull(dc: dc)); //0 open with 0 on waiting list means full
        }
  
        /// <summary>
        /// Returns a list of all WebUSers signed up for the same activityID
        /// </summary>
        [Fact]
        public void GetTeamMembersTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var activityWebUser = Factories.ActivityWebUserFactory.Create(activity, user, msgs, dc: dc);
            var debug = activity.GetTeamMembers(dc: dc).ElementAt(0);

            Assert.Equal(activityWebUser.ToString(), activity.GetTeamMembers(dc: dc).ElementAt(0).ToString());
        }

        /// <summary>
        /// returns a list of all forms ascociated with that activity
        /// </summary>
        [Fact]
        public void GetFormsTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);

            Assert.Single(activity.GetForms(dc: dc));
        }


        /// <summary>
        /// Same as above, but they must also be active (not to be confused with a simple activity form)
        /// </summary>
        [Fact]
        public void GetActiveFormTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            activityForm.IsActive = true;
            msgs.AddRange(activityForm.Save(dc));

            Assert.NotNull(activity.GetActiveForm(msgs, dc));
        }

        /// <summary>
        /// Returns a list of the activity forms
        /// </summary>
        [Fact]
        public void GetActivityFormsTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);

            Assert.Single(activity.GetActivityForms(dc: dc));
        }

        /// <summary>
        /// Returns a list of the Attendance objects asociated with this activity.
        /// </summary>
        [Fact]
        public void GetAttendanceTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);

            Assert.Single(activity.GetAttendance(dc: dc));
        }

        /// <summary>
        /// Returns a list of the Attendance objects asociated with this activity that are completed.
        /// </summary>
        [Fact]
        public void GetCompletedAttendanceTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.IsCompleted = true;
            attendance.Save(dc: dc);

            Assert.Single(activity.GetCompletedAttendance( dc: dc));
        }

        [Theory]
        [InlineData("date","1",1,10)]
        public void GetCompletedAttendanceWithPagingTest(string orderByParam,string direction,int pageNumber,int recordsPerPage)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.IsCompleted = true;
            attendance.Save(dc: dc);

            Assert.Single(activity.GetCompletedAttendanceWithPaging(orderByParam, (OrderByDirectionEnum)Enum.Parse(typeof(OrderByDirectionEnum), direction), pageNumber, recordsPerPage, msgs, dc: dc));
        }
        /// <summary>
        /// returns the numberof attendance objects asociated with the activity that have been marked as present
        /// </summary>
        [Fact(Skip = "Need to update")]
        public void GetAttendancePresentCountTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = Factories.RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);

            Assert.Equal(1, activity.GetAttendancePresentCount(dc: dc));
        }
        
        [Fact(Skip = "Need to update")]
        public void GetAttendancePossibleCountTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = Factories.RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);

            Assert.Equal(1, activity.GetAttendancePossibleCount(dc: dc));
        }

        [Fact]
        public void GetAttendanceByDateTest()
        {
            var date = new DateTime(2018, 10, 10) { };
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc: dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = Factories.RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);
            attendance.Date = date;
            attendance.Save(dc: dc);


            var count = activity.GetAttendanceByDate(date, dc: dc);

            Assert.Single(activity.GetAttendanceByDate(date, dc: dc));
        }

        /// <summary>
        /// returns a list of activity Registration Periods
        /// </summary>
        [Fact]
        public void GetActivityRegistrationPeriodsTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(account, msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetActivityRegistrationPeriods(dc: dc));
        }
        [Fact]
        public void GetRegistrationPeriodsTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(account, msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetRegistrationPeriods(msgs, dc));
        }

        // TODO HELP
        [Fact]
        public void GetUpcomingRegistrationPeriodsTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var form = FormFactory.Create(account, msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc);
            var activityRegistrationPeriod = ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);
            registrationPeriod.StartDateTime = DateTime.UtcNow.AddDays(1);
            registrationPeriod.EndDateTime = DateTime.UtcNow.AddDays(2);
            msgs.AddRange(registrationPeriod.Save(dc));

            Assert.Single(activity.GetUpcomingRegistrationPeriods(msgs, dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTestPast()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-2), DateTime.UtcNow.AddWeeks(-1), msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Null(activity.GetCurrentRegistrationPeriod(msgs, dc: dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTestPresent()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-2), DateTime.UtcNow.AddWeeks(2), msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.NotNull(activity.GetCurrentRegistrationPeriod(msgs, dc: dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTestFuture()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(1), DateTime.UtcNow.AddWeeks(2), msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Null(activity.GetCurrentRegistrationPeriod(msgs, dc: dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTestPast()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-2), DateTime.UtcNow.AddWeeks(-1), msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.Null(activity.GetCurrentOrNextRegistrationPeriod(dc: dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTestPresent()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var form = FormFactory.Create(account, msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(-1), DateTime.UtcNow.AddWeeks(1), msgs, dc);
            var activityRegistrationPeriod = ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.NotNull(activity.GetCurrentOrNextRegistrationPeriod(msgs, dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTestFuture()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddWeeks(1), DateTime.UtcNow.AddWeeks(2), msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.NotNull(activity.GetCurrentOrNextRegistrationPeriod(dc: dc));
        }

        [Fact]
        public void GetActiveRegistrationPeriodTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(account, msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);
            activityRegistrationPeriod.IsActive = true;
            activityRegistrationPeriod.Save(dc: dc);

            Assert.NotNull(activity.GetActiveRegistrationPeriod(dc: dc));
        }
        // returns the account that started the 
        [Fact]
        public void GetAccountTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);

            Assert.True(activity.GetAccount(msgs, dc).IsNotEmpty() && activity.GetAccount(msgs, dc).ID == account.ID);
        }

        [Fact]
        public void CopyTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);

            Assert.NotNull(activity.Copy("newActivity", false, accountOwner.ID.Value, msgs, dc: dc));
        }

        [Fact]
        public void CopyDefaultFormTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            account.DefaultFormId = form.ID.Value;
            account.Save(dc);

            Assert.True(activity.CopyDefaultForm(accountOwner.ID.Value, dc).HasNoErrors());
        }

        [Fact]
        public void AddFormAndActivateTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);

            Assert.NotNull(activity.AddFormAndActivitate(form, accountOwner.ID.Value, false, msgs, dc));
        }

        [Fact(Skip = "Need to add this test in the future")]
        public void EditExistingTest()
        {
            
        }

        [Fact(Skip = "Need to add this test in the future")]
        public void CreateNewTest()
        {
            
        }

        [Fact(Skip = "Moved to ActivityScheduleService, need to rewrite test")]
        public void AddNewSheduleTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, activityGroup, msgs, dc);

            var start = new DateTime(2017, 06, 06);
            var end = new DateTime(2017, 10, 10);
            var startTime = new DateTime(2018, 1, 1, 1, 1, 1);
            var endtime = new DateTime(2018, 3, 3, 3, 3, 3);

            //var schedule = activity.AddNewSchedule(
            //    "newShed", 
            //    "det", 
            //    true, 
            //    start, 
            //    end, 
            //    startTime, 
            //    endtime, 
            //    "1/1/2019-12:39 PM", 
            //    account.ID.Value, 
            //    accountOwner.ID.Value, 
            //    msgs, 
            //    dc);

            // Assert.NotNull(schedule);
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
            var activityGroup = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, activityGroup, msgs, dc);

            Assert.NotNull(activity.GetActivityGroup(msgs, dc));
        }


        [Fact]
        public void AddOrUpdateActivityPropertyTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

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

            Assert.NotNull(activity.GetSchedule(msgs, dc));
        }

        [Fact]
        public void GetActivityPropertyTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var accountProperty = Factories.AccountPropertyFactory.Create(account, AccountPropertyType.ApplicationThankYouEmail, msgs, dc);
            accountProperty.ActivityId = activity.ID.Value;
            accountProperty.Save(dc);


            Assert.NotNull(activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc));
        }

        [Fact]
        public void AddMessageTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);

            var registrantIds = new List<int>() { registrant.ID.Value };

            Assert.NotNull(activity.AddMessage("subj", "Hello Class", registrantIds, false, accountOwner.ID.Value, msgs, dc));
        }

        //[Fact]
        //public void GetActivitiesByRegistrantByDayTest()
        //{
        //    var date = new DateTime(2018, 6, 11);
        //    var user = Factories.WebUserFactory.Create(msgs, dc);
        //    var account = Factories.AccountFactory.Create(user, msgs, dc);
        //    var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
        //    var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
        //    var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, date, date.AddHours(1), msgs, dc);
        //    var registrant = Factories.RegistrantFactory.Create(account, msgs, dc);
        //    var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);                        

        //    Assert.Single(Activity.GetActivitiesByRegistrantByDay(registrant.ID.Value, DayOfWeekASL.NotSet, schedule.StartDate.Value, msgs, dc));
        //}

        //[Theory]
        //[InlineData(3)]
        //[InlineData(8)]
        //public void GetByActivitiesByRegistrantsTest(int numberToCreate)
        //{
        //    var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
        //    var user = Factories.WebUserFactory.Create(msgs, dc);
        //    var account = Factories.AccountFactory.Create(user, msgs, dc);            

        //    var registrantIds = new List<int>();
        //    var activityIds = new List<int>();

        //    for (var i = 0; i < numberToCreate; i++)
        //    {
        //        var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
        //        var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
        //        var instance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
        //        var registrant = Factories.RegistrantFactory.Create(msgs, dc);
        //        var registration = Factories.RegistrationFactory.Create(account, registrant, msgs, dc);

        //        var ra = registrant.AddOrUpdateParticipationObject(
        //            registrationId: registration.ID.Value,
        //            participationObjectId: activity.ID.Value,
        //            participationObjectType: ParticipationObjectType.Activity,
        //            assignmentType: RegistrationAssignmentType.Reserved,
        //            registrationPeriodId: null,
        //            activityGroupId: null,
        //            loggedInUserId: loggedInUser.ID.Value,
        //            msgs: msgs,
        //            dc: dc
        //            );

        //        registrantIds.Add(registrant.ID.Value);
        //        activityIds.Add(activity.ID.Value);
        //    }

        //    Assert.Equal(
        //        numberToCreate, 
        //        Activity.GetByActivitiesByRegistrants(
        //            activityIds: activityIds, 
        //            registrantIds: registrantIds, 
        //            dc: dc
        //        ).Count);
        //}

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
        /// <summary>
        /// Tests that an Activity is currently in session
        /// </summary>
        [Fact]
        public void IsInSessionTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);

            var activityInstance = Factories.ActivityInstanceFactory.Create(
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
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            var start = new DateTime(2118, 10, 10, 10, 10, 10) { };
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
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            

            Assert.True(activity.IsOutOfSession(msgs, dc));
        }

        [Fact]
        public void GetFirstDateTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);

            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            Assert.NotNull(activity.GetFirstDate(msgs, dc));
        }

        /// <summary>
        /// Tests to make sure that the Activity can properly get the last scheduled date
        /// </summary>
        [Fact]
        public void GetLastDateTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);

            var instances = new List<ActivityInstance>();

            for (var i = 0; i < 3; i++)
                instances.Add(Factories.ActivityInstanceFactory.Create(
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

        [Fact(Skip = "Need to figure out why it's not working")]
        public void SendRegistrationSummaryReportsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.True(ActivityService.SendRegistrationSummaryReport(activity.ID.Value,"test", dc).HasNoErrors());
        }

        [Fact]
        public void IsPublicallyViewableTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            activity.Publish(user, msgs, dc);

            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());

        }

        [Fact]
        public void IsPublicallyViewableFailingTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(user.ID.Value, dc));
            activity.Publish(user, msgs, dc);

            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());

        }

        [Fact]
        public void ActivityCheckLogOut()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var activityWebUser = ActivityWebUserFactory.Create(activity, user, msgs, dc); 
            var redirectCheck = false;


            if (!activity.IsPublished && user.IsNotEmpty() && user.HasAccessToActivity(activity, msgs, dc))
                redirectCheck = true;

            Assert.True(redirectCheck);
        }
    }
}
