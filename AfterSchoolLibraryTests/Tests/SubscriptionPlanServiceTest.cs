using Xunit;
using Xunit.Abstractions;
using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class SubscriptionPlanServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account PlanOwner { get; set; }
        private Package Package { get; }
        
        public SubscriptionPlanServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.CreateFreePackage(msgs, dc);                         
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

        [Theory(DisplayName = "CheckAndCancelPaymentPlanTest")]
        [InlineData(2, 2, true)]
        [InlineData(2, 1, false)]
        [InlineData(2, 3, true)]
        public void CheckAndCancelPaymentPlanTest(int totalPeriods, int totalPayments, bool expectedResult)
        {
            var payerUser = WebUserFactory.Create(msgs, dc);
            var payerAccount = AccountFactory.Create(payerUser, msgs, dc);
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var paymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(
                                PlanOwner, 
                                activity, 
                                100, 
                                totalPeriods, 
                                msgs, 
                                dc);
            var purchase = PurchaseFactory.Create(payerAccount, 100, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registrantSubscriptionPlan = RegistrantSubscriptionPlanFactory.Create(registrant, paymentPlan, purchase, msgs, dc);
            var transaction = TransactionFactory.Create(PlanOwner, payerAccount, PaymentType.CreditCard, msgs, dc);

            for (var i = 0; i < totalPayments; i++)
                ActivityPaymentFactory.Create(PlanOwner, activity, registrantSubscriptionPlan, transaction, msgs, dc);

            Assert.Equal(expectedResult, paymentPlan.CheckAndCancelPlan(registrantSubscriptionPlan, PlanOwner, msgs, dc));
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
            var endDatePlanTypeNotSet = planTypeNotSet.GetPlanEndDate(activity.GetParticipationObject(msgs, dc), msgs);
            var endDatePlanTypePostPay = planTypeNotSet.GetPlanEndDate(activity.GetParticipationObject(msgs, dc), msgs);

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
            var endDateSchedule = planScheduleSubscription.GetPlanEndDate(activityWithSchedule.GetParticipationObject(msgs, dc), msgs);
            var endDateOnGoing = planOnGoingSubscription.GetPlanEndDate(onGoingActivity.GetParticipationObject(msgs, dc), msgs);


            // Assert
            Assert.True(endDateSchedule.HasValue);
            Assert.Equal(expected: schedule.EndDate?.AddDays(1).Date, actual: endDateSchedule.Value.Date);
            Assert.False(endDateOnGoing.HasValue);

        }


        [Fact(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodDayTest")]
        public void GetPlanEndDatePaymentPlanBillingPeriodDayTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var totalPeriods = 2;
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Day, msgs, dc);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDate(activity.GetParticipationObject(msgs, dc), msgs);            

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: DateTime.UtcNow.AddDays(totalPeriods).AddDays(-1).Date, actual: endDate.Value.Date);
        }

        [Fact(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodWeekTest")]
        public void GetPlanEndDatePaymentPlanBillingPeriodWeekTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var totalPeriods = 2;
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Week, msgs, dc);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDate(activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: DateTime.UtcNow.AddWeeks(totalPeriods).AddDays(-1).Date, actual: endDate.Value.Date);
        }

        [Fact(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodTwoWeekTest")]
        public void GetPlanEndDatePaymentPlanBillingPeriodTwoWeekTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var totalPeriods = 2;
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.TwoWeeks, msgs, dc);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDate(activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: DateTime.UtcNow.AddWeeks(2 * totalPeriods).AddDays(-1).Date, actual: endDate.Value.Date);
        }

        [Fact(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodMonthTest")]
        public void GetPlanEndDatePaymentPlanBillingPeriodMonthTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var totalPeriods = 2;
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Month, msgs, dc);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDate(activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: DateTime.UtcNow.AddMonths(totalPeriods).AddDays(-1).Date, actual: endDate.Value.Date);
        }

        [Fact(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodYearTest")]
        public void GetPlanEndDatePaymentPlanBillingPeriodYearTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var totalPeriods = 2;
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Year, msgs, dc);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDate(activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: DateTime.UtcNow.AddYears(totalPeriods).AddDays(-1).Date, actual: endDate.Value.Date);
        }
    }
}
