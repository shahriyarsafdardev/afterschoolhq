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
        //Saves the Account in the DB, returns messages that will send errors if this is not recorded 
        // in the DB properly
        [Fact]
        public void SaveTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);

            Assert.True(activity.Save(dc: dc).HasNoErrors());
        }
        //Removes the Activity from the DB, it should not be able to be loaded from the DB afterwards
        [Fact]
        public void DeleteTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            activity.Delete(dc: dc);
            Assert.Null(Activity.Load(activity.ID.Value, dc: dc));
        }
        //Creates an ----------- and returns a message regarding the status of the ------ 
        [Fact]
        public void CreateTest()
        {
            Assert.True(Activity.Create(dc: dc).HasNoErrors());
        }
        //
        [Fact]
        public void AlterTest()
        {
            Assert.True(Activity.Alter(dc: dc).HasNoErrors());
        }

        //Loads the activity from the DB (using it's ID) and returns it.
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
        // activity groups are collections of activities, this methos will load a list of 
        // the activities in a group from the DB
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
        // Loads using a List of Activity Ids
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
        // This will get the number of Activitiess under one Account
        [Fact]
        public void GetCountByAccountIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc: dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            activity.Save(dc: dc);
            Assert.Equal(1, Activity.GetCountByAccountId(account.ID.Value, activity.ActivityGroupId.Value, null, null, dc: dc));
        }

        // This loads the Activities under one Account, very similar to the above
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

        // Gets the number of ActivitiyDays associated with the activity
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
        // Gets the days of the week that the activity repeats on weekly
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
        // Returns a list of the registrants that have "signed up" for the activity
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
        // Returns a list of all of the RegistrantActivities associated with the activity
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

        // will count the number of registrants for the particular activity of a given registrantion type
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
        
        //returns if an activity is full or not (checks the SeatsAvaible feild)
        
        //weird error with DeBugging
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

        //returns bolean (if Waitinglistseats = 10 and registration type)
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

        // difference between grade and grade level?
        // returns true if the activity his for students of the passed in gradelevel
        [Fact]
        public void HasGradeLevelTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc: dc);

            var activityGrade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc: dc);

            var x = activity.HasGradeLevel(gradeLevel, dc: dc);

            Assert.True(activity.HasGradeLevel(gradeLevel, dc: dc));
        }

        //Returns a list of all of the grade levels associated with that activity
        [Fact]
        public void GetGradeLevelsTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc: dc);
            gradeLevel.ID = 12;
            gradeLevel.Save(dc: dc);
            
            var activityGrade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc: dc);
      
            Assert.Single(activity.GetGradeLevels(dc: dc));
        }
        //Returns a list of all WebUSers signed up for the same activityID
        [Fact]
        public void GetTeamMembersTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var User = Factories.WebUserFactory.Create(msgs, dc: dc);
            var activityWebUser = Factories.ActivityWebUserFactory.Create(activity, User, msgs, dc: dc);

            Assert.Single(activity.GetTeamMembers(dc: dc));
        }

        //returns a list of all forms ascociated with that activity
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


        // Same as above, but they must also be active (not to be confused with a simple activity form)
        [Fact]
        public void GetActiveFormTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            activityForm.IsActive = true;
            activityForm.Save(dc: dc);

            Assert.NotNull(activity.GetActiveForm(dc: dc));
        }
        // Returns a list of the activity forms
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

        // Returns a list of the Attendance objects asociated with this activity.
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

        // does the same as above as long as the attendace has been completed
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

            Assert.Single(activity.GetCompletedAttendance(dc: dc));
        }
        // returns the numberof attendance objects asociated with the activity that have been marked as present
        [Fact]
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
            var registrant = Factories.RegistrantFactory.Create(account, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);

            Assert.Equal(1, activity.GetAttendancePresentCount(dc: dc));
        }
        
        [Fact]
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
            var registrant = Factories.RegistrantFactory.Create(account, msgs, dc: dc);
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
            var registrant = Factories.RegistrantFactory.Create(account, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);
            attendance.Date = date;
            attendance.Save(dc: dc);


            var count = activity.GetAttendanceByDate(date, dc: dc);

            Assert.Single(activity.GetAttendanceByDate(date, dc: dc));
        }
        // gets the Activity Registration Periods for the current activity
        // activity registration periods contain more methods that DO things
        [Fact]
        public void GetActivityRegistrationPeriodsTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetActivityRegistrationPeriods(dc: dc));
        }
        // Registration periods hold more of the fields of a registration
        [Fact]
        public void GetRegistrationPeriodsTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc: dc);

            Assert.Single(activity.GetRegistrationPeriods(dc: dc));
        }

        // TODO HELP
        [Fact]
        public void GetUpcommingRegistrationPeriodsTest()
        {
            var date = new DateTime(2018, 10, 10);
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
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
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
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
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
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
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var form = Factories.FormFactory.Create(account, msgs, dc: dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc: dc);
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

            Assert.True(activity.GetAccount(dc).IsNotEmpty() && activity.GetAccount(dc).ID == account.ID);
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
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, activityGroup, msgs, dc);

            var start = new DateTime(2017, 06, 06);
            var end = new DateTime(2017, 10, 10);
            var startTime = new DateTime(2018, 1, 1, 1, 1, 1);
            var endtime = new DateTime(2018, 3, 3, 3, 3, 3);

            var schedule = activity.AddNewSchedule(
                "newShed", 
                "det", 
                true, 
                start, 
                end, 
                startTime, 
                endtime, 
                "1/1/2019-12:39 PM", 
                account.ID.Value, 
                accountOwner.ID.Value, 
                msgs, 
                dc);

            Assert.NotNull(schedule);
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
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);

            var registrantIds = new List<int>() { registrant.ID.Value };

            Assert.NotNull(activity.AddMessage("subj", "Hello Class", registrantIds, accountOwner.ID.Value, msgs, dc));
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

            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            activityInstance.StartDateTime = new DateTime(1, 1, 1);
            activityInstance.EndDateTime = new DateTime(2020, 1, 1);
            activityInstance.ActivityScheduleId = schedule.ID.Value;
            activityInstance.Save(dc);

            Assert.True(activity.IsInSession(dc: dc));
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

            activity.StartDateTime = start;
            activityInstance.StartDateTime = start;
            msgs.AddRange(activityInstance.Save(dc));

            Assert.True(activity.WillBeInSession(dc));
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
            

            Assert.True(activity.IsOutOfSession(dc));
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

            Assert.Equal(instances.Last().StartDateTime.ToShortDateTimeString(), activity.GetLastDate(dc: dc).ToShortDateTimeString());
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
