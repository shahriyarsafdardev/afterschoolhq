using Xunit;
using Xunit.Abstractions;
using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Billing;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class SubscriptionPlanServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account PlanOwner { get; set; }
        private Package Package { get; set; }

        private DateTime Now { get; set; }
        
        public SubscriptionPlanServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.CreateFreePackage(msgs, dc);
            Now = DateTime.UtcNow;
        }

        [Fact]
        public void CreateNewActivityObjectTest()
        {
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(
                        PlanOwner,
                        ParticipationObjectType.Activity,
                        activity.ID.Value,
                        35.ToDecimal(),
                        BillingPeriodType.Month,
                        true,
                        false,
                        false,
                        false,
                        LoggedInUser.ID.Value,
                        msgs,
                        dc);

            Assert.True(plan.ID.HasValue);
        }

        [Fact(DisplayName = "Create new Activity Group")]
        public void CreateNewActivityGroupTest()
        {
            var activityGroup = ActivityGroupFactory.Create(PlanOwner, ActivityGroupType.Session, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(
                        PlanOwner,
                        ParticipationObjectType.ActivityGroup,
                        activityGroup.ID.Value,
                        35.ToDecimal(),
                        BillingPeriodType.Month,
                        true,
                        false,
                        false,
                        false,
                        LoggedInUser.ID.Value,
                        msgs,
                        dc);

            Assert.True(plan.ID.HasValue);
        }

        [Fact]
        public void CreateNewSubscriptionPlanTest()
        {
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);            
            var accountPackage = AccountPackageFactory.Create(PlanOwner, Package, msgs, dc);
         
            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(
                               PlanOwner, 
                               ParticipationObjectType.Activity,
                               activity.ID.Value,
                               100, 
                               BillingPeriodType.Month,
                               PlanPaymentStartType.OnCheckout, 
                               null, 
                               null, 
                               true, 
                               true, 
                               false,
                               false,
                               true,
                               LoggedInUser.ID.Value,
                               msgs, 
                               dc);

            Assert.True(plan.ID.HasValue);
            Assert.True(msgs.HasNoErrors());
        }


        [Fact(DisplayName = "GetPlanEndDateNotSetPostPayTest")]
        public void GetPlanEndDateNotSetPostPayTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planTypeNotSet = SubscriptionPlanFactory.Create(PlanOwner, activity, SubscriptionPlanType.NotSet, msgs, dc);
            var planTypePostPay = SubscriptionPlanFactory.Create(PlanOwner, activity, SubscriptionPlanType.PostPayPlan, msgs, dc);

            // Act
            var endDatePlanTypeNotSet = planTypeNotSet.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);
            var endDatePlanTypePostPay = planTypeNotSet.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.False(endDatePlanTypeNotSet.HasValue);
            Assert.False(endDatePlanTypePostPay.HasValue);
        }

        [Fact(DisplayName ="GetPlanEndDateSubscriptionTest")]
        public void GetPlanEndDateSubscriptionTest()
        {
            // Arrange
            var schedule = ActivityScheduleFactory.Create(PlanOwner, msgs, dc);
            var activityWithSchedule = ActivityFactory.Create(PlanOwner, schedule, msgs, dc);
            var onGoingActivity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planScheduleSubscription = SubscriptionPlanFactory.Create(PlanOwner, activityWithSchedule, SubscriptionPlanType.Subscription, msgs, dc);
            var planOnGoingSubscription = SubscriptionPlanFactory.Create(PlanOwner, onGoingActivity, SubscriptionPlanType.Subscription, msgs, dc);

            // Act
            var endDateSchedule = planScheduleSubscription.GetPlanEndDateForRegistrantSubscription(Now, activityWithSchedule.GetParticipationObject(msgs, dc), msgs);
            var endDateOnGoing = planOnGoingSubscription.GetPlanEndDateForRegistrantSubscription(Now, onGoingActivity.GetParticipationObject(msgs, dc), msgs);


            // Assert
            Assert.True(endDateSchedule.HasValue);
            Assert.Equal(expected: schedule.EndDate?.AddDays(1).Date, actual: endDateSchedule.Value.Date);
            Assert.False(endDateOnGoing.HasValue);

        }


        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodDayTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodDayTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Day, msgs, dc);
            var expected = Now.Date.AddDays(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);            

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodWeekTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodWeekTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Week, msgs, dc);
            var expected = Now.Date.AddWeeks(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }        

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodTwoWeekTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodTwoWeekTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.TwoWeeks, msgs, dc);
            var expected = Now.Date.AddWeeks(totalPeriods * 2);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodMonthTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodMonthTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Month, msgs, dc);
            var expected = Now.Date.AddMonths(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodYearTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodYearTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Year, msgs, dc);
            var expected = Now.Date.AddYears(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }
    }
}
