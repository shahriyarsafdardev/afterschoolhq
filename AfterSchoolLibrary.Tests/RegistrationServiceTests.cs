using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class RegistrationServiceTests
    {
        [TestCase("$email-th/is report!", "email-this report.csv")]
        [TestCase("new.report.csv", "new.report.csv")]
        [TestCase("numbers.csv", "numbers.csv")]
        [TestCase("attendance", "attendance.csv")]
        [TestCase("", "registrations.csv")]
        [TestCase("new.csv.report", "new.csv.report.csv")]
        public void CreateRegistrationsCsvFilenameTests(string input, string expected)
        {
            var actual = RegistrationService.CreateRegistrationsCsvFilename(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetRegistrationAssignmentTypeAppliedTest()
        {
            var result = GetRegistrationAssignmentType(RegistrationMode.Apply);
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.Applied));
        }

        [Test]
        public void GetRegistrationAssignmentTypeAlreadyReservedTest()
        {
            var result = GetRegistrationAssignmentType(existingAssignmentType: RegistrationAssignmentType.Reserved);
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.Reserved));
        }

        [Test]
        public void GetRegistrationAssignmentTypeReservedPendingParentalConsentTest()
        {
            var result = GetRegistrationAssignmentType(requiresParentalConsent: true);
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.ReservedPendingParentalConsent));
        }

        [Test]
        public void GetRegistrationAssignmentTypeReservedPendingPaymentTest()
        {
            var result = GetRegistrationAssignmentType(isFree: false);
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.ReservedPendingPayment));
        }

        [Test]
        public void GetRegistrationAssignmentTypeReservedTest()
        {
            var result = GetRegistrationAssignmentType();
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.Reserved));
        }

        [Test]
        public void GetRegistrationAssignmentTypeReservedForActivityGroupTest()
        {
            var result = GetRegistrationAssignmentType(participationObjectType: ParticipationObjectType.ActivityGroup,
                activityGroupType: ActivityGroupType.Session);
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.Reserved));
        }

        [Test]
        public void GetRegistrationAssignmentTypeWaitingListTest()
        {
            var result = GetRegistrationAssignmentType(seatsRemaining: 0);
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.WaitingList));
        }

        [Test]
        public void GetRegistrationAssignmentTypeReservedPendingBackgroundCheckTest()
        {
            var result = GetRegistrationAssignmentType(requiresBackgroundCheck: true);
            Assert.That(result, Is.EqualTo(RegistrationAssignmentType.ReservedPendingBackgroundCheck));
        }

        private static RegistrationAssignmentType GetRegistrationAssignmentType(RegistrationMode registrationMode = RegistrationMode.Register,
            RegistrationAssignmentType? existingAssignmentType = null, bool requiresParentalConsent = false, bool isFree = true,
            ParticipationObjectType participationObjectType = ParticipationObjectType.Activity, ActivityGroupType? activityGroupType = null,
            int seatsRemaining = 17, bool requiresBackgroundCheck = false)
        {
            var participationObject = new ParticipationObject(ActivityType.Activity)
            {
                IsFree = isFree,
                Object = activityGroupType.HasValue ? new ActivityGroup {ActivityGroupTypeId = activityGroupType.Value} : null,
                ObjectType = participationObjectType,
                PostPayPlans = Enumerable.Empty<SubscriptionPlan>().ToList(),
                RegistrationMode = registrationMode,
                RequiresParentalConsent = requiresParentalConsent,
                SeatsRemaining = seatsRemaining,
            };

            var masterRegistrant = new Registrant
            {
                WebUserId = 1234,
            };

            return RegistrationService.GetRegistrationAssignmentType(participationObject, masterRegistrant, 1, existingAssignmentType, null,
                masterRegistrant.WebUserId, requiresBackgroundCheck);
        }

        [Test]
        public void IsBackgroundCheckRequiredFalseForActivityTypesThatAreNotVolunteerOpportunitiesTest()
        {
            Assert.That(RegistrationService.IsBackgroundCheckRequired(ActivityType.Activity, 1234), Is.False);
        }

        [Test]
        public void IsBackgroundCheckRequiredFalseEvenForVolunteerOpportunitiesTest()
        {
            Assert.That(RegistrationService.IsBackgroundCheckRequired(ActivityType.VolunteerOpportunity, 1234), Is.False);
        }
    }
}