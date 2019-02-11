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

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var location = LocationFactory.Create(HostAccount, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                Location.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var location = LocationFactory.Create(HostAccount, msgs, dc);
            }

            Assert.Equal(numberToCreate, Location.GetCount(msgs: msgs, dc: dc));
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

            Assert.Equal(0, Location.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void ReloadTest()
        {
            var location = LocationFactory.Create(HostAccount, msgs, dc);

            Assert.Equal(location.ID, location.Reload(msgs, dc).ID);
        }
    }
}
