using System.Collections.Generic;
using System.Linq;
using AfterSchool.Areas.Org.Assemblers.Activity;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers
{
    public class RegistrantsAssemblerTests
    {
        [Test]
        public void ShouldFilterAndOrderRegistrantActivitiesWhenAssignmentTypeIsSet()
        {
            var registrantsAssembler = new RegistrantsAssembler();

            var systemMessages = new SystemMessages();
            var activity = new AfterSchoolLibrary.Models.Activity();
            var registrantActivities = new List<RegistrantActivity>
            {
                new RegistrantActivity {AssignmentTypeId = RegistrationAssignmentType.Applied},
                new RegistrantActivity {AssignmentTypeId = RegistrationAssignmentType.Invited, SeatIndex = 5},
                new RegistrantActivity {AssignmentTypeId = RegistrationAssignmentType.Invited, SeatIndex = 1},
            };
            var currentAccount = new Account();
            var currentUser = new WebUser();
            var assignmentType = RegistrationAssignmentType.Invited;
            var accountPackage = new AccountPackage();
            var activitySchedule = new ActivitySchedule();
            var result = registrantsAssembler.AssembleRegistrantsResult(activity, registrantActivities, currentAccount, currentUser, assignmentType,
                accountPackage, false, activitySchedule, UserType.Instructor, false, systemMessages);

            Assert.That(result.Activity, Is.SameAs(activity));
            Assert.That(result.RegistrantActivities, Has.Count.EqualTo(2));
            var firstRegistrantActivity = result.RegistrantActivities.First();
            Assert.That(firstRegistrantActivity.AssignmentTypeId, Is.EqualTo(RegistrationAssignmentType.Invited));
            Assert.That(firstRegistrantActivity.SeatIndex, Is.EqualTo(1));
            var lastRegistrantActivity = result.RegistrantActivities.Last();
            Assert.That(lastRegistrantActivity.AssignmentTypeId, Is.EqualTo(RegistrationAssignmentType.Invited));
            Assert.That(lastRegistrantActivity.SeatIndex, Is.EqualTo(5));
            Assert.That(result.CurrentAccount, Is.SameAs(currentAccount));
            Assert.That(result.CurrentUser, Is.SameAs(currentUser));
            Assert.That(result.AssignmentType, Is.EqualTo(assignmentType));
            Assert.That(result.AccountPackage, Is.SameAs(accountPackage));
            Assert.That(result.IsReadOnlySeatStatus, Is.False);
            Assert.That(result.IsActivityLeader, Is.False);
            Assert.That(result.ActivitySchedule, Is.SameAs(activitySchedule));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
        
        [Test]
        public void ShouldUseTheSameRegistrantActivitiesWhenAssignmentTypeIsNotSet()
        {
            var registrantsAssembler = new RegistrantsAssembler();

            var systemMessages = new SystemMessages();
            var activity = new AfterSchoolLibrary.Models.Activity();
            var registrantActivities = new List<RegistrantActivity>
            {
                new RegistrantActivity {AssignmentTypeId = RegistrationAssignmentType.Applied},
                new RegistrantActivity {AssignmentTypeId = RegistrationAssignmentType.Invited, SeatIndex = 5},
                new RegistrantActivity {AssignmentTypeId = RegistrationAssignmentType.Invited, SeatIndex = 1},
            };
            var currentAccount = new Account();
            var currentUser = new WebUser();
            var assignmentType = RegistrationAssignmentType.NotSet;
            var accountPackage = new AccountPackage();
            var activitySchedule = new ActivitySchedule();
            var result = registrantsAssembler.AssembleRegistrantsResult(activity, registrantActivities, currentAccount, currentUser, assignmentType,
                accountPackage, false, activitySchedule, UserType.Instructor, false, systemMessages);

            Assert.That(result.Activity, Is.SameAs(activity));
            Assert.That(result.RegistrantActivities, Is.SameAs(registrantActivities));
            Assert.That(result.CurrentAccount, Is.SameAs(currentAccount));
            Assert.That(result.CurrentUser, Is.SameAs(currentUser));
            Assert.That(result.AssignmentType, Is.EqualTo(assignmentType));
            Assert.That(result.AccountPackage, Is.SameAs(accountPackage));
            Assert.That(result.IsReadOnlySeatStatus, Is.False);
            Assert.That(result.IsActivityLeader, Is.False);
            Assert.That(result.ActivitySchedule, Is.SameAs(activitySchedule));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
    }
}