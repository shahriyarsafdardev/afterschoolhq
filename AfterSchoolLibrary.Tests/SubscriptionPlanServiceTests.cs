using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using Moq;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class SubscriptionPlanServiceTests
    {
        private readonly DateTime lastMonth = new DateTime(2022, 9, 10);
        private readonly ParticipationObject participationObject;
        private readonly DateTime startDateTime = new DateTime(2022, 10, 4);
        private readonly DateTime startOfPeriod = new DateTime(2022, 10, 1);
        private readonly Mock<IStripeProratingService> stripeProratingService = new Mock<IStripeProratingService>();
        private readonly DateTime today = new DateTime(2022, 10, 10);

        public SubscriptionPlanServiceTests()
        {
            participationObject = new ParticipationObject(ActivityType.Activity)
            {
                Schedule = new ActivitySchedule {ID = 3},
                StartDate = startDateTime,
            };
        }

        [Test]
        public void PaymentPlanProratingTest()
        {
            var plan = new SubscriptionPlan {PlanTypeId = SubscriptionPlanType.PaymentPlan};
            stripeProratingService.Setup(x => x.GetProratingValue()).Returns(2);

            var result = plan.FigureOutProrating(startOfPeriod, today, stripeProratingService.Object, participationObject, startDateTime);
            Assert.That(result.ShouldWeBeProratingToday, Is.False);
            Assert.That(result.ProratingValue, Is.Null);
            Assert.That(result.BackdateStartDate, Is.Null);
            Assert.That(result.BillingCycleAnchor, Is.Null);
        }

        [Test]
        public void OnPeriodStartMidPeriodTest()
        {
            /*
    On period start - (higher need for pro-rating)
    - Allow someone to start mid period? Yes
    - Pro-rating required
    - Use default stripe logic
    - Need to get stripe pro-rating value to display to customer and apply during checkout (https://stripe.com/docs/billing/subscriptions/prorations#preview-proration)
    - Would pro-rate if period has already started and the activity has already started
    - Activity page should show total cost per period
    - If after period start and after activity start, Checkout page should show pro-rated value --- ex. You'll pay {pro-rated amount} today and then {full amount} on {next period start date}
    - On activity Payment details screen - "If parent registers in the middle of a period, it will be pro-rated"
             */
            var plan = new SubscriptionPlan
            {
                BillingPeriodTypeId = BillingPeriodType.Month,
                PaymentStartTypeId = PlanPaymentStartType.OnPeriodStart,
                PlanTypeId = SubscriptionPlanType.Subscription,
            };

            stripeProratingService.Setup(x => x.GetProratingValue()).Returns(2);

            var result = plan.FigureOutProrating(startOfPeriod, today, stripeProratingService.Object, participationObject, startDateTime);
            Assert.That(result.ShouldWeBeProratingToday, Is.True);
            Assert.That(result.ProratingValue, Is.EqualTo(2));
            Assert.That(result.BackdateStartDate, Is.EqualTo(startOfPeriod));
            Assert.That(result.BillingCycleAnchor, Is.EqualTo(startOfPeriod));
        }

        [Test]
        public void OnPeriodStartEarlyTest()
        {
            var plan = new SubscriptionPlan
            {
                BillingPeriodTypeId = BillingPeriodType.Month,
                PaymentStartTypeId = PlanPaymentStartType.OnPeriodStart,
                PlanTypeId = SubscriptionPlanType.Subscription,
            };

            var result = plan.FigureOutProrating(startOfPeriod, lastMonth, stripeProratingService.Object, participationObject, startDateTime);
            Assert.That(result.ShouldWeBeProratingToday, Is.False);
            Assert.That(result.ProratingValue, Is.Null);
            Assert.That(result.BackdateStartDate, Is.Null);
            Assert.That(result.BillingCycleAnchor, Is.Null);
        }

        [Test]
        public void OnCheckoutTest()
        {
            var plan = new SubscriptionPlan
            {
                BillingPeriodTypeId = BillingPeriodType.Month,
                PaymentStartTypeId = PlanPaymentStartType.OnCheckout,
                PlanTypeId = SubscriptionPlanType.Subscription,
            };

            var result = plan.FigureOutProrating(startOfPeriod, today, stripeProratingService.Object, participationObject, startDateTime);
            Assert.That(result.ShouldWeBeProratingToday, Is.False);
            Assert.That(result.ProratingValue, Is.Null);
            Assert.That(result.BackdateStartDate, Is.Null);
            Assert.That(result.BillingCycleAnchor, Is.Null);
        }

        [Test]
        public void WhenActivityStartsEarlyTest()
        {
            var plan = new SubscriptionPlan
            {
                BillingPeriodTypeId = BillingPeriodType.Month,
                PaymentStartTypeId = PlanPaymentStartType.WhenActivityStarts,
                PlanTypeId = SubscriptionPlanType.Subscription,
            };

            var result = plan.FigureOutProrating(startOfPeriod, lastMonth, stripeProratingService.Object, participationObject, startDateTime);
            Assert.That(result.ShouldWeBeProratingToday, Is.False);
            Assert.That(result.ProratingValue, Is.Null);
            Assert.That(result.BackdateStartDate, Is.Null);
            Assert.That(result.BillingCycleAnchor, Is.Null);
        }

        [Test]
        public void WhenActivityStartsMidPeriodTest()
        {
            var plan = new SubscriptionPlan
            {
                BillingPeriodTypeId = BillingPeriodType.Month,
                PaymentStartTypeId = PlanPaymentStartType.WhenActivityStarts,
                PlanTypeId = SubscriptionPlanType.Subscription,
            };

            var result = plan.FigureOutProrating(startOfPeriod, today, stripeProratingService.Object, participationObject, startDateTime);
            Assert.That(result.ShouldWeBeProratingToday, Is.True);
            Assert.That(result.ProratingValue, Is.EqualTo(2));
            Assert.That(result.BackdateStartDate, Is.EqualTo(startDateTime));
            Assert.That(result.BillingCycleAnchor, Is.EqualTo(startOfPeriod));
        }
    }
}