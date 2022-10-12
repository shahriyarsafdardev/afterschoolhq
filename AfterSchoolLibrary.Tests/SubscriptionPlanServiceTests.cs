using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Moq;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class SubscriptionPlanServiceTests
    {
        private readonly Account account = new Account();
        private readonly DateTime lastMonth = new DateTime(2022, 9, 10);
        private readonly SystemMessages msgs = new SystemMessages();
        private readonly ParticipationObject participationObject;
        private readonly SubscriptionPlan plan = new SubscriptionPlan {StartDate = new DateTime(2022, 10, 1)};
        private readonly DateTime startDateTime = new DateTime(2022, 10, 4);
        private readonly Mock<IStripeProratingService> stripeProratingService = new Mock<IStripeProratingService>();
        private readonly DateTime today = new DateTime(2022, 10, 10);

        public SubscriptionPlanServiceTests()
        {
            participationObject = new ParticipationObject(ActivityType.Activity)
            {
                Schedule = new ActivitySchedule {ID = 3},
                StartDate = startDateTime,
            };
            stripeProratingService.Setup(x => x.GetProratingValue(account, plan, msgs)).Returns(2);
        }

        [Test]
        public void PaymentPlanProratingTest()
        {
            plan.PlanTypeId = SubscriptionPlanType.PaymentPlan;

            var result = plan.FigureOutProrating(today, stripeProratingService.Object, participationObject, startDateTime, account, msgs);
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
            plan.BillingPeriodTypeId = BillingPeriodType.Month;
            plan.PaymentStartTypeId = PlanPaymentStartType.OnPeriodStart;
            plan.PlanTypeId = SubscriptionPlanType.Subscription;

            var result = plan.FigureOutProrating(today, stripeProratingService.Object, participationObject, startDateTime, account, msgs);
            Assert.That(result.ShouldWeBeProratingToday, Is.True);
            Assert.That(result.ProratingValue, Is.EqualTo(2));
            Assert.That(result.BackdateStartDate, Is.EqualTo(today));
            Assert.That(result.BillingCycleAnchor, Is.EqualTo(plan.StartDate.Value));
        }

        [Test]
        public void OnPeriodStartEarlyTest()
        {
            plan.BillingPeriodTypeId = BillingPeriodType.Month;
            plan.PaymentStartTypeId = PlanPaymentStartType.OnPeriodStart;
            plan.PlanTypeId = SubscriptionPlanType.Subscription;

            var result = plan.FigureOutProrating(lastMonth, stripeProratingService.Object, participationObject, startDateTime, account, msgs);
            Assert.That(result.ShouldWeBeProratingToday, Is.False);
            Assert.That(result.ProratingValue, Is.Null);
            Assert.That(result.BackdateStartDate, Is.Null);
            Assert.That(result.BillingCycleAnchor, Is.Null);
        }

        [Test]
        public void OnCheckoutTest()
        {
            plan.BillingPeriodTypeId = BillingPeriodType.Month;
            plan.PaymentStartTypeId = PlanPaymentStartType.OnCheckout;
            plan.PlanTypeId = SubscriptionPlanType.Subscription;

            var result = plan.FigureOutProrating(today, stripeProratingService.Object, participationObject, startDateTime, account, msgs);
            Assert.That(result.ShouldWeBeProratingToday, Is.False);
            Assert.That(result.ProratingValue, Is.Null);
            Assert.That(result.BackdateStartDate, Is.Null);
            Assert.That(result.BillingCycleAnchor, Is.Null);
        }

        [Test]
        public void WhenActivityStartsEarlyTest()
        {
            plan.BillingPeriodTypeId = BillingPeriodType.Month;
            plan.PaymentStartTypeId = PlanPaymentStartType.WhenActivityStarts;
            plan.PlanTypeId = SubscriptionPlanType.Subscription;

            var result = plan.FigureOutProrating(lastMonth, stripeProratingService.Object, participationObject, startDateTime, account, msgs);
            Assert.That(result.ShouldWeBeProratingToday, Is.False);
            Assert.That(result.ProratingValue, Is.Null);
            Assert.That(result.BackdateStartDate, Is.Null);
            Assert.That(result.BillingCycleAnchor, Is.Null);
        }

        [Test]
        public void WhenActivityStartsMidPeriodTest()
        {
            plan.BillingPeriodTypeId = BillingPeriodType.Month;
            plan.PaymentStartTypeId = PlanPaymentStartType.WhenActivityStarts;
            plan.PlanTypeId = SubscriptionPlanType.Subscription;

            var result = plan.FigureOutProrating(today, stripeProratingService.Object, participationObject, startDateTime, account, msgs);
            Assert.That(result.ShouldWeBeProratingToday, Is.True);
            Assert.That(result.ProratingValue, Is.EqualTo(2));
            Assert.That(result.BackdateStartDate, Is.EqualTo(today));
            Assert.That(result.BillingCycleAnchor, Is.EqualTo(participationObject.StartDate));
        }
    }
}