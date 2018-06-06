using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class ActivityTest : IDisposable
    {
        DatabaseConnection dc = null;
        SystemMessages msgs = null;

        public ActivityTest()
        {
            AfterSchoolLibrary.Utilities.InitializeApp();
            dc = new DatabaseConnection(Database.Credentials.ConnectionString);
            msgs = new SystemMessages();
            dc.Open();
            dc.BeginTransaction();
        }

        public void Dispose()
        {
            dc.RollbackTransaction();
            dc.Close();
            dc.Dispose();
        }

        [Fact]
        public void LoadTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;
            Assert.NotNull(Activity.Load(id, dc));
        }

        [Fact]
        public void LoadLongTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Account.Load(c, 0, c.Count, dc));
        }


        [Fact]
        public void GetCountTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Activity.GetCount(c, dc));
        }

        [Fact]
        public void SaveTest()
        {
            var a = new Activity() { };
            Assert.NotNull(a.Save(dc));
        }

        [Fact]
        public void DeleteTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;
            a.Delete(dc);
            Assert.Null(Activity.Load(id, dc));
        }

        [Fact]
        public void CreateTest()
        {
            Assert.NotNull(Activity.Create(dc));
        }

        [Fact]
        public void AlterTest()
        {
            Assert.NotNull(Activity.Alter(dc));
        }


        [Fact]
        public void ReloadTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            Assert.NotNull(a.Reload(dc));
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;
            Assert.NotNull(Activity.LoadByAccountId(id, dc));
        }

        [Fact]
        public void LoadByAccountIdByIsPublishedTest()
        {
            var a = new Activity() { IsPublished = true };
            a.Save(dc);
            var id = a.ID.Value;
            Assert.NotNull(Activity.LoadByAccountIdByIsPublished(id, true, dc));
        }

        [Fact]
        public void LoadByLocationIdTest()
        {
            var a = new Activity() { LocationId = 1234 };
            a.Save(dc);
            var id = a.LocationId.Value;
            Assert.NotNull(Activity.LoadByLocationId(id, dc));
        }

        [Fact]
        public void LoadByArchiveIdTest()
        {
            var a = new Activity() { ArchiveId = 1234 };
            a.Save(dc);
            var id = a.ArchiveId.Value;
            Assert.NotNull(Activity.LoadByArchiveId(id, dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var g = new Guid();

            var a = new Activity() { Uid = g };
            a.Save(dc);
            var id = a.Uid;
            Assert.NotNull(Activity.LoadByUid(id, dc));
        }

        [Fact]
        public void LoadByAccountIdByArchiveIdTest()
        {
            var a = new Activity() { ParentActivityId = 1234 };
            a.Save(dc);
            var id = a.ParentActivityId.Value;
            Assert.NotNull(Activity.LoadByParentActivityId(id, dc));
        }

        [Fact]
        public void LoadByActivityGroupIdTest()
        {
            var a = new Activity() { ActivityGroupId = 1234 };
            a.Save(dc);
            var id = a.ActivityGroupId.Value;
            Assert.NotNull(Activity.LoadByActivityGroupId(id, dc));
        }

        [Fact]
        public void LoadByActivityScheduleIdTest()
        {
            var a = new Activity() { ActivityScheduleId = 1234 };
            a.Save(dc);
            var id = a.ActivityScheduleId.Value;
            Assert.NotNull(Activity.LoadByActivityScheduleId(id, dc));
        }

        [Fact]
        public void LoadByUniqueNameByAccountIdTest()
        {
            var a = new Activity() { UniqueName = "test", ID = 1};
            a.Save(dc);
            var id = a.ID.Value;
            var name = a.UniqueName;

            Assert.NotNull(Activity.LoadByUniqueNameByAccountId(name, id, dc));
        }

        [Fact]
        public void LoadListTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;
            var list = new List<int> { 1, 2, 3, 4, 5,id };
            Assert.NotNull(Activity.LoadList(list, dc));
        }

        [Fact]
        public void GetCountByAccountIdTest()
        {

            var a = new Activity() { AccountId = 1234, IsArchived = true, IsDeleted = false };
            a.Save(dc);
            var id = a.AccountId.Value;
            var actid = a.ActivityGroupId;
            Assert.NotNull(Activity.GetCountByAccountId(id, actid, true, false, dc));
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
            var a = new Activity() { IsPublished = false };
            a.Save(dc);
            a.SetIsPublished(true, 1234, dc);
            Assert.True(a.IsPublished);
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
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;

            var d = new ActivityDay() {ActivityId = id };
            d.Save(dc);

            Assert.Equal(1,a.GetDays(dc).Count);
        }

        [Fact]
        public void GetDaysOfTheWeekTest()
        {

            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;

            var s = new DateTime(1, 1, 1, 1, 1, 1);
            var e = new DateTime(7, 7, 7, 7, 7, 7);

            //a.AddNewSchedule("mondays","meets every monday",true,s,e,e,e,"dates",id,id,msgs,dc);

            Assert.NotNull(a.GetDaysOfTheWeek(dc));
        }

        [Fact]
        public void GetRegistrantsTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;

            //var r = new RegistrantActivity() { ActivityId = id, NumberOfApplicationSeats=1,NumberOfSeats = 1, NumberOfWaitingListSeats=1 };
            //r.Save(dc);

            Assert.NotNull(a.GetRegistrants(dc));
        }

        [Fact]
        public void GetRegistrantsActivitiesTest()
        {
            var a = new Activity() { };
            a.Save(dc);

            //var r = new RegistrantActivity() { ActivityId = id, NumberOfApplicationSeats=1,NumberOfSeats = 1, NumberOfWaitingListSeats=1 };
            //r.Save(dc);

            Assert.NotNull(a.GetRegistrantActivities(dc));
        }

        [Fact]
        public void GetRegistrantsActivitiesLongTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            Assert.NotNull(a.GetRegistrantActivities(true, dc));
        }

        [Fact]
        public void GetRegistrantsCountTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID.Value;

            var r = new RegistrantActivity() { ActivityId = id, NumberOfApplicationSeats=1,NumberOfSeats = 1, NumberOfWaitingListSeats=1 };
            r.Save(dc);

            Assert.Equal(0, a.GetRegistrantCount(dc));
        }

        [Fact]
        public void GetRegistrantCountByApplicationTypeTest()
        {
            var a = new Activity() { };
            a.Save(dc);

            var r = new RegistrantActivity() { };
            r.Save(dc);
            var rid = r.ID.Value;

            var n = new RegistrantAssignment() { RegistrantId = rid };

            Assert.Equal(0, a.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Invited, dc));
        }
        //how to make seats avaible
        [Fact]
        public void isFullTest()
        {
            var a = new Activity() { SeatsAvailable = 12 };
            a.Save(dc);
            var id = a.ID.Value;

            var r = new RegistrantActivity() { NumberOfInvitationSeats = 6, NumberOfApplicationSeats = 2, NumberOfSeats = 1, ActivityId = id };
            r.Save(dc);
            var rid = r.ID.Value;

            Assert.False(a.IsFull(dc));
        }

        [Fact]
        public void isWaitingListFullTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            Assert.True(a.IsWaitingListFull(dc)); //0 open with 0 on waiting list means full
        }

        [Fact]
        public void HasGradeLevelTest()
        {
            var a = new Activity() { ID =1 ,GradeLevelStart = 1, GradeLevelEnd = 6  };
            a.Save(dc);
            var aid = a.ID.Value;

            var g = new GradeLevel() {ID =1 };
            g.Save(dc);
            var gid = g.ID.Value;

            var actg = new ActivityGradeLevel() { ActivityId = aid, GradeLevelId = gid};
            actg.Save();

            Assert.True(a.HasGradeLevel(g, dc));
        }

        [Fact]
        public void GetGradeLevelsTest()
        {
            var a = new Activity() { GradeLevelStart = 1, GradeLevelEnd = 6 };
            a.Save(dc);
            var id = a.ID.Value;

            var gr = new GradeLevel() { ID = 1 };
            gr.Save(dc);
            var gid = gr.ID.Value;

            var g = new ActivityGradeLevel() { ActivityId = id, GradeLevelId = gid};
            g.Save(dc);

            Assert.NotNull( a.GetGradeLevels(dc));
        }

        [Fact]
        public void GetTeamMembersTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;

            var u = new ActivityWebUser() { ActivityId = id };
            u.Save(dc);

            Assert.Equal(1,a.GetTeamMembers(dc).Count);
        }


        [Fact]
        public void GetFormsTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;

            var form = new Form() {  };
            form.Save(dc);

            var f = new ActivityForm() { ActivityId = id };
            f.Save(dc);

            Assert.NotNull(a.GetForms(dc));
        }


        //this part sucks
        [Fact]
        public void GetActiveFormTest()
        {
            var acc = new Account() { IsActive = true };
            acc.Save(dc);
            var aid = acc.ID.Value;

            var a = new Activity() {};
            a.Save(dc);
            var id = a.ID;

            var form = new Form() {  AccountId = aid, IsActive = true};
            form.Save(dc);

            var f = new ActivityForm() { ActivityId = id, IsActive = true, FormId= 12 };
            f.Save(dc);
            var fid = f.FormId;



            Assert.NotNull(a.GetActiveForm(dc));
        }

        [Fact]
        public void GetActiveFormsTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;
            var f = new ActivityForm() { ActivityId = id };
            f.Save(dc);

            Assert.NotNull(a.GetActivityForms(dc));
        }

        [Fact]
        public void GetAttendanceTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;
            var f = new ActivityForm() { ActivityId = id };
            f.Save(dc);

            Assert.NotNull(a.GetAttendance(dc));
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
            var a = new Activity() { };
            a.Save(dc);


            var id = a.ID;
            var f = new ActivityRegistrationPeriod() { ActivityId = id };
            f.Save(dc);

            Assert.NotNull(a.GetActivityRegistrationPeriods(dc));
        }

        [Fact]
        public void GetRegistrationPeriodsTest()
        {
            var a = new Activity() { };
            a.Save(dc);


            var id = a.ID;
            var f = new ActivityRegistrationPeriod() { ActivityId = id };
            f.Save(dc);

            Assert.NotNull(a.GetRegistrationPeriods(dc));
        }

        // read only problem!
        [Fact]
        public void GetUpcommingRegistrationPeriodsTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;

            var date = new DateTime(2018, 10, 10) { };
            var f = new ActivityRegistrationPeriod() { ActivityId = id /*,RegistrationPeriodStartDateTime = date*/ };
            f.Save(dc);

            //f.RegistrationPeriodStartDateTime{ };

            Assert.NotNull(a.GetUpcomingRegistrationPeriods(dc));
        }

        [Fact]
        public void GetCurrentRegistrationPeriodsTest()
        {
            var a = new Activity() { };
            a.Save(dc);


            var id = a.ID;
            var f = new ActivityRegistrationPeriod() { ActivityId = id };
            f.Save(dc);
            var date = new DateTime(2018, 10, 10) { };
            //f.RegistrationPeriodStartDateTime{ };
            //f.RegistrationPeriodEndDateTime{ };

            Assert.NotNull(a.GetCurrentRegistrationPeriod(dc));
        }

        [Fact]
        public void GetCurrentOrNextRegistrationPeriodTest()
        {
            var a = new Activity() { };
            a.Save(dc);


            var id = a.ID;
            var f = new ActivityRegistrationPeriod() { ActivityId = id, };
            f.Save(dc);
            var date = new DateTime(2018, 10, 10) { };
            //f.RegistrationPeriodStartDateTime{ };
            //f.RegistrationPeriodEndDateTime{ };

            Assert.NotNull(a.GetCurrentOrNextRegistrationPeriod(dc));
        }

        [Fact]
        public void GetActiveRegistrationPeriodTest()
        {
            var a = new Activity() { };
            a.Save(dc);
            var id = a.ID;

            var f = new ActivityRegistrationPeriod() { ActivityId = id, IsActive = true };
            f.Save(dc);

            Assert.NotNull(a.GetActiveRegistrationPeriod(dc));
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
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Activity() { };
            a.Save(dc);

            var form = new Form() { };
            form.Save(dc);

            Assert.NotNull(a.AddFormAndActivitate(form, loggedInUserId, false, msgs, dc));
        }


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
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;

            var a = new Account() {  };
            a.Save(dc);
            var aid = a.ID.Value;

            var act = new Activity() {AccountId = loggedInUserId };
            act.Save(dc);

            var nv = new System.Collections.Specialized.NameValueCollection() { };

            var start = new DateTime(2017, 06, 06);
            var end = new DateTime(2017, 10, 10);
            var startTime = new DateTime(2018, 1, 1, 1, 1, 1);
            var endtime = new DateTime(2018, 3, 3, 3, 3, 3);

            var x = act.AddNewSchedule("newShed", "det", true, start, end, startTime, endtime, "Monday", aid, loggedInUserId, msgs, dc);

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
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;


            var g = new GradeLevel() { };
            g.Save(dc);
            var gid = g.ID.Value;

            var act = new Activity() {  AccountId = loggedInUserId};
            act.Save(dc);
            act.AddGrade(gid, loggedInUserId, msgs);
            var actid = act.ID.Value;

            var grade = new ActivityGradeLevel() { GradeLevelId = 1, ActivityId = actid };
            grade.Save(dc);


            Assert.NotNull(act.RemoveGrade(g, dc));
        }

        [Fact]
        public void AddGradeTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var loggedInUserId = currentUser.ID.Value;


            var g = new GradeLevel() { };
            g.Save(dc);
            var gid = g.ID.Value;

            var act = new Activity() { AccountId = loggedInUserId};
            act.Save(dc);

            var grade = new ActivityGradeLevel() { GradeLevelId = gid };
            grade.Save(dc);


            Assert.NotNull(act.AddGrade(gid, loggedInUserId, msgs, dc));
        }


        [Fact]
        public void MarkAsDeletedTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;

            var act = new Activity() { AccountId = UserId };
            act.Save(dc);


            Assert.NotNull(act.MarkAsDeleted(UserId, dc));
        }

        [Fact]
        public void MoveToArchiveTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;

            var act = new Activity() { AccountId = UserId };
            act.Save(dc);


            Assert.NotNull(act.MoveToArchive(UserId, dc));
        }

        [Fact]
        public void MoveFromArchiveTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;

            var act = new Activity() { AccountId = UserId };
            act.Save(dc);


            Assert.NotNull(act.MoveFromArchive(UserId, dc));
        }

        //I think there is a logic issue in the code
        [Fact]
        public void GetActivityGroupTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;


            var g = new ActivityGroup() { };
            g.Save(dc);
            var aid = g.ID.Value;

            var act = new Activity() { AccountId = UserId, ActivityGroupId = aid };
            act.Save(dc);
            act.AddGrade(1, UserId, msgs);

            var grade = new ActivityGradeLevel() { GradeLevelId = 1 };


            Assert.NotNull(act.GetActivityGroup(dc));
        }


        [Fact]
        public void AddOrUpdateActivityPropertyTest()
        {
            var currentUser = new WebUser() { IsActive = true };
            currentUser.Save(dc);
            var UserId = currentUser.ID.Value;


            var g = new ActivityGroup() { };
            g.Save(dc);
            var aid = g.ID.Value;

            var act = new Activity() { AccountId = UserId, ActivityGroupId = aid };
            act.Save(dc);
            act.AddGrade(1, UserId, msgs);

            Assert.NotNull(act.AddOrUpdateActivityProperty(AccountPropertyType.LandingPageText, "value", UserId, msgs, dc));
        }


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
            var act = new Activity() {};
            act.Save(dc);
            var actid = act.ID.Value;

            var ap = new AccountProperty() { AccountPropertyTypeId = AccountPropertyType.ApplicationThankYouEmail, ActivityId = actid };
            ap.Save(dc);

            Assert.NotNull(act.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, dc));
        }

        [Fact]
        public void AddMessageTest()
        {

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            List<int> iList = new List<int>();
                iList.Add(1);
                iList.Add(2);
                iList.Add(3);

            Assert.NotNull(act.AddMessage("subj","Hello Class",iList,id,msgs,dc));
        }

        [Fact]
        public void GetActivitiesByRegistrantByDayTest()
        {

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };
            var start = new DateTime(2018,10,10,10,10,10) { };

            Assert.NotNull(Activity.GetActivitiesByRegistrantByDay(id,DayOfWeekASL.Monday,start,dc));
        }

        [Fact]
        public void GetByActivitiesByRegistrantsTest()
        {

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            List<int> userList = new List<int>();
            userList.Add(id);

            var x = new AccountPropertyType() { };
            var start = new DateTime(2018, 10, 10, 10, 10, 10) { };

            List<int> regList = new List<int>();
            regList.Add(1);
            regList.Add(2);
            regList.Add(3);

            Assert.NotNull(Activity.GetByActivitiesByRegistrants(userList,regList,DayOfWeekASL.Monday,RegistrationStatus.NotSet,regList,dc));
        }

        [Fact]
        public void GetAllowedGendersTest()
        {

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };
            var start = new DateTime(2018, 10, 10, 10, 10, 10) { };

            Assert.NotNull(act.GetAllowedGenders());
        }

        [Fact]
        public void IsInSessionTest()
        {

            var start = new DateTime(1,1,1,1,1,1) { };
            var end = new DateTime(3000, 12, 12, 24, 59, 59) { };

            var wuser = new WebUser() {  };
            wuser.Save(dc);
            var uid = wuser.ID.Value;

            var act = new Activity() { AccountId = uid};
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            act.AddNewSchedule("shed", "dets", true, start, end, start, end, "dates",uid,uid,msgs,dc);

            Assert.True(act.IsInSession());
        }

        [Fact]
        public void WillBeInSessionTest()
        {

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };
            var start = new DateTime(2018, 10, 10, 10, 10, 10) { };

            Assert.True(act.WillBeInSession());
        }

        [Fact]
        public void IsOutOfSessionTest()
        {
            var start = new DateTime(1, 1, 1, 1, 1, 1) { };
            var end = new DateTime(2, 2, 2, 2, 2, 2) { };


            var act = new Activity() {StartDateTime = start, EndDateTime = end };
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };

            Assert.True(act.IsOutOfSession(dc));
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
            var start = new DateTime(1, 1, 1, 1, 1, 1) { };
            var end = new DateTime(2, 2, 2, 2, 2, 2) { };


            var act = new Activity() { StartDateTime = start, EndDateTime = end};
            act.Save(dc);
            var actid = act.ID.Value;

            var user = new ActivityWebUser() { ActivityId = actid };
            user.Save(dc);
            var id = user.ID.Value;

            var x = new AccountPropertyType() { };

            //Assert.NotNull(Activity.SendRegistrationSummaryReport(acid,actlab,dc));
        }
    }
}
