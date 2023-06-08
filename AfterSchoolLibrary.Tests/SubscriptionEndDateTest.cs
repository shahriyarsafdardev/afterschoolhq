using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class SubscriptionEndDateTest
    {
        // These cases are for Not OnGoing Subscription only.
        
        [TestCase(10)]
        [TestCase(1)]
        public void CalculatePlanEndDateFromNumberOfPaymentsMonthly( int numberOfPeriodsForSubscription)
        {
            var startDate = new DateTime(2022, 09, 15, 0, 0, 0);
            
            var plan = new SubscriptionPlan()
            {
                BillingPeriodTypeId = BillingPeriodType.Month,
                IsOngoingSubscription = false,
                PlanTypeId = SubscriptionPlanType.Subscription,
                StartDate = startDate,
                EndDate = null,
            };
            var endDate = plan.CalculateEndDateFromNumberOfPeriods(startDate, numberOfPeriodsForSubscription);
            Assert.That(endDate, Is.EqualTo(startDate.AddMonths(numberOfPeriodsForSubscription)));
        }
        
        [TestCase(11)]
        [TestCase(8)]
        public void CalculatePlanEndDateFromNumberOfPaymentsWeekly( int numberOfPeriodsForSubscription)
        {
            var startDate = new DateTime(2022, 09, 15, 0, 0, 0);
            
            var plan = new SubscriptionPlan()
            {
                BillingPeriodTypeId = BillingPeriodType.Week,
                IsOngoingSubscription = false,
                PlanTypeId = SubscriptionPlanType.Subscription,
                StartDate = startDate,
                EndDate = null,
            };
            var endDate = plan.CalculateEndDateFromNumberOfPeriods(startDate, numberOfPeriodsForSubscription);
            Assert.That(endDate, Is.EqualTo(startDate.AddWeeks(numberOfPeriodsForSubscription)));
        }
        [TestCase(11)]
        [TestCase(16)]
        public void CalculatePlanEndDateFromNumberOfPaymentsBiWeekly( int numberOfPeriodsForSubscription)
        {
            var startDate = new DateTime(2022, 09, 15, 0, 0, 0);
            
            var plan = new SubscriptionPlan()
            {
                BillingPeriodTypeId = BillingPeriodType.TwoWeeks,
                IsOngoingSubscription = false,
                PlanTypeId = SubscriptionPlanType.Subscription,
                StartDate = startDate,
                EndDate = null,
            };
            var endDate = plan.CalculateEndDateFromNumberOfPeriods(startDate, numberOfPeriodsForSubscription);
            Assert.That(endDate, Is.EqualTo(startDate.AddBiWeeks(numberOfPeriodsForSubscription)));
        }
        [TestCase(11)]
        [TestCase(13)]
        public void CalculatePlanEndDateFromNumberOfPaymentsYearly( int numberOfPeriodsForSubscription)
        {
            var startDate = new DateTime(2022, 09, 15, 0, 0, 0);
            
            var plan = new SubscriptionPlan()
            {
                BillingPeriodTypeId = BillingPeriodType.Year,
                IsOngoingSubscription = false,
                PlanTypeId = SubscriptionPlanType.Subscription,
                StartDate = startDate,
                EndDate = null,
            };
            var endDate = plan.CalculateEndDateFromNumberOfPeriods(startDate, numberOfPeriodsForSubscription);
            Assert.That(endDate, Is.EqualTo(startDate.AddYears(numberOfPeriodsForSubscription)));
        }

        
    }
}