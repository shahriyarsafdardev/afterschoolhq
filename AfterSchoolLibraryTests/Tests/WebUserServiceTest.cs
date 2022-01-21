using System;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class WebUserServiceTest : BaseTest
    {
        public WebUserServiceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CreateNewTest()
        {
            var newUser = WebUserService.CreateNew(
                            firstName: "jeff",
                            lastName: "gordon",
                            email: $"jeff+{DateTime.Now.Ticks}@example.com",
                            phone: $"317-555-{(DateTime.Now.Ticks % 10000):0000}",
                            password: null,
                            isActive: false,
                            isRegistered: false,
                            isVerified: false,
                            allowTextMessages: true,
                            loggedInUser: null,
                            msgs: msgs,
                            dc: dc);

            Assert.True(newUser.ID.HasValue);
        }

        [Fact]
        public void EncryptPasswordTest()
        {
            var password = "123123123";
            var hashedPassword = WebUserService.EncryptPassword(password);

            Assert.True(WebUserService.IsValidPassword(password, hashedPassword));

        }

        [Theory]
        [InlineData("test@test.com", "123123123", "123123123", true)]
        [InlineData("test@test.com", "123123123", "123412341", false)]
        public void LoginByEmailTest(string email, string setPassword, string testingPassword, bool expectedResult)
        {
            var user = WebUserFactory.CreateWithEmail(email, setPassword, msgs, dc);

            Assert.Equal(expectedResult, WebUserService.LoginByEmail(email, testingPassword, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void CanPublishActivityTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            var user2 = WebUserFactory.Create(msgs, dc);
            var account2 = AccountFactory.Create(user2, msgs, dc);
            var activity2 = ActivityFactory.Create(account2, msgs, dc);

            var user3 = WebUserFactory.Create(msgs, dc);
            var account3 = AccountFactory.Create(user3, msgs, dc);
            var activity3 = ActivityFactory.Create(account3, msgs, dc);

            var user4 = WebUserFactory.Create(msgs, dc);
            var account4 = AccountFactory.Create(user4, msgs, dc);
            var activity4 = ActivityFactory.Create(account4, msgs, dc);

            var user5 = WebUserFactory.Create(msgs, dc);
            var account5 = AccountFactory.Create(user5, msgs, dc);
            var activity5 = ActivityFactory.Create(account5, msgs, dc);

            var schoolUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            AccountRelationshipFactory.Create(schoolAccount, account2, ApprovalStatus.Approved, false, msgs, dc);
            AccountRelationshipFactory.Create(schoolAccount, account3, ApprovalStatus.Denied, false, msgs, dc);
            AccountRelationshipFactory.Create(schoolAccount, account4, ApprovalStatus.Approved, true, msgs, dc);

            activity.SetLocation(location, user, msgs, dc);
            activity2.SetLocation(location, user, msgs, dc);
            activity3.SetLocation(location, user, msgs, dc);
            activity4.SetLocation(location, user, msgs, dc);
            activity5.SetLocation(location, user, msgs, dc);

            var canUserPublish = user.CanPublishActivity(account, activity, msgs, dc);
            var canSchoolUserPublish = schoolUser.CanPublishActivity(schoolAccount, activity, msgs, dc);
            var canUser2Publish = user2.CanPublishActivity(account2, activity2, msgs, dc);
            var canUser3Publish = user3.CanPublishActivity(account3, activity3, msgs, dc);
            var canUser4Publish = user4.CanPublishActivity(account4, activity4, msgs, dc);
            var canUser5Publish = user5.CanPublishActivity(account5, activity5, msgs, dc);

            Assert.False(canUserPublish);
            Assert.True(canSchoolUserPublish);
            Assert.True(canUser2Publish);
            Assert.False(canUser3Publish);
            Assert.False(canUser4Publish);
            Assert.False(canUser5Publish);
        }

        [Fact]
        public void GetUserByCleverIdTest()
        {
            // Arrange
            var webUser = WebUserFactory.CreateWithCleverId(msgs, dc);

            // Act
            var userWithCleverIntegration = WebUserService.GetUserByCleverId(id: webUser.CleverUserId, msgs: msgs, dc: dc);

            // Assert
            Assert.NotNull(webUser);
            Assert.NotNull(userWithCleverIntegration);
            Assert.True(webUser.ID == userWithCleverIntegration.ID);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void SetCleverIdentityToUserTest1()
        {
            // Arrange
            var webUser = WebUserFactory.Create(msgs, dc);

            // Act
            WebUserService.SetCleverIdentityToUser(id: "1", name: "Clever test", webUser: webUser, msgs: msgs, dc: dc);

            // Assert
            Assert.NotNull(webUser);            
            Assert.True(webUser.CleverUserId == "1");
            Assert.True(webUser.CleverUserName == "Clever test");
            Assert.True(msgs.HasNoErrors());
            Assert.True(msgs.HasMessageContainingText($"Clever integration for {webUser.DisplayName} has been successfully completed."));
        }
    }
}
