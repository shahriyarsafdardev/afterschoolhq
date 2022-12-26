using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests.ActivityServiceTests
{
    public class FinalizeStartAndEndDateForSubscriptionTests
    {
        [TestCase(BillingPeriodType.Month, 10)]
        [TestCase(BillingPeriodType.Week, 7)]
        public void FinalizeStartAndEndDateForSubscriptionTest(BillingPeriodType billingPeriodType, int expectedPaymentStartDayOfMonth)
        {
            var dayOfMonth = new DateTime(2022, 11, 10, 0, 0, 0);

            var schedule = new ActivitySchedule
            {
                ID = 4,
                StartDate = new DateTime(2022, 09, 27, 0, 0, 0),
                EndDate = new DateTime(2022, 09, 27, 0, 0, 0).AddDays(35),
                IsOngoing = false,
            };

            var activity = new Activity {ActivityScheduleId = schedule.ID};
            var upcomingInstance = new ActivityInstance {StartDateTime = dayOfMonth};

            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity.HasSchedule, billingPeriodType, DayOfWeek.Monday,
                dayOfMonth.Day, schedule, upcomingInstance, new DateTime(2022, 09, 27, 0, 0, 0));
            Assert.That(paymentStart, Is.EqualTo(new DateTime(2022, 11, expectedPaymentStartDayOfMonth, 0, 0, 0)));
            Assert.That(paymentEnd, Is.EqualTo(schedule.EndDate));
        }

        [Test]
        public void FinalizeStartAndEndDateForOngoingSubscription()
        {
            var schedule = new ActivitySchedule
            {
                ID = 1,
                IsOngoing = true,
                StartDate = new DateTime(2122, 10, 6, 0, 0, 0),
                EndDate = new DateTime(2132, 11, 21, 0, 0, 0),
            };

            var activity = new Activity {ActivityScheduleId = schedule.ID};

            var dayOfMonth = new DateTime(2122, 9, 1);
            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity.HasSchedule, BillingPeriodType.Month,
                DayOfWeek.Sunday, dayOfMonth.Day, schedule, null, new DateTime(2022, 9, 30));

            Assert.That(paymentStart, Is.EqualTo(new DateTime(2122, 10, 1, 0, 0, 0)));
            Assert.That(paymentEnd, Is.EqualTo(schedule.EndDate));
        }

        [TestCase(1, 7)]
        [TestCase(8, 14)]
        [TestCase(11, 14)]
        public void ShouldStartWeeklyBillingOnTheRightDayBasedOnActivityStartDate(int currentDayOfMonth, int expectedPaymentStartDayOfMonth)
        {
            var schedule = new ActivitySchedule
            {
                EndDate = new DateTime(2021, 12, 8), // Thursday
                ID = 1234,
            };

            var activity = new Activity
            {
                ActivityScheduleId = schedule.ID,
            };

            var upcomingInstance = new ActivityInstance
            {
                StartDateTime = new DateTime(2021, 11, 8), // Monday
            };

            var now = new DateTime(2021, 11, currentDayOfMonth, 0, 0, 0);
            var dayOfMonth = DateTime.Now.AddDays(1);
            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity.HasSchedule, BillingPeriodType.Week,
                DayOfWeek.Sunday, dayOfMonth.Day, schedule, upcomingInstance, now);

            Assert.That(paymentStart, Is.EqualTo(new DateTime(2021, 11, expectedPaymentStartDayOfMonth, 0, 0, 0)));
            Assert.That(paymentEnd, Is.EqualTo(schedule.EndDate));
        }

        [TestCase(1, 5)]
        [TestCase(5, 5)]
        [TestCase(6, 6)]
        public void ShouldStartMonthlyBillingOnTheRightDayBasedOnActivityStartDate(int currentDayOfMonth, int expectedPaymentStartDayOfMonth)
        {
            var schedule = new ActivitySchedule
            {
                EndDate = new DateTime(2021, 12, 31),
                ID = 1234,
            };

            var activity = new Activity
            {
                ActivityScheduleId = schedule.ID,
            };

            var upcomingInstance = new ActivityInstance
            {
                StartDateTime = new DateTime(2021, 11, 1),
            };

            var now = new DateTime(2021, 11, currentDayOfMonth, 0, 0, 0);
            var dayOfMonth = new DateTime(2021, 10, 5);
            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity.HasSchedule, BillingPeriodType.Month,
                DayOfWeek.Sunday, dayOfMonth.Day, schedule, upcomingInstance, now);

            Assert.That(paymentStart, Is.EqualTo(new DateTime(2021, 11, expectedPaymentStartDayOfMonth)));
            Assert.That(paymentEnd, Is.EqualTo(schedule.EndDate));
        }

        [Test]
        public void ShouldReturnNullForActivitiesWithoutSchedule()
        {
            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(false, BillingPeriodType.Month,
                DayOfWeek.Sunday, DateTime.Now.Day, null, null, DateTime.UtcNow);

            Assert.That(paymentStart, Is.Null);
            Assert.That(paymentEnd, Is.Null);
        }
    }
}