using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountActivityGroupTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account ActivityGroupOwner { get; set; }
        private WebUser InvitedAccountOwner { get; set; }
        private Account InvitedAccount { get; set; }
        private ActivityGroup ActivityGroup { get; set; }

        public AccountActivityGroupTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            ActivityGroupOwner = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
            ActivityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);

            InvitedAccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            InvitedAccount = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);

            Assert.NotNull(AccountActivityGroup.Load(aag.ID.Value, msgs, dc));
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                AccountActivityGroup.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }
        
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);
            }

            Assert.Equal(numberToCreate, AccountActivityGroup.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);
            aag.InviteStatusId = InviteStatus.Accepted;
            msgs.AddRange(aag.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);

            msgs.AddRange(aag.Delete(dc));

            Assert.Equal(0, AccountActivityGroup.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void ReloadTest()
        {
            var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);

            Assert.Equal(aag.ID, aag.Reload(msgs, dc).ID);
        }
    }
}
