using System.Collections.Generic;
using AfterSchool.Assemblers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers
{
    public class BrowseAssemblerTests
    {
        [Test]
        public void BrowseAssemblerTest()
        {
            var browseAssembler = new BrowseAssembler();

            var account = new Account();
            var accountProfile = new AccountProfile();
            var featuredProviders = new List<Account>();
            var grades = new List<GradeLevel>();
            var location = new Location{ ID = 1 };
            var locations = new List<Location>
            {
                location,
                new Location{ ID = 1 }
            };
            var expectedLocations = new List<Location>{ location };
            var registrationClosed = new List<ParticipationObject>();
            var registrationOpened = new List<ParticipationObject>();
            var registrationWillOpen = new List<ParticipationObject>();
            var session = new ParticipationObject(ActivityType.Activity);
            var systemMessages = new SystemMessages();
            var result = browseAssembler.AssembleBrowseResult(account, accountProfile, false, featuredProviders, grades, locations, registrationClosed,
                registrationOpened, registrationWillOpen, session, systemMessages);

            Assert.That(result.Account, Is.SameAs(account));
            Assert.That(result.AccountProfile, Is.SameAs(accountProfile));
            Assert.That(result.CleverSsoEnabled, Is.False);
            Assert.That(result.FeaturedProviders, Is.SameAs(featuredProviders));
            Assert.That(result.Grades, Is.SameAs(grades));
            Assert.That(result.Locations, Is.EqualTo(expectedLocations));
            Assert.That(result.RegistrationClosed, Is.SameAs(registrationClosed));
            Assert.That(result.RegistrationOpened, Is.SameAs(registrationOpened));
            Assert.That(result.RegistrationWillOpen, Is.SameAs(registrationWillOpen));
            Assert.That(result.Session, Is.SameAs(session));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
    }
}