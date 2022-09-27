using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class RegistrantServiceTests
    {
        private readonly Account billingAccount = new Account {TimeZone = "Eastern - EST"};
        private readonly DateTime nowInAccountTimeZone;

        private readonly ParticipationObject participationObject = new ParticipationObject(ActivityType.Activity)
        {
            StartDate = new DateTime(2122, 10, 6, 15, 0, 0),
        };

        private readonly SubscriptionPlan subscriptionPlan = new SubscriptionPlan
        {
            PlanTypeId = SubscriptionPlanType.Subscription,
            PaymentStartTypeId = PlanPaymentStartType.OnPeriodStart,
        };

        private readonly RegistrantSubscriptionPlan[] unexpiredCancelledPlans = { };

        public RegistrantServiceTests()
        {
            nowInAccountTimeZone = billingAccount.ConvertToAccountTimeZone(DateTime.UtcNow);
        }

        [Test]
        public void GetPlanStartDateShouldReturnSubscriptionStartDateIfItHasNotStartedYet()
        {
            subscriptionPlan.StartDate = new DateTime(2122, 10, 1, 0, 0, 0);
            participationObject.StartDate = new DateTime(2122, 10, 6, 15, 0, 0);

            var startDate = RegistrantService.GetPlanStartDate(subscriptionPlan, participationObject, billingAccount, unexpiredCancelledPlans);

            Assert.That(startDate, Is.EqualTo(subscriptionPlan.StartDate));
        }

        [Test]
        public void GetPlanStartDateShouldReturnNowForASubscriptionWithMissingStartDate()
        {
            subscriptionPlan.StartDate = null;
            participationObject.StartDate = new DateTime(2122, 10, 6, 15, 0, 0);

            var startDate = RegistrantService.GetPlanStartDate(subscriptionPlan, participationObject, billingAccount, unexpiredCancelledPlans);

            Assert.That(startDate, Is.EqualTo(nowInAccountTimeZone).Within(1).Seconds);
        }

        [Test]
        public void GetPlanStartDateShouldReturnNowForASubscriptionThatStartedInThePast()
        {
            subscriptionPlan.StartDate = new DateTime(2021, 10, 1, 0, 0, 0);
            participationObject.StartDate = new DateTime(2021, 10, 6, 15, 0, 0);

            var startDate = RegistrantService.GetPlanStartDate(subscriptionPlan, participationObject, billingAccount, unexpiredCancelledPlans);

            Assert.That(startDate, Is.EqualTo(nowInAccountTimeZone).Within(1).Seconds);
        }
    }
}