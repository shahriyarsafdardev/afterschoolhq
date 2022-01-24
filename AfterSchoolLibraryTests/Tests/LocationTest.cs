using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class LocationTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account HostAccount { get; set; }

        public LocationTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            HostAccount = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var location = LocationFactory.Create(HostAccount, msgs, dc);

            Assert.True(Location.Load(location.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var location = LocationFactory.Create(HostAccount, msgs, dc);

            location.LastModifiedBy = 1;
            msgs.AddRange(location.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var location = LocationFactory.Create(HostAccount, msgs, dc);

            msgs.AddRange(location.Delete(dc));

            Assert.Null(Location.Load(location.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var location = LocationFactory.Create(HostAccount, msgs, dc);

            Assert.Equal(location.ID, location.Reload(msgs, dc).ID);
        }
    }
}
