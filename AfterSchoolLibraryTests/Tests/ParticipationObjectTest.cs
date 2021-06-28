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

        private ActivityGroup ActivityGroup { get; }
        private ActivitySchedule Schedule { get; }
        private Activity Activity { get; }
        private List<ActivityInstance> Instances { get; }
        private Location Location { get; }
        private Package Package { get; }
        private AccountPackage AccountPackage { get; }        

        public PartcipationObjectTest(ITestOutputHelper output) : base(output)
        {            
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            ActivityOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
            PlanOwner = AccountFactory.CreateAccount(AccountOwner, msgs, dc);
            ActivityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount,ActivityGroupType.Session,20, msgs, dc);
            Schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, ActivityGroup, DateTime.UtcNow.FirstDateOfMonth(), msgs, dc);
            Location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            Activity = ActivityFactory.Create(ActivityOwnerAccount, Schedule, ActivityGroup, Location,20, msgs, dc);
            Instances = new List<ActivityInstance>();
            Package = PackageFactory.Create(100, 1200, msgs, dc);
            AccountPackage = AccountPackageFactory.Create(ActivityOwnerAccount, Package, msgs, dc);
            createInstances();
        }

        private void createInstances()
        {
            var firstDayOfMonth = DateTime.UtcNow.FirstDateOfMonth();
            var lastDateOfMonth = DateTime.UtcNow.AddMonths(2).LastDateOfMonth();
            for (var i = firstDayOfMonth; i < lastDateOfMonth; i = i.AddDays(1))
            {
                Instances.Add(Factories.ActivityInstanceFactory.Create(
                    ActivityGroup,
                    Schedule,
                    i,
                    i.AddHours(2),
                    msgs,
                    dc));
            }
        }

        [Fact]
        public void GetNumberOfClassesInCurrentCycleTest()
        {     
            
            var firstDayOfMonth = DateTime.UtcNow.FirstDateOfMonth();            
            var remainingDays = DateTime.UtcNow.LastDateOfMonth().Day - firstDayOfMonth.Day;

            ParticipationObject participationObject = new ParticipationObject(Activity, msgs, dc);
            var classes = participationObject.GetNumberOfClassesInCurrentCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());
            Assert.True(classes.Equals(remainingDays));

        }

        [Theory]
        [InlineData(20,0.5)]
        public void GetCheckoutCostWithConvenienceFeeUsingActivity(decimal costPerRegistrant, decimal convenienceFee)
        {
            ParticipationObject participationObject = new ParticipationObject(Activity, msgs, dc);
            
            var calculatedFee = participationObject.GetCheckoutCostWithConvenienceFee();
            var requiredFee = costPerRegistrant + convenienceFee;
            
            Assert.True(calculatedFee == requiredFee);
            
        }

        [Theory]
        [InlineData(20, 0.5)]
        public void GetCheckoutCostWithConvenienceFeebyUsingActivityGroup(decimal costPerRegistrant, decimal convenienceFee)
        {
            ParticipationObject participationObject = new ParticipationObject(ActivityGroup, msgs, dc);

            var calculatedFee = participationObject.GetCheckoutCostWithConvenienceFee();
            var requiredFee = costPerRegistrant + convenienceFee;

            Assert.True(calculatedFee == requiredFee);
        }

        [Fact]
        public void GetNumberOfRemainingClassesInNextCycleTest()
        {
            
            var remainingDays = DateTime.UtcNow.AddMonths(1).LastDateOfMonth().Day - DateTime.UtcNow.AddMonths(1).FirstDateOfMonth().Day;
            
            ParticipationObject participationObject = new ParticipationObject(Activity, msgs, dc);
            var classes = participationObject.GetNumberOfRemainingClassesInNextCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());
            Assert.True(classes.Equals(remainingDays));
        }  
        
        [Fact]
        public void GetNumberOfRemainingClassesInCurrentCycleTest()
        {
            var remainingDays = DateTime.UtcNow.LastDateOfMonth().Day - DateTime.UtcNow.Day;
            
            ParticipationObject participationObject = new ParticipationObject(Activity, msgs, dc);
            var classes = participationObject.GetNumberOfRemainingClassesInCurrentCycle(BillingPeriodType.Month, msgs, dc);
            
            Assert.True(classes.IsNotEmpty());
            Assert.True(classes.Equals(remainingDays));
        }
    }
}

