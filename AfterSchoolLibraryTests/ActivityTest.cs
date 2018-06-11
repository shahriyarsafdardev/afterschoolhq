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
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.NotNull(Activity.Load(activity.ID.Value, dc));
        }

        [Fact]
        public void GetCountTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.Equal(1,Activity.GetCount(null, dc));
        }

        [Fact]
        public void SaveTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.True(activity.Save(dc).HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            activity.Delete(dc);
            Assert.Null(Activity.Load(activity.ID.Value, dc));
        }

        [Fact]
        public void CreateTest()
        {
            Assert.True(Activity.Create(dc).HasNoErrors());
        }

        [Fact]
        public void AlterTest()
        {
            Assert.True(Activity.Alter(dc).HasNoErrors());
        }


        [Fact]
        public void ReloadTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.NotNull(activity.Reload(dc));
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            Assert.Single(Activity.LoadByAccountId(account.ID.Value, dc));
        }

        [Fact]
        public void LoadByAccountIdByIsPublishedTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            activity.IsPublished = true;
            activity.Save(dc);
            Assert.Single(Activity.LoadByAccountIdByIsPublished(account.ID.Value, true, dc));
        }

        [Fact]
        public void LoadByLocationIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var location = new Location() {};
            location.Save(dc);
            activity.LocationId = location.ID.Value;
            activity.Save(dc);
            Assert.Single(Activity.LoadByLocationId(activity.LocationId.Value, dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            Assert.NotNull(Activity.LoadByUid(activity.Uid, dc));
        }

        [Fact]
        public void LoadByActivityGroupIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            activity.Save(dc);
            Assert.Single(Activity.LoadByActivityGroupId(activity.ActivityGroupId.Value, dc));
        }

        [Fact]
        public void LoadByActivityScheduleIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            activity.ActivityScheduleId = 12;
            activity.Save(dc);
            Assert.Single(Activity.LoadByActivityScheduleId(activity.ActivityScheduleId.Value, dc));
        }

        [Fact]
        public void LoadByUniqueNameByAccountIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            activity.UniqueName = "test";
            activity.Save(dc);
            Assert.NotNull(Activity.LoadByUniqueNameByAccountId(activity.UniqueName, account.ID.Value, dc));
        }

        [Fact]
        public void LoadListTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            List<int> list = new List<int>();
            list.Add(activity.ID.Value);


            Assert.Single(Activity.LoadList(list, dc));
        }

        [Fact]
        public void GetCountByAccountIdTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            activity.ActivityGroupId = activityGroup.ID.Value;
            activity.Save(dc);
            Assert.Equal(1,Activity.GetCountByAccountId(account.ID.Value, activity.ActivityGroupId.Value, false, false, dc));
        }

        [Fact]
        public void LoadByAccountIdLongTest()
        {

            var a = new Activity() { AccountId = 1234, IsArchived = true, IsDeleted = false };
            a.Save(dc);
            var id = a.AccountId.Value;
            var actid = a.ActivityGroupId;
            Assert.NotNull(Activity.LoadByAccountId(id, actid, true, false, 0, 100, dc));
        }

        [Fact]
        public void SetIsPublishedTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            activity.SetIsPublished(true, account.ID.Value, dc);
            Assert.True(activity.IsPublished);
        }

        /*
        [Fact]
        public void GetParentActivityTest()
        {
            var a = new Activity() { ParentActivityId = 2 };
            a.Save(dc);

            Assert.NotNull(a.GetParentActivity(dc));
        }
        */

        [Fact]
        public void GetDaysTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            var d = new ActivityDay() {ActivityId = activity.ID.Value };
            d.Save(dc);
            Assert.Equal(1,activity.GetDays(dc).Count);
        }

        [Fact]
        public void GetDaysOfTheWeekTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);

            var s = new DateTime(1, 1, 1, 1, 1, 1);
            var e = new DateTime(7, 7, 7, 7, 7, 7);

            schedule.StartDate = s;
            schedule.EndDate = e;

            var activityShedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            activity.ActivityScheduleId = activityShedule.ID.Value;
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, msgs, dc);
            activityInstance.StartDateTime = s;
            var d = new ActivityDay() { ActivityId = activity.ID.Value };
            d.Save(dc);
            d.ActivityId = activity.ID.Value;

            Assert.Single(activity.GetDaysOfTheWeek(dc));
        }

        [Fact]
        public void GetRegistrantsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            var x = activity.GetRegistrants(dc);
            Assert.Single(activity.GetRegistrants(dc));
        }
