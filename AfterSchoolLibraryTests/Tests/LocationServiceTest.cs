using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace AfterSchoolLibraryTests.Tests
{
    public class LocationServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account LocationOwnerAccount { get; set; }

        public LocationServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            LocationOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void CreateNewTest()
        {
            var location = LocationService.CreateNew(
                locationName: "School 70",
                address1: "1234 N Whatever Street",
                address2: "Suite 1900",
                city: "Indianapolis",
                state: "IN",
                postalCode: "12345",
                country: "US",
                locationType: LocationType.School,
                hostAccountId: LocationOwnerAccount.ID,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc
                );

            Assert.NotNull(location.ID);
        }

        [Theory]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US")]
        public void CleanUpDuplicateLocationsTest(string name, string address1, string address2, string city, string state, string postalCode, string country)
        {
            for (var i = 0; i < 4; i++)
            {
                var location = LocationService.CreateNew(
                  locationName: name,
                  address1: address1,
                  address2: address2,
                  city: city,
                  state: state,
                  postalCode: postalCode,
                  country: country,
                  locationType: LocationType.NotSet,
                  hostAccountId: LocationOwnerAccount.ID.Value,
                  loggedInUser: LoggedInUser,
                  msgs: msgs,
                  dc: dc);
            }

            LocationService.CleanUpDuplicateLocations(msgs, dc);

            Assert.Single(Location.Load(msgs: msgs, dc: dc));
        }

        [Theory]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US")]
        public void CleanUpDuplicateLocationsRegistrantTest(string name, string address1, string address2, string city, string state, string postalCode, string country)
        {
            var locations = new List<Location>();

            for (var i = 0; i < 4; i++)
            {
                var location = LocationService.CreateNew(
                  locationName: name,
                  address1: address1,
                  address2: address2,
                  city: city,
                  state: state,
                  postalCode: postalCode,
                  country: country,
                  locationType: LocationType.NotSet,
                  hostAccountId: LocationOwnerAccount.ID.Value,
                  loggedInUser: LoggedInUser,
                  msgs: msgs,
                  dc: dc);

                var registrant = RegistrantFactory.Create(msgs, dc);
                registrant.SchoolLocationId = location.ID.Value;
                msgs.AddRange(registrant.Save(dc));

                locations.Add(location);
            }

            LocationService.CleanUpDuplicateLocations(msgs, dc);

            var registrantsAfterCleanUp = Registrant.Load(criterias: new Hashtable() { { "SchoolLocationId", locations.First().ID.Value } }, msgs: msgs, dc: dc);

            Assert.Equal(4, registrantsAfterCleanUp.Count);
        }

        [Theory]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US")]
        public void CleanUpDuplicateLocationsAccountTest(string name, string address1, string address2, string city, string state, string postalCode, string country)
        {
            var locations = new List<Location>();

            for (var i = 0; i < 4; i++)
            {
                var newUser = WebUserFactory.Create(msgs, dc);
                var newAccount = AccountFactory.Create(newUser, msgs, dc);

                var location = LocationService.CreateNew(
                                  locationName: name,
                                  address1: address1,
                                  address2: address2,
                                  city: city,
                                  state: state,
                                  postalCode: postalCode,
                                  country: country,
                                  locationType: LocationType.NotSet,
                                  hostAccountId: LocationOwnerAccount.ID.Value,
                                  loggedInUser: LoggedInUser,
                                  msgs: msgs,
                                  dc: dc);
                
                newAccount.SetDefaultLocation(location, LoggedInUser, msgs, dc);

                locations.Add(location);
            }

            LocationService.CleanUpDuplicateLocations(msgs, dc);

            var accountsAfterCleanUp = Account.Load(criterias: new Hashtable() { { "DefaultLocationId", locations.First().ID.Value } }, msgs: msgs, dc: dc);

            Assert.Equal(4, accountsAfterCleanUp.Count);
        }

        [Theory]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US")]
        public void CleanUpDuplicateLocationsActivityTest(string name, string address1, string address2, string city, string state, string postalCode, string country)
        {
            var locations = new List<Location>();

            for (var i = 0; i < 4; i++)
            {
                var newUser = WebUserFactory.Create(msgs, dc);
                var newOwnerAccount = AccountFactory.Create(newUser, msgs, dc);

                newOwnerAccount.Name = name;
                msgs.AddRange(newOwnerAccount.Save(dc));

                var location = LocationService.CreateNew(
                  locationName: name,
                  address1: address1,
                  address2: address2,
                  city: city,
                  state: state,
                  postalCode: postalCode,
                  country: country,
                  locationType: LocationType.NotSet,
                  hostAccountId: LocationOwnerAccount.ID.Value,
                  loggedInUser: LoggedInUser,
                  msgs: msgs,
                  dc: dc);

                var activity = ActivityFactory.Create(newOwnerAccount, msgs, dc);
                activity.LocationId = location.ID;
                msgs.AddRange(activity.Save(dc));

                locations.Add(location);
            }

            LocationService.CleanUpDuplicateLocations(msgs, dc);

            var activitiesAfterCleanUp = Activity.Load(criterias: new Hashtable() { { "LocationId", locations.First().ID.Value } }, msgs: msgs, dc: dc);

            Assert.Equal(4, activitiesAfterCleanUp.Count);
        }

        [Theory]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US", "School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US", true)]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US", "School 70", "1234 N Whatever Street", " Suite 1900 ", "Indianapolis", "IN", "12345", "US", true)]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US", "School 70", "1234 N Whatever Street", " Suite 1900 ", "Indianapolis", "IN", "12345", "USA", false)]
        [InlineData("School 70", "1234 N Whatever Street", null, "Indianapolis", "IN", "12345", "US", "School 70", "1234 N Whatever Street", "", "Indianapolis", "IN", "12345", "USA", false)]
        [InlineData("School 70", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US", "School 700", "1234 N Whatever Street", "Suite 1900", "Indianapolis", "IN", "12345", "US", false)]
        public void DoesLocationAlreadyExistTest(string name, string address1, string address2, string city, string state, string postalCode, string country, string otherName, string otherAddress1, string otherAddress2, string otherCity, string otherState, string otherPostalCode, string otherCountry, bool expectedResult)
        {
            var location = LocationService.CreateNew(
                locationName: name,
                address1: address1,
                address2: address2,
                city: city,
                state: state,
                postalCode: postalCode,
                country: country,
                locationType: LocationType.NotSet,
                hostAccountId: LocationOwnerAccount.ID.Value,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc
                );

            var doesAlreadyExist = LocationService.DoesLocationAlreadyExist(
                locationName: otherName,
                address1: otherAddress1,
                address2: otherAddress2,
                city: otherCity,
                state: otherState,
                postalCode: otherPostalCode,
                country: otherCountry,
                msgs: msgs,
                dc: dc
                );

            Assert.Equal(expectedResult, doesAlreadyExist);
        }

        [Fact]
        public void LoadByAddressTest()
        {
            var locationName = "School 70";
            var address1 = "1234 N Whatever Street";
            var address2 = "Suite 1900";
            var city = "Indianapolis";
            var state = "IN";
            var postalCode = "12345";

            var location = LocationService.CreateNew(
                locationName: locationName,
                address1: address1,
                address2: address2,
                city: city,
                state: state,
                postalCode: postalCode,
                country: "US",
                locationType: LocationType.School,
                hostAccountId: LocationOwnerAccount.ID.Value,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc
                );

            var locationLoad = LocationService.LoadByAddress(
                locationName: locationName,
                address1: address1,
                address2: address2,
                city: city,
                state: state,
                postalCode: postalCode,
                msgs: msgs,
                dc: dc
                );

            Assert.NotNull(locationLoad);
        }

        [Fact(DisplayName = "GetHostAccount Test")]
        public void GetHostAccountTest()
        {
            try
            {
                // Arrange
                var location = LocationService.CreateNew(
                    locationName: "School 101",
                    address1: "1234 N Whatever Street",
                    address2: "Suite 1900",
                    city: "Indianapolis",
                    state: "IN",
                    postalCode: "12345",
                    country: "US",
                    locationType: LocationType.School,
                    hostAccountId: LocationOwnerAccount.ID,
                    loggedInUser: LoggedInUser,
                    msgs: msgs,
                    dc: dc
                );

                // Act
                var hostAccount = location.GetHostAccount(msgs, dc);

                // Assert
                Assert.NotNull(hostAccount);
                Assert.Equal(hostAccount.ID, LocationOwnerAccount.ID);
                Assert.Equal(hostAccount.Name, LocationOwnerAccount.Name);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetHostAccount Reverse Test")]
        public void GetHostAccountWithEmptyHostTest()
        {
            try
            {
                // Arrange
                var location = LocationService.CreateNew(
                    locationName: "School 101",
                    address1: "1234 N Whatever Street",
                    address2: "Suite 1900",
                    city: "Indianapolis",
                    state: "IN",
                    postalCode: "12345",
                    country: "US",
                    locationType: LocationType.School,
                    hostAccountId: null,
                    loggedInUser: LoggedInUser,
                    msgs: msgs,
                    dc: dc
                );

                // Act
                var hostAccount = location.GetHostAccount(msgs, dc);

                // Assert
                Assert.Null(hostAccount);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetHostAccount Empty Location Test")]
        public void GetHostAccountWithEmptyLocationTest()
        {
            try
            {
                // Arrange & Act
                var hostAccount = LocationService.GetHostAccount(location: null, msgs, dc);

                // Assert
                Assert.Null(hostAccount);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
