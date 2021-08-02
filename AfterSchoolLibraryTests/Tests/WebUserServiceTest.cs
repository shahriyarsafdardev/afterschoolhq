using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary;
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
                            email: "jeff@gordon.com",
                            phone: "317-224-3123",
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

            var schoolUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.SetLocation(location, user, msgs, dc);

            var canUserPublish = user.CanPublishActivity(account, activity, msgs, dc);
            var canSchoolUserPublish = schoolUser.CanPublishActivity(schoolAccount, activity, msgs, dc);

            Assert.False(canUserPublish);
            Assert.True(canSchoolUserPublish);
        }
    }
}
