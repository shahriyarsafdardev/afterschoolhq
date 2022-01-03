using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System;
using System.Collections.Generic;
using BasicLibrary;
using System.Threading.Tasks;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account ActivityOwnerAccount { get; set; }

        public ActivityServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            ActivityOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        //[Fact(DisplayName = "Payment plan is saved as an option on an activity")]
        //public void AddPaymentPlanActivityObjectTest()
        //{
        //    var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
        //    var plan = activity.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

        //    Assert.Single(SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, SubscriptionPlanType.PaymentPlan, msgs, dc));
        //}

        [Fact(DisplayName = "Payment plan is saved as an option on an activity group")]
        public void AddPaymentPlanActivityGroupObjectTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var plan = activityGroup.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

            Assert.Single(SubscriptionPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, SubscriptionPlanType.PaymentPlan, msgs, dc));
        }

        [Fact(DisplayName = "GetParticipationObjectActivityTestFull")]
        public void GetParticipationObjectActivityTestFull()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, location,null, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var period = RegistrationPeriodFactory.Create(ActivityOwnerAccount, msgs, dc);
            var arp = ActivityRegistrationPeriodFactory.Create(activity, period, msgs, dc);

            Assert.IsType<ParticipationObject>(activity.GetParticipationObject(msgs, dc));
        }

        [Fact(DisplayName = "GetParticipationObjectActivityTestSimple")]
        public void GetParticipationObjectActivityTestSimple()
        {           
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            Assert.IsType<ParticipationObject>(activity.GetParticipationObject(msgs, dc));
        }

        [Fact(DisplayName = "GetPaymentPlansTest")]
        public void GetPaymentPlansTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var plan = SubscriptionPlanFactory.Create(ActivityOwnerAccount, activity, SubscriptionPlanType.PaymentPlan, msgs, dc);

            Assert.Single(activity.GetActivePaymentPlans(msgs, dc));
        }

        [Fact(DisplayName = "GetSubscriptionPlansTest")]
        public void GetSubscriptionPlansTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var plan = SubscriptionPlanFactory.Create(ActivityOwnerAccount, activity, SubscriptionPlanType.Subscription, msgs, dc);

            Assert.Single(activity.GetActiveSubscriptionPlans(msgs, dc));
        }

        [Fact(DisplayName = "CreateNewTest")]
        public void CreateNewTest()
        {
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);
            var activity = ActivityService.CreateNew(
                activityName: "Test Activity",
                activityType: ActivityType.Activity,
                activityDetails: "<p>Here are some details</p>",
                seatsAvailable: 100,
                waitingListSeatsAvailable: 10,
                genderRestriction: GenderRestrictionType.None,
                contactName: "Test User",
                contactPhone: "123-123-1234",
                contactEmail: "dave@dave.com",
                costPerRegistrant: 12,
                costDescription: "Here is the main reason for the cost and what you get",
                acceptCreditCard: false,
                allowToPayLater: true,
                registrationMode: RegistrationMode.Register,
                accountId: ActivityOwnerAccount.ID,
                scheduleId: schedule.ID,
                locationId: location.ID,
                locationDetails: "<p>this is where instructions go</p>",
                activityGroupId: activityGroup.ID,
                minGradeLevelId: gradeLevels.First().ID,
                maxGradeLevelId: gradeLevels.Last().ID,
                loggedInUser: this.LoggedInUser,
                msgs: msgs,
                dc: dc
                );

            Assert.NotNull(activity.ID);
        }

        /// <summary>
        /// returns the numberof attendance registrant objects asociated with the activity that are newly created against an attendace
        /// </summary>
        [Fact]
        public void CreateNewAttendanceRegistrantByRegistrantActivitiesTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            var activityForm = ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);
            var newRegistrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc: dc);
            var registrantActivity = RegistrantActivityFactory.Create(activity, newRegistrant, msgs, dc);

            List<AttendanceRegistrant> attendanceRegistrants = new List<AttendanceRegistrant>();
            attendanceRegistrants.Add(attendanceRegistrant);

            List<RegistrantActivity> registrantActivities = new List<RegistrantActivity>();
            registrantActivities.Add(registrantActivity);

            Assert.Single(activity.CreateNewAttendanceRegistrantByRegistrantActivities(attendance, attendanceRegistrants, registrantActivities,LoggedInUser, msgs, dc: dc));
        }

        /// <summary>
        /// Tests that an activity instance has schedule id or not 
        /// </summary>
        [Fact]
        public void IsValidActivityInstance()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);


            Assert.True(activity.IsValidActivityInstance(activityInstance, msgs, dc));
        }

        /// <summary>
        /// Tests that an activity instance does not have schedule id or not 
        /// </summary>
        [Fact]
        public void IsNotValidActivityInstance()
        {
            // Arrange
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var wrongSchedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            // Act
            activityInstance.ActivityScheduleId = wrongSchedule.ID.Value;
            msgs.AddRange(activityInstance.Save(dc));

            // Assert
            Assert.False(activity.IsValidActivityInstance(activityInstance, msgs, dc));
        }

        [Theory(DisplayName = "GenerateUniqueName")]
        [InlineData("My Sample Account", "MySampleAccount")]
        [InlineData("My Sample Account Rocks Bud Boy", "MySampleAccountRocksBud")]
        [InlineData("My!@#$%^&*()-_+<>/,.?[]{}\\|'\":;~`", "My-_")]
        public void GenerateUniqueName(string input, string expectedOutput)
        {
            var output = ActivityService.GenerateUniqueName(input, null, ActivityOwnerAccount.ID, msgs, dc);

            Assert.Equal(expectedOutput, output);
        }

        [Fact(DisplayName = "FinalizeStartAndEndDateForSubscriptionTest")]
        public void FinalizeStartAndEndDateForSubscriptionTest()
        {
            DateTime dayOfMonth = new DateTime(2021, 06, 06);

            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, msgs, dc);

            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity, BillingPeriodType.Month, System.DayOfWeek.Monday, dayOfMonth, msgs,dc);

            Assert.True(paymentStart == dayOfMonth);
        }    
        
        [Fact(DisplayName = "FinalizeStartAndEndDateForWeekSubscriptionTest")]
        public void FinalizeStartAndEndDateForOneWeekSubscriptionTest()
        {
            DateTime dayOfMonth = new DateTime();

            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, msgs, dc);

            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity, BillingPeriodType.Week, System.DayOfWeek.Monday, dayOfMonth, msgs,dc);


            Assert.True(paymentStart.HasValue && paymentEnd.HasValue);
        }

        [Fact]
        public void GetProviderTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schoolUser = Factories.WebUserFactory.Create(msgs, dc);
            var schoolAccount = Factories.AccountFactory.Create(schoolUser, msgs, dc);

            var activity = Factories.ActivityFactory.Create(schoolAccount, msgs, dc);
            activity.SetProvider(account, schoolUser, msgs, dc);

            var provider = activity.GetProvider(msgs, dc);

            Assert.Equal(account.ID, provider.ID);
            Assert.True(msgs.HasNoErrors());

        }

        [Fact]
        public void SetProviderTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schoolUser = Factories.WebUserFactory.Create(msgs, dc);
            var schoolAccount = Factories.AccountFactory.Create(schoolUser, msgs, dc);

            var activity = Factories.ActivityFactory.Create(schoolAccount, msgs, dc);
            activity.SetProvider(account, schoolUser, msgs, dc);

            Assert.Equal(activity.ProviderAccountId, account.ID);
        }

        [Fact]
        public void SetLocationTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);

            activity.SetLocation(location, LoggedInUser, msgs, dc);

            Assert.Equal(location.ID.Value, activity.LocationId.Value);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetLocationTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);

            activity.SetLocation(location, LoggedInUser, msgs, dc);
            var activityLocation = activity.GetLocation(msgs, dc);

            Assert.Equal(location.ID.Value, activityLocation.ID.Value);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetLocationOwnerTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);

            // Act
            activity.SetLocation(location, LoggedInUser, msgs, dc);
            var locationOwner = activity.GetLocationOwner(msgs, dc);

            // Assert
            Assert.Equal(ActivityOwnerAccount.ID.Value, locationOwner.ID.Value);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetLocationOwnerFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            // Act
            // we are not setting any location with this activity, so location owner will be empty
            var locationOwner = activity.GetLocationOwner(msgs, dc);

            // Assert
            Assert.Null(locationOwner);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void IsOpenTest()
        {
            var activity1 = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity2 = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity3 = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var period1 = RegistrationPeriodFactory.Create(ActivityOwnerAccount, msgs, dc);
            var period2 = RegistrationPeriodFactory.Create(ActivityOwnerAccount, DateTime.UtcNow.AddDays(-12), DateTime.UtcNow.AddDays(-1), msgs, dc);
            var period3 = RegistrationPeriodFactory.Create(ActivityOwnerAccount, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(12), msgs, dc);

            msgs.AddRange(period1.AddActivity(activity1, true, LoggedInUser.ID.Value, dc));
            msgs.AddRange(period2.AddActivity(activity2, true, LoggedInUser.ID.Value, dc));
            msgs.AddRange(period3.AddActivity(activity3, true, LoggedInUser.ID.Value, dc));

            Assert.True(msgs.HasNoErrors());
            Assert.True(activity1.IsOpen(msgs, dc));
            Assert.False(activity2.IsOpen(msgs, dc));
            Assert.False(activity3.IsOpen(msgs, dc));
        }

        #region Activity Copy Functionality

        [Fact]
        public void ActivityCopyTest()
        {
            try
            {
                // Arrange                
                var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
                var package = PackageFactory.Create(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);
                var form = ActivityOwnerAccount.GetOrCreateDefaultForm(LoggedInUser, msgs, dc);
                 var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, msgs, dc);                

                // Act
                var expected = activity.Copy(newName: activity.Name + "- [copy]", useSameForm: false, loggedInUserId: LoggedInUser.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.NotNull(expected);
                Assert.NotEqual(activity.ID, expected.ID);
                Assert.NotEqual(activity.CreateDate, expected.CreateDate);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public async void ActivityCopyScheduleCheckTest()
        {
            try
            {
                // Arrange                
                var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
                var package = PackageFactory.Create(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);
                var form = ActivityOwnerAccount.GetOrCreateDefaultForm(LoggedInUser, msgs, dc);
                var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, msgs, dc);

                var startDates = new List<DateRange>()
                {
                    new DateRange() {StartDate = $"6/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"6/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"7/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"7/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"8/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"8/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                };

                var endDates = new List<DateTime>() { };
                ActivityScheduleFactory.Create(ActivityOwnerAccount, activity, schedule, startDates, endDates, LoggedInUser, msgs, dc);

                // Act
                var newName = activity.Name + "- [copy]";
                Activity expected = new Activity();
                await Task.Delay(2000);
                
                expected = activity.Copy(newName: newName, useSameForm: false, loggedInUserId: LoggedInUser.ID.Value, msgs: msgs, dc: dc);
                
                // Assert
                Assert.Equal(expected.Name, newName);

                var activitySchedule = activity.GetSchedule(msgs, dc);
                var copiedActivitySchedule = expected.GetSchedule(msgs: msgs, dc: dc);
                
                Assert.Equal(activitySchedule.AccountId, copiedActivitySchedule.AccountId);
                Assert.Equal(activitySchedule.IsOngoing, copiedActivitySchedule.IsOngoing);
                Assert.Equal(activitySchedule.RepeatsWeekly, copiedActivitySchedule.RepeatsWeekly);
                Assert.Equal(activitySchedule.StartDate, copiedActivitySchedule.StartDate);
                Assert.Equal(activitySchedule.EndDate, copiedActivitySchedule.EndDate);
                Assert.Equal(activitySchedule.DailyStartTime, copiedActivitySchedule.DailyStartTime);
                Assert.Equal(activitySchedule.DailyEndTime, copiedActivitySchedule.DailyEndTime);
                Assert.NotEqual(activitySchedule.ID, copiedActivitySchedule.ID);
                Assert.NotEqual(activitySchedule.CreateDate, copiedActivitySchedule.CreateDate);

                var scheduleInstances = activitySchedule.GetInstances(msgs: msgs, dc: dc);
                var copiedScheduleInstances = copiedActivitySchedule.GetInstances(msgs: msgs, dc: dc);

                Assert.Equal(scheduleInstances.Count, copiedScheduleInstances.Count);

                for (int i = 0; i < scheduleInstances.Count; i++)
                {
                    var instance = scheduleInstances[i];
                    var copiedInstance = copiedScheduleInstances[i];
                    
                    Assert.Equal(instance.ActivityId, copiedInstance.ActivityId);
                    Assert.Equal(instance.ActivityGroupId, copiedInstance.ActivityGroupId);
                    Assert.Equal(instance.Day, copiedInstance.Day);
                    Assert.Equal(instance.StartDateTime, copiedInstance.StartDateTime); 
                    Assert.Equal(instance.EndDateTime, copiedInstance.EndDateTime);
                    Assert.Equal(instance.InstanceTypeId, copiedInstance.InstanceTypeId);
                    Assert.NotEqual(instance.ID, copiedInstance.ID);
                    Assert.NotEqual(instance.CreateDate, copiedInstance.CreateDate);
                }
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        public static IEnumerable<object[]> Data => new List<object[]>() {
            new object[] { 
                new List<DateRange>() {
                    new DateRange() {
                        StartDate = $"{DateTime.Now.Month}/6/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(),
                        EndDate = $"{DateTime.Now.Month}/6/{DateTime.Now.Year} 5:00:00 PM".ToDateTime() 
                    },
                    new DateRange() {
                        StartDate =$"{DateTime.Now.Month + 1}/7/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(),
                        EndDate =$"{DateTime.Now.Month + 1}/7/{DateTime.Now.Year} 5:00:00 PM".ToDateTime(),
                    },
                    new DateRange() {
                        StartDate =$"{DateTime.Now.Month + 1}/8/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(),
                        EndDate =$"{DateTime.Now.Month + 1}/8/{DateTime.Now.Year} 5:00:00 PM".ToDateTime(),
                    }
                }
            }
        };

        [Theory]
        [MemberData(nameof(Data))]        
        public async void ActivityCopyScheduleCheckTheoryTest(List<DateRange> dates)
        {
            try
            {
                // Arrange                
                var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
                var package = PackageFactory.Create(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);
                var form = ActivityOwnerAccount.GetOrCreateDefaultForm(LoggedInUser, msgs, dc);
                var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, msgs, dc);

                var startDates = new List<DateRange>();
                startDates.AddRange(dates);

                var endDates = new List<DateTime>() { };
                ActivityScheduleFactory.Create(ActivityOwnerAccount, activity, schedule, startDates, endDates, LoggedInUser, msgs, dc);

                // Act
                var newName = activity.Name + "- [copy]";
                Activity expected = new Activity();
                await Task.Delay(2000); // this is becuase sometimes createDate assert gets file due to same time of activity and it's copy

                expected = activity.Copy(newName: newName, useSameForm: false, loggedInUserId: LoggedInUser.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected.Name, newName);

                var activitySchedule = activity.GetSchedule(msgs, dc);
                var copiedActivitySchedule = expected.GetSchedule(msgs: msgs, dc: dc);

                Assert.Equal(activitySchedule.AccountId, copiedActivitySchedule.AccountId);
                Assert.Equal(activitySchedule.IsOngoing, copiedActivitySchedule.IsOngoing);
                Assert.Equal(activitySchedule.RepeatsWeekly, copiedActivitySchedule.RepeatsWeekly);
                Assert.Equal(activitySchedule.StartDate, copiedActivitySchedule.StartDate);
                Assert.Equal(activitySchedule.EndDate, copiedActivitySchedule.EndDate);
                Assert.Equal(activitySchedule.DailyStartTime, copiedActivitySchedule.DailyStartTime);
                Assert.Equal(activitySchedule.DailyEndTime, copiedActivitySchedule.DailyEndTime);
                Assert.NotEqual(activitySchedule.ID, copiedActivitySchedule.ID);
                Assert.NotEqual(activitySchedule.CreateDate, copiedActivitySchedule.CreateDate);

                var scheduleInstances = activitySchedule.GetInstances(msgs: msgs, dc: dc);
                var copiedScheduleInstances = copiedActivitySchedule.GetInstances(msgs: msgs, dc: dc);

                Assert.Equal(scheduleInstances.Count, copiedScheduleInstances.Count);

                for (int i = 0; i < scheduleInstances.Count; i++)
                {
                    var instance = scheduleInstances[i];
                    var copiedInstance = copiedScheduleInstances[i];

                    Assert.Equal(instance.ActivityId, copiedInstance.ActivityId);
                    Assert.Equal(instance.ActivityGroupId, copiedInstance.ActivityGroupId);
                    Assert.Equal(instance.Day, copiedInstance.Day);
                    Assert.Equal(instance.StartDateTime, copiedInstance.StartDateTime);
                    Assert.Equal(instance.EndDateTime, copiedInstance.EndDateTime);
                    Assert.Equal(instance.InstanceTypeId, copiedInstance.InstanceTypeId);
                    Assert.NotEqual(instance.ID, copiedInstance.ID);
                    Assert.NotEqual(instance.CreateDate, copiedInstance.CreateDate);
                }
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void ActivityCopyPropertiesCheckTest()
        {
            try
            {
                // Arrange
                var package = PackageFactory.Create(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);
                var form = ActivityOwnerAccount.GetOrCreateDefaultForm(LoggedInUser, msgs, dc);
                var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
                AccountPropertyFactory.Create(activity, AccountPropertyType.RegistrationThankYouEmail, msgs, dc);
                AccountPropertyFactory.Create(activity, AccountPropertyType.ApplicationThankYouEmail, msgs, dc);
                AccountPropertyFactory.Create(activity, AccountPropertyType.InterestThankYouEmail, msgs, dc);

                // Act
                var expected = activity.Copy(newName: activity.Name + "- [copy]", useSameForm: false, loggedInUserId: LoggedInUser.ID.Value, msgs: msgs, dc: dc);

                // Assert
                var activityProperties = activity.GetActivityProperties(msgs, dc);
                var copiedActivityProperties = expected.GetActivityProperties(msgs, dc);

                Assert.Equal(activityProperties.Count, copiedActivityProperties.Count);

                for (int i = 0; i < activityProperties.Count; i++)
                {
                    var main = activityProperties[i];
                    var copied = copiedActivityProperties[i];

                    Assert.Equal(main.ActivityId, activity.ID);
                    Assert.Equal(copied.ActivityId, expected.ID);

                    Assert.Equal(main.AccountPropertyTypeId, copied.AccountPropertyTypeId);
                }
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void ActivityCopyFormsCheckTest()
        {
            try
            {
                // Arrange
                var package = PackageFactory.Create(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);
                var form = FormFactory.Create(ActivityOwnerAccount, msgs, dc);
                var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
                var activityForm = ActivityFormWebUserFactory.CreateActiveForm(ActivityOwnerAccount, activity, form, msgs, dc);

                // Act
                var expected = activity.Copy(newName: activity.Name + "- [copy]", useSameForm: true, loggedInUserId: LoggedInUser.ID.Value, msgs: msgs, dc: dc);

                // Assert
                var activeActivityForm = activity.GetActiveActivityForm(msgs, dc);
                var copiedActivityForm = expected.GetActiveActivityForm(msgs, dc);

                Assert.Equal(activeActivityForm.Name, activityForm.Name);
                Assert.Equal(activeActivityForm.FormId, activityForm.FormId);
                Assert.Equal(activeActivityForm.ActivityId, activityForm.ActivityId);
                Assert.Equal(activeActivityForm.AccountId, activityForm.AccountId);
                
                Assert.NotNull(copiedActivityForm);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        #endregion

        #region Remove Activity Schedule Test

        [Fact(DisplayName="RemoveActivityTest")]
        public void RemoveActivityTest()
        {
            // Arrange
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activityWithSchedule = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activityWithoutSchedule = ActivityFactory.Create(ActivityOwnerAccount, schedule, msgs, dc);
            var differentAccount = AccountFactory.Create(LoggedInUser, msgs, dc);
            var scheduleForActivityWithDifferentAccount = ActivityScheduleFactory.Create(differentAccount, msgs, dc);
            var activityWithDifferentAccount = ActivityFactory.Create(differentAccount,scheduleForActivityWithDifferentAccount, msgs, dc);

            // Act
            activityWithSchedule.RemoveActivitySchedule(LoggedInUser, ActivityOwnerAccount, msgs, dc);
            activityWithoutSchedule.RemoveActivitySchedule(LoggedInUser, ActivityOwnerAccount, msgs, dc);
            activityWithDifferentAccount.RemoveActivitySchedule(LoggedInUser, ActivityOwnerAccount, msgs, dc);

            // Assert
            Assert.False(activityWithSchedule.HasSchedule);
            Assert.True(msgs.HasErrors());
            Assert.Equal(expected: "Error Removing Schedule", actual: msgs[0].Text);
            Assert.True(activityWithDifferentAccount.HasSchedule);
        }

        #endregion

        [Fact(DisplayName="GetRegistrationAssignmentTypesTest")]
        public void GetRegistrationAssignmentTypesTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(ActivityOwnerAccount, ActivityType.Activity, msgs, dc);
            var permissionslip = ActivityFactory.Create(ActivityOwnerAccount, ActivityType.PermissionSlip, msgs, dc);

            // Act
            var activityAssignmentTypes = activity.GetRegistrationAssignmentTypes();
            var permissionAssignmentTypes = permissionslip.GetRegistrationAssignmentTypes();

            // Assert
            Assert.True(activityAssignmentTypes.IsNotEmpty());
            Assert.Contains(RegistrationAssignmentType.Applied, activityAssignmentTypes);
            Assert.Contains(RegistrationAssignmentType.WaitingList, activityAssignmentTypes);
            Assert.DoesNotContain(RegistrationAssignmentType.Withdrawn, activityAssignmentTypes);

            Assert.True(permissionAssignmentTypes.IsNotEmpty());
            Assert.DoesNotContain(RegistrationAssignmentType.Applied, permissionAssignmentTypes);
            Assert.DoesNotContain(RegistrationAssignmentType.WaitingList, permissionAssignmentTypes);
            Assert.DoesNotContain(RegistrationAssignmentType.Withdrawn, permissionAssignmentTypes);

        }
    }
}