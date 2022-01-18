using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;

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
            ActivityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

            InvitedAccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            InvitedAccount = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);

            Assert.NotNull(AccountActivityGroup.Load(aag.ID.Value, msgs, dc));
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

            Assert.Null(AccountActivityGroup.Load(aag.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var aag = Factories.AccountActivityGroupFactory.Create(InvitedAccount, ActivityGroup, msgs, dc);

            Assert.Equal(aag.ID, aag.Reload(msgs, dc).ID);
        }
    }
}
