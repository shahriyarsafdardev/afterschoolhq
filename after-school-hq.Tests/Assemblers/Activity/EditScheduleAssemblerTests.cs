using System;
using System.Collections.Generic;
using AfterSchool.Areas.Org.Assemblers.Activity;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers.Activity
{
    public class EditScheduleAssemblerTests
    {
        [Test]
        public void CreateDefaultScheduleForRegistrationPeriodTest()
        {
            var assembler = new EditScheduleAssembler();
            var activity = new AfterSchoolLibrary.Models.Activity
            {
                IsContinuousEnrollment = false,
                CostFrequencyTypeId = ActivityCostFrequencyType.PerActivity,
            };

            var registrationPeriod = new RegistrationPeriod
            {
                EndDateTime = new DateTime(2022, 6, 8),
            };
            var schedule = assembler.CreateDefaultScheduleForRegistrationPeriod(activity, registrationPeriod);

            Assert.That(schedule.StartDate, Is.EqualTo(new DateTime(2022, 6, 12)));
            Assert.That(schedule.EndDate, Is.EqualTo(new DateTime(2022, 8, 7)));
        }

        [Test]
        public void EditScheduleAssemblerTest()
        {
            var assembler = new EditScheduleAssembler();
            var activity = new AfterSchoolLibrary.Models.Activity();
            var msgs = new SystemMessages();
            var currentAccount = new Account();
            var fullScreenExitUrl = string.Empty;

            var schedule = new ActivitySchedule();
            var activeRegistrationPeriod = new RegistrationPeriod();
            var schedules = new List<ActivitySchedule>();
            var result = assembler.AssembleEditScheduleResult(schedule, activity, currentAccount, UserType.Instructor, activeRegistrationPeriod, false,
                schedules, "1", fullScreenExitUrl, msgs);

            Assert.That(result.ActiveMenu, Is.EqualTo("EditSchedule"));
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.Messages, Is.SameAs(msgs));
            Assert.That(result.Activity, Is.SameAs(activity));
        }
    }
}