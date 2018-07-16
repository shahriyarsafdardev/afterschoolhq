using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;

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
                accountId: LocationOwnerAccount.ID.Value,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc
                );

            Assert.NotNull(location.ID);
        }

        [Fact]
        public void DoesLocationAlreadyExistTest()
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
                accountId: LocationOwnerAccount.ID.Value,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc
                );

            var doesAlreadyExist = LocationService.DoesLocationAlreadyExist(
                locationName: locationName,
                address1: address1,
                address2: address2,
                city: city,
                state: state,
                postalCode: postalCode,
                msgs: msgs,
                dc: dc
                );

            Assert.True(doesAlreadyExist);
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
                accountId: LocationOwnerAccount.ID.Value,
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
    }
}