/*
        [Fact]
        public void GetRegistrantsActivitiesTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            Assert.Single(activity.GetRegistrantActivities(dc));
        }
*/
        [Fact]
        public void GetRegistrantsActivitiesLongTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            Assert.Single(activity.GetRegistrantActivities(true, dc));
        }

        [Fact]
        public void GetRegistrantsCountTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            activity.SeatsAvailable = 1;
            activity.Save(dc);

            Assert.NotEqual(0, activity.GetRegistrantCount(dc));
        }

        [Fact]
        public void GetRegistrantCountByApplicationTypeTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            var n = new RegistrantAssignment() { RegistrantId = registrant.ID.Value };

            Assert.Equal(1, activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Invited, dc));
        }
        //weird error with DeBugging
        [Fact]
        public void isFullTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var regitrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            activity.SeatsAvailable = 1;
            activity.Save(dc);
            var x = activity.IsFull();
            Assert.False(activity.IsFull(dc));
        }

        [Fact]
        public void isWaitingListFullTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            Assert.True(a.IsWaitingListFull(dc)); //0 open with 0 on waiting list means full
        }

        // difference between grade and grade level?
        [Fact]
        public void HasGradeLevelTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc);

            var activityGrade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc);

            var x = activity.HasGradeLevel(gradeLevel, dc);

            Assert.True(activity.HasGradeLevel(gradeLevel, dc));
        }

        [Fact]
        public void GetGradeLevelsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc);
            gradeLevel.ID = 12;
            gradeLevel.Save(dc);

            var activityGrade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc);

            Assert.Single( activity.GetGradeLevels(dc));
        }

        [Fact]
        public void GetTeamMembersTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var User = Factories.WebUserFactory.Create(msgs, dc);
            var activityWebUser = Factories.ActivityWebUserFactory.Create(activity, User, msgs, dc);

            Assert.Single(activity.GetTeamMembers(dc));
        }


        [Fact]
        public void GetFormsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form,msgs,dc);

            Assert.Single(activity.GetForms(dc));
        }


        //this part sucks
        [Fact]
        public void GetActiveFormTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc);
            form.IsActive = true;
            activityForm.IsActive = true;

            Assert.NotNull(activity.GetActiveForm(dc));
        }

        [Fact]
        public void GetActivityFormsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc);

            Assert.Single(activity.GetActivityForms(dc));
        }

        //TODO Attendance Factory
        [Fact]
        public void GetAttendanceTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc);
            //var attendance = Factories.a

            Assert.NotNull(activity.GetAttendance(dc));
        }

        [Fact]
        public void GetCompletedAttendanceTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;
            var f = new ActivityForm() { ActivityId = id };
            f.Save(dc);

            Assert.NotNull(a.GetCompletedAttendance(dc));
        }

        [Fact]
        public void GetAttendancePresentCountTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;

            var f = new ActivityForm() { ActivityId = id };
            f.Save(dc);

            Assert.Equal(1, a.GetAttendancePresentCount(dc));
        }

        [Fact]
        public void GetAttendancePossibleCountTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;
            var f = new ActivityForm() { ActivityId = id };
            f.Save(dc);

            Assert.NotNull(a.GetAttendancePossibleCount(dc));
        }

        [Fact]
        public void GetAttendanceByDateTest()
        {
            var date = new DateTime(2018, 10, 10) { };
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;
            var f = new ActivityForm() { ActivityId = id, CreateDate = date };
            f.Save(dc);

            Assert.NotNull(a.GetAttendanceByDate(date, dc));
        }

        [Fact]
        public void GetActivityRegistrationPeriodsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.Single(activity.GetActivityRegistrationPeriods(dc));
        }

        [Fact]
        public void GetRegistrationPeriodsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);

            Assert.Single(activity.GetRegistrationPeriods(dc));
        }

        // HELP
        [Fact]
        public void GetUpcommingRegistrationPeriodsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);
            registrationPeriod.StartDateTime = new DateTime(2018, 10, 10);

            Assert.Single(activity.GetUpcomingRegistrationPeriods(dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);
            registrationPeriod.StartDateTime = new DateTime(18, 10, 10);
            registrationPeriod.EndDateTime = new DateTime(2018, 10, 10);

            Assert.NotNull(activity.GetCurrentRegistrationPeriod(dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);
            registrationPeriod.StartDateTime = new DateTime(18, 10, 10);
            registrationPeriod.EndDateTime = new DateTime(2018, 10, 10);

            Assert.NotNull(activity.GetCurrentOrNextRegistrationPeriod(dc));
        }

        [Fact]
        public void GetActiveRegistrationPeriodTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs, dc);
            var activityRegistrationPeriod = Factories.ActivityRegistrationPeriodFactory.Create(activity, registrationPeriod, msgs, dc);
            activityRegistrationPeriod.IsActive = true;

            Assert.NotNull(activity.GetActiveRegistrationPeriod(dc));
        }

        [Fact]
        public void GetAccountTest()
        {

            var account = new Account() { };
            account.Save(dc);
            var aid = account.ID.Value;

            var a = new Activity() { AccountId = aid };
            a.Save(dc);

            Assert.NotNull(a.GetAccount(dc));
        }

        [Fact]
        public void CopyTest()
        {
            var currentUser = new WebUser() { IsActive = true, };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Activity() {AccountId = loggedInUserId };
            a.Save(dc);
            var aid = a.ID.Value;

            Assert.NotNull(a.Copy("newActivity", true, loggedInUserId, msgs, dc));
        }

        [Fact]
        public void CopyDefaultFormTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Activity() {AccountId = loggedInUserId };
            a.Save(dc);
            var aid = a.ID.Value;

            var form = new ActivityForm() {  AccountId = loggedInUserId, ActivityId = aid, IsActive =true};
            form.Save(dc);

            Assert.NotNull(a.CopyDefaultForm(loggedInUserId, dc));
        }

        [Fact]
        public void AddFormAndActivateTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);

            Assert.NotNull(activity.AddFormAndActivitate(form, user.ID.Value, false, msgs, dc));
        }

        //How to construct a base adresss?
        [Fact]
        public void HasAdressTest()
        {
            var a = new Activity() { };
            a.Save(dc);

            Assert.False(a.HasAddress);
        }

        [Fact]
        public void EditExistingTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Account() { };
            a.Save(dc);
            var aid = a.ID.Value;

            var act = new Activity() { };
            a.Save(dc);
            var actid = act.ID.ToString();

            var nv = new System.Collections.Specialized.NameValueCollection() { };

            Assert.NotNull(Activity.EditExisting(loggedInUserId, a, actid, nv, msgs));
        }

        [Fact]
        public void CreateNewTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Account() { };
            a.Save(dc);
            var aid = a.ID.Value;

            var act = new Activity() { };
            a.Save(dc);
            var actid = act.ID.ToString();

            var nv = new System.Collections.Specialized.NameValueCollection() { };

            Assert.NotNull(Activity.CreateNew(loggedInUserId, a, nv, msgs, dc));
        }

        [Fact]
        public void AddNewSheduleTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);

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
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Account() { };
            a.Save(dc);
            var aid = a.ID.Value;

            var act = new Activity() { };
            act.Save(dc);


            var nv = new System.Collections.Specialized.NameValueCollection() { };

            var start = new DateTime(2017, 06, 06);
            var end = new DateTime(2017, 10, 10);

            var x = act.AddNewRegistrationPeriod("newreg", start, end, aid, loggedInUserId, msgs, dc);

            Assert.NotNull(x);
        }

        [Fact]
        public void AddTeamMemberTest()
        {
            var User = new WebUser() { };
            User.Save(dc);
            var UserId = User.ID.Value;

            var currentUser = new WebUser() { };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Activity() { };
            a.Save(dc);
            var aid = a.ID.Value;



            var x = a.AddTeamMember(UserId, true, loggedInUserId, msgs, dc);


            Assert.NotNull(x);
        }

        [Fact]
        public void RemoveGradeTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc);
            var grade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc);


            Assert.True(activity.RemoveGrade(gradeLevel, dc).HasNoErrors());
        }

        [Fact]
        public void AddGradeTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var gradeLevel = Factories.GradeLevelFactory.Create(msgs, dc);
            var grade = Factories.ActivityGradeLevelFactory.Create(activity, gradeLevel, msgs, dc);

            Assert.NotNull(activity.AddGrade(gradeLevel.ID.Value, user.ID.Value, msgs, dc));
        }


        [Fact]
        public void MarkAsDeletedTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;

            var act = new Activity() { AccountId = UserId };
            act.Save(dc);


            Assert.NotNull(act.MarkAsDeleted(UserId, dc).HasNoErrors());
        }

        [Fact]
        public void MoveToArchiveTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;

            var act = new Activity() { AccountId = UserId };
            act.Save(dc);


            Assert.True(act.MoveToArchive(UserId, dc).HasNoErrors());
        }

        [Fact]
        public void MoveFromArchiveTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;

            var act = new Activity() { AccountId = UserId };
            act.Save(dc);


            Assert.True(act.MoveFromArchive(UserId, dc).HasNoErrors());
        }

        //I think there is a logic issue in the code
        [Fact]
        public void GetActivityGroupTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            activity.ActivityGroupId = activityGroup.ID.Value;


            Assert.NotNull(activity.GetActivityGroup(dc));
        }


        [Fact]
        public void AddOrUpdateActivityPropertyTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);

            Assert.NotNull(activity.AddOrUpdateActivityProperty(AccountPropertyType.LandingPageText, "value", user.ID.Value, msgs, dc));
        }

        //TODO Learn to add shedule
        [Fact]
        public void HasSheduleTest()
        {
            var act = new Activity() { };
            act.Save(dc);

            Assert.False(act.HasSchedule);
        }

        [Fact]
        public void GetSheduleTest()
        {
            var x = new ActivitySchedule() { };
            x.Save(dc);
            var xid = x.ID.Value;

            var act = new Activity() {ActivityScheduleId = xid};
            act.Save(dc);
            Assert.NotNull(act.GetSchedule(false, dc ));
        }

        [Fact]
        public void GetActivityPropertyTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            var accountProperty = Factories.AccountPropertyFactory.Create(account, AccountPropertyType.ApplicationThankYouEmail, msgs, dc);
            accountProperty.ActivityId = activity.ID.Value;


            Assert.NotNull(activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, dc));
        }

        [Fact]
        public void AddMessageTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);

            List<int> iList = new List<int>();
                iList.Add(registrant.ID.Value);
                iList.Add(2);
                iList.Add(3);

            Assert.NotNull(activity.AddMessage("subj","Hello Class",iList,user.ID.Value,msgs,dc));
        }

        [Fact]
        public void GetActivitiesByRegistrantByDayTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var activityShedule = Factories.ActivityScheduleFactory.Create(msgs, dc);
            activityShedule.AccountId = account.ID.Value;
            activityShedule.StartDate = new DateTime(2018, 6, 11);
            
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);


            Assert.Single(Activity.GetActivitiesByRegistrantByDay(registrant.ID.Value,DayOfWeekASL.Monday,activityShedule.StartDate.Value,dc));
        }

        [Fact]
        public void GetByActivitiesByRegistrantsTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);

            var registrant = Factories.RegistrantFactory.Create(msgs, dc);

            List<int> regList = new List<int>();
            regList.Add(registrant.ID.Value);

            List<int> actList = new List<int>();
            regList.Add(activity.ID.Value);

            Assert.Single(Activity.GetByActivitiesByRegistrants(actList,regList,0,RegistrationStatus.NotSet,null,dc));
        }

        [Fact]
        public void GetAllowedGendersTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            activity.GenderRestriction = GenderRestrictionType.None;

            Assert.Equal(2,activity.GetAllowedGenders().Count);
        }

        [Fact]
        public void IsInSessionTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var form = Factories.FormFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var Sched = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            Sched.StartDate = new DateTime(1, 1, 1);
            Sched.EndDate = new DateTime(2020, 1, 1);
            Sched.DailyStartTime = new DateTime(1, 1, 1, 1, 1, 1);
            Sched.DailyEndTime = new DateTime(24, 1, 1, 1, 1, 1);


            Assert.True(activity.IsInSession());
        }

        [Fact]
        public void WillBeInSessionTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var Sched = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, msgs, dc);

            var start = new DateTime(2018, 10, 10, 10, 10, 10) { };
            Sched.StartDate = start;
            activity.StartDateTime = start;
            activityInstance.StartDateTime = start;
            activity.ActivityScheduleId = Sched.ID.Value;

            Assert.True(activity.WillBeInSession());
        }

        [Fact]
        public void IsOutOfSessionTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var Sched = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, msgs, dc);

            var start = new DateTime(2018, 10, 10, 10, 10, 10) { };
            Sched.StartDate = start;
            activity.StartDateTime = start;
            activityInstance.StartDateTime = start;
            activity.ActivityScheduleId = Sched.ID.Value;

            Assert.True(activity.IsOutOfSession(dc));
        }

        [Fact]
        public void GetFirstDateTest()
        {
            var start = new DateTime(1, 1, 1, 1, 1, 1) { };
            var end = new DateTime(2, 2, 2, 2, 2, 2) { };


            var act = new Activity() { StartDateTime = start, EndDateTime = end };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };

            Assert.NotNull(act.GetFirstDate(dc));
        }

        [Fact]
        public void GetLastDateTest()
        {
            var start = new DateTime(1, 1, 1, 1, 1, 1) { };
            var end = new DateTime(2, 2, 2, 2, 2, 2) { };


            var act = new Activity() { StartDateTime = start, EndDateTime = end };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };

            Assert.NotNull(act.GetLastDate(dc));
        }

        [Fact]
        public void SendRegistrationSummaryReportsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);

            Assert.NotNull(Activity.SendRegistrationSummaryReport(activity.ID.Value,"test",dc).HasNoErrors());
        }
    }
}
