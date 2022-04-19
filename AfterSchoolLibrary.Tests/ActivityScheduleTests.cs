using System;
using System.Collections.Generic;
using System.Diagnostics;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class ActivityScheduleTests
    {
        [Test]
        public void CheckForOverlappingTimesInLargeListTest()
        {
            var stopwatch = Stopwatch.StartNew();
            var dateRanges = new List<DateRange>();
            var initialDate = new DateTime(2022, 4, 25, 15, 0, 0);
            for (var i = 0; i < 500; i++)
            {
                var startDate = initialDate.AddDays(7 * i);
                var endDate = startDate.AddHours(2);
                dateRanges.Add(new DateRange
                {
                    StartDate = startDate,
                    EndDate = endDate,
                });
            }

            var msgs = ActivitySchedule.CheckForOverlappingTimes(dateRanges.ToArray());
            stopwatch.Stop();
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, $"This test took {stopwatch.ElapsedMilliseconds} ms.");
            Assert.IsTrue(msgs.HasNoErrors(), msgs.RenderMessageList());
        }

        [Test]
        public void CheckForOverlappingTimesFailureTest()
        {
            var startDate1 = new DateTime(2022, 4, 25, 15, 0, 0);
            var endDate1 = startDate1.AddHours(2);
            var startDate2 = startDate1.AddHours(1);
            var endDate2 = startDate1.AddHours(3);
            var dateRanges = new List<DateRange>
            {
                new DateRange
                {
                    StartDate = startDate1,
                    EndDate = endDate1,
                },
                new DateRange
                {
                    StartDate = startDate2,
                    EndDate = endDate2,
                },
            };

            var msgs = ActivitySchedule.CheckForOverlappingTimes(dateRanges.ToArray());
            Assert.IsTrue(msgs.HasErrors());
            Assert.IsTrue(msgs.HasMessageContainingText("You have dates that overlap"));
        }
    }
}