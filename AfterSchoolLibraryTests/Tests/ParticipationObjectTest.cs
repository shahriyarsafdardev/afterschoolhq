using System;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;

namespace AfterSchoolLibraryTests.Tests
{
    public class PartcipationObjectTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account ActivityOwnerAccount { get; }
        private Account PlanOwner { get; }

        public PartcipationObjectTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            ActivityOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
            PlanOwner = AccountFactory.CreateAccount(AccountOwner, msgs, dc);
        }

        [Fact]
        public void GetNumberOfClassesInCurrentCycleTest()
        {
            //var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            //var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(PlanOwner, ParticipationObjectType.Activity, 3008, 100, BillingPeriodType.Month,
            //PlanPaymentStartType.OnCheckout, null, null, true, true, false, false,
            //true, 1004, msgs, dc);

            //var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);

            //schedule.AccountId = 3008;
            //schedule.ID = 2027;
            //schedule.ActivityGroupId = 15;

            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount,activityGroup, DateTime.UtcNow.FirstDateOfMonth(), msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, location, msgs, dc);
            var instances = new List<ActivityInstance>();
            var firstDayOfMonth = DateTime.UtcNow.FirstDateOfMonth();
            var lastDateOfMonth = DateTime.UtcNow.LastDateOfMonth();
            var instance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            //while (i<lastDateOfMonth)
            //{

            //}
            for (var i= firstDayOfMonth ; i < lastDateOfMonth; i=i.AddDays(1))
            {
                instances.Add(Factories.ActivityInstanceFactory.Create(
                    activityGroup,
                    schedule,
                    i,
                    i.AddHours(2),
                    msgs,
                    dc));
            }
                
            var period = RegistrationPeriodFactory.Create(ActivityOwnerAccount, msgs, dc);
            var package = PackageFactory.Create(100,1200,msgs, dc);
            var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);
            //ParticipationObject participationObject = new ParticipationObject(plan.ParticipationObjectId.Value, ParticipationObjectType.Activity, msgs,dc)
            //{
            //    Schedule = schedule
            //};

            ParticipationObject participationObject = new ParticipationObject(activity, msgs, dc);
            var classes = participationObject.GetNumberOfClassesInCurrentCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());

        }
        
        [Fact]
        public void GetNumberOfRemainingClassesInNextCycleTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, activityGroup, DateTime.UtcNow.FirstDateOfMonth(), msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, location, msgs, dc);
            var instances = new List<ActivityInstance>();
            var firstDayOfMonth = DateTime.UtcNow.FirstDateOfMonth();
            var lastDateOfMonth = DateTime.UtcNow.LastDateOfMonth();
            var instance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
          
            for (var i = firstDayOfMonth; i < lastDateOfMonth; i = i.AddDays(1))
            {
                instances.Add(Factories.ActivityInstanceFactory.Create(
                    activityGroup,
                    schedule,
                    i,
                    i.AddHours(2),
                    msgs,
                    dc));
            }

            var period = RegistrationPeriodFactory.Create(ActivityOwnerAccount, msgs, dc);
            var package = PackageFactory.Create(100, 1200, msgs, dc);
            var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);

            ParticipationObject participationObject = new ParticipationObject(activity, msgs, dc);

            var classes = participationObject.GetNumberOfRemainingClassesInNextCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());

        }  
        
        [Fact]
        public void GetNumberOfRemainingClassesInCurrentCycleTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, activityGroup, DateTime.UtcNow.FirstDateOfMonth(), msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, location, msgs, dc);
            var instances = new List<ActivityInstance>();
            var firstDayOfMonth = DateTime.UtcNow.FirstDateOfMonth();
            var lastDateOfMonth = DateTime.UtcNow.LastDateOfMonth();
            var instance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            for (var i = firstDayOfMonth; i < lastDateOfMonth; i = i.AddDays(1))
            {
                instances.Add(Factories.ActivityInstanceFactory.Create(
                    activityGroup,
                    schedule,
                    i,
                    i.AddHours(2),
                    msgs,
                    dc));
            }

            var period = RegistrationPeriodFactory.Create(ActivityOwnerAccount, msgs, dc);
            var package = PackageFactory.Create(100, 1200, msgs, dc);
            var accountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, package, msgs, dc);

            ParticipationObject participationObject = new ParticipationObject(activity, msgs, dc);
            var classes = participationObject.GetNumberOfRemainingClassesInCurrentCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());

        }
    }
}

