using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountRelationshipServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private WebUser LoggedInUser { get; set; }

        public AccountRelationshipServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            LoggedInUser = WebUserFactory.Create(msgs, dc);
        }

        [Fact]
        public void CreateNewTest()
        {
            var visitorOwner = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
            var hostOwner = WebUserFactory.Create(msgs, dc);
            var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);

            var accountRelationship = AccountRelationshipService.CreateNew(
                hostAccount, 
                visitorAccount, 
                ApprovalStatus.Approved, 
                HostPermissionType.Edit,
                true,
                false, 
                LoggedInUser, 
                msgs, 
                dc);

            Assert.True(accountRelationship.ID.HasValue);
        }

        [Fact]
        public void CreateNewNoDuplicateTest()
        {
            var visitorOwner = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
            var hostOwner = WebUserFactory.Create(msgs, dc);
            var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);

            var accountRelationship1 = AccountRelationshipService.CreateNew(
                hostAccount, 
                visitorAccount, 
                ApprovalStatus.Approved,
                HostPermissionType.Edit,
                true,
                false, 
                LoggedInUser, 
                msgs, 
                dc);

            var accountRelationship2 = AccountRelationshipService.CreateNew(
                hostAccount, 
                visitorAccount, 
                ApprovalStatus.Approved, 
                HostPermissionType.Edit,
                true,
                false,
                LoggedInUser, 
                msgs, 
                dc);

            Assert.Equal(accountRelationship1.ID.Value, accountRelationship2.ID.Value);
        }

        [Fact]
        public void UpdateRequiresPublishPermissionTest()
        {
            var visitorOwner = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
            var hostOwner = WebUserFactory.Create(msgs, dc);
            var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);

            msgs.AddRange(hostAccount.AddVisitorAccount(visitorAccount, HostPermissionType.ApproveOnly, ApprovalStatus.Approved, true, false, hostOwner, dc));

            var ar = visitorAccount.GetRequestForAccount(hostAccount, msgs, dc);

            ar.UpdateRequiresPublishPermission(false, hostOwner, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(ar);
            Assert.False(ar.RequiresPublishPermission);
        }

        [Fact]
        public void UpdateShowAllProgramsTest()
        {
            var visitorOwner = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
            var hostOwner = WebUserFactory.Create(msgs, dc);
            var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);

            msgs.AddRange(hostAccount.AddVisitorAccount(visitorAccount, HostPermissionType.ApproveOnly, ApprovalStatus.Approved, true, false, hostOwner, dc));

            var ar = visitorAccount.GetRequestForAccount(hostAccount, msgs, dc);

            ar.UpdateShowAllPrograms(true, hostOwner, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(ar);
            Assert.True(ar.RequiresPublishPermission);
        }

    }
}
