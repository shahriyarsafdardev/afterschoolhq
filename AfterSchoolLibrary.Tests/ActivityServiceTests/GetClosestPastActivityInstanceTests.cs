using System;
using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests.ActivityServiceTests
{
    public class GetClosestPastActivityInstanceTests
    {
        [Test]
        public void GetClosestPastActivityInstanceTest()
        {
            var account = new Account {TimeZone = "Eastern - EST"};
            var instance1 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(-12)};
            var instance2 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(-5)};
            var instance3 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(2)};
            var instances = new List<ActivityInstance> {instance1, instance2, instance3};
            var result = ActivityService.GetClosestPastActivityInstance(instances, account);
            Assert.That(result, Is.SameAs(instance2));
        }

        [Test]
        public void GetClosestPastActivityInstanceWithOnlyActivitiesInTheFutureTest()
        {
            var account = new Account {TimeZone = "Eastern - EST"};
            var instance1 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(2)};
            var instance2 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(9)};
            var instance3 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(16)};
            var instances = new List<ActivityInstance> {instance1, instance2, instance3};
            var result = ActivityService.GetClosestPastActivityInstance(instances, account);
            Assert.That(result, Is.SameAs(instance1));
        }

        [Test]
        public void GetClosestActivityInstanceTest()
        {
            var account = new Account {TimeZone = "Eastern - EST"};
            var instance1 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(-12)};
            var instance2 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(-5)};
            var instance3 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(2)};
            var instances = new List<ActivityInstance> {instance1, instance2, instance3};
            var result = ActivityService.GetClosestActivityInstance(instances, account);
            Assert.That(result, Is.SameAs(instance3));
        }

        [Test]
        public void GetClosestActivityInstanceWithOnlyActivitiesInTheFutureTest()
        {
            var account = new Account {TimeZone = "Eastern - EST"};
            var instance1 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(2)};
            var instance2 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(9)};
            var instance3 = new ActivityInstance {StartDateTime = DateTime.UtcNow.AddDays(16)};
            var instances = new List<ActivityInstance> {instance1, instance2, instance3};
            var result = ActivityService.GetClosestActivityInstance(instances, account);
            Assert.That(result, Is.SameAs(instance1));
        }
    }
}