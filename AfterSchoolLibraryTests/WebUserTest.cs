using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class WebUserTest : BaseTest
    {
        public WebUserTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ReloadTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var user2 = user.Reload(dc: dc);

            Assert.NotNull(user2);
        }

        [Fact]
        public void LoadByUidTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var user2 = WebUser.LoadByUid(user.Uid, dc: dc);

            Assert.NotNull(user2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LoadByUidIsActiveTest(bool isActive)
        {
            var user = Factories.WebUserFactory.Create(isActive, msgs, dc: dc);
            var user2 = WebUser.LoadByUidByIsActive(user.Uid, isActive, dc: dc);

            Assert.NotNull(user2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LoadByIsActiveTest(bool isActive)
        {
            var user = Factories.WebUserFactory.Create(isActive, msgs, dc: dc);
            var user2 = WebUser.LoadByIsActive(isActive, dc: dc);

            Assert.NotNull(user2);
        }

        [Fact]
        public void LoadByFacebookIdTest()
        {
            var testId = "testid";

            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            user.FacebookId = "testid";
            msgs.AddRange(user.Save(dc: dc));

            var user2 = WebUser.LoadByFacebookId(testId, dc: dc);

            Assert.NotNull(user2);
        }

        [Fact]
        public void GetInviteUrlTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var url = user.GetInviteUrl(account);

            Assert.NotEqual(0, url.Length);
        }

        [Fact]
        public void LoadByEmailTest()
        {
            var user = Factories.WebUserFactory.CreateWithEmail("test@test.com", msgs, dc: dc);
            var user2 = WebUser.LoadByEmail(user.Email, dc: dc);

            Assert.NotNull(user2);
        }

        [Theory]
        [InlineData("test1@test.com", "test2@test.com", "test2@test.com", false)]
        [InlineData("test1@test.com", "test2@test.com", "test3@test.com", true)]
        public void CanEmailBeUsedTest(string email1, string email2, string testEmail, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email1, msgs, dc: dc);
            var user2 = Factories.WebUserFactory.CreateWithEmail(email2, msgs, dc: dc);

            Assert.True(user.CanEmailBeUsed(testEmail, dc: dc) == result);
        }

        [Theory]
        [InlineData("test1@test.com", "test1@test.com", true)]
        [InlineData("test1@test.com", "test2@test.com", false)]
        public void ExistsByEmailTest(string email1, string testEmail, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email1, msgs, dc: dc);

            Assert.True(WebUser.ExistsByEmail(testEmail, dc: dc) == result);
        }

        [Fact]
        public void ExistsByUidPassingTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);

            Assert.True(WebUser.ExistsByUid(user.Uid, dc: dc));
        }

        [Fact]
        public void ExistsByUidFailingTest()
        {
            Assert.False(WebUser.ExistsByUid(Guid.NewGuid(), dc: dc));
        }

        [Fact]
        public void LoadByUsernameTest()
        {
            var user = Factories.WebUserFactory.CreateWithUsername("username1", msgs, dc: dc);

            Assert.NotNull(WebUser.LoadByUsername(user.Username, dc: dc));
        }

        [Theory]
        [InlineData("username1", "password1", "username1", "password1", true)]
        [InlineData("username1", "password1", "username2", "password1", false)]
        [InlineData("username1", "password1", "username1", "password2", false)]
        [InlineData("username1", "password1", "username2", "password2", false)]
        public void LoginByUsernameTest(string username, string password, string testUsername, string testPassword, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithUsername(username, password, msgs, dc: dc);

            Assert.True(WebUser.LoginByUsername(testUsername, testPassword, dc: dc).IsNotEmpty() == result);
        }

        [Theory]
        [InlineData("test1@test.com", "password1", "test1@test.com", "password1", true)]
        [InlineData("test1@test.com", "password1", "test2@test.com", "password1", false)]
        [InlineData("test1@test.com", "password1", "test1@test.com", "password2", false)]
        [InlineData("test1@test.com", "password1", "test2@test.com", "password2", false)]
        public void LoginByEmailTest(string email, string password, string testEmail, string testPassword, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, password, msgs, dc: dc);

            Assert.True(WebUser.LoginByEmail(testEmail, testPassword, dc: dc).IsNotEmpty() == result);
        }

        [Theory]
        [InlineData("password1")]
        public void EncryptPasswordTest(string password)
        {
            var passwordEncrypted = WebUserService.EncryptPassword(password);

            Assert.NotEqual(password, passwordEncrypted);
        }

        [Fact]
        public void DecryptPasswordTest()
        {
            var testPassword = "test2";
            var testPasswordEncrypted = WebUserService.EncryptPassword(testPassword);
            var testPasswordDecrypted = WebUserService.DecryptPassword(testPasswordEncrypted);

            Assert.Equal(testPassword, testPasswordDecrypted);
        }

        [Theory]
        [InlineData("test@test.com", true)]
        [InlineData("test@test.com@test.com", false)]
        public void SendInviteTest(string email, bool result)
        {
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);

            Assert.True(user.SendInvite(email, "invite message", user, account, dc: dc).HasNoErrors() == result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetAccountsTest(int numberOfAccounts)
        {
            var user = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            for (var i = 0; i < numberOfAccounts; i++)
            {
                var account = Factories.AccountFactory.Create(msgs, dc: dc);
                msgs.AddRange(account.AddUser(user, UserType.Administrator, user.ID.Value, dc: dc));
            }       

            Assert.Equal(numberOfAccounts, user.GetAccounts(dc: dc).Count);
        }

        [Theory]
        [InlineData(UserType.Administrator)]
        [InlineData(UserType.Parent)]
        public void GetUserTypeTest(UserType ut)
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var awu = Factories.AccountWebUserFactory.Create(account, user, ut, true, msgs, dc: dc);

            Assert.Equal(ut, user.GetUserType(account, dc: dc));
        }

        [Fact]
        public void UpdateLastActivityTest()
        {
            var testdate = new DateTime();
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            user.LastActivity = testdate;
            msgs.AddRange(user.Save(dc: dc));

            var pre = user.LastActivity;
            msgs.AddRange(user.UpdateLastActivity(dc: dc));
            var post = user.LastActivity;

            Assert.NotEqual(pre, post);
        }

        [Fact]
        public void UpdateLastLoginTest()
        {
            var testdate = new DateTime();
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            user.LastLogin = testdate;
            msgs.AddRange(user.Save(dc: dc));

            var pre = user.LastLogin;
            msgs.AddRange(user.UpdateLastLogin(dc: dc));
            var post = user.LastLogin;

            Assert.NotEqual(pre, post);
        }

        //cannot test because Feature is not fully implemeted always returns true currently
        [Fact]
        public void HasAcessToFeatureFalseTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);

            msgs.AddRange(account.AddUser(user, UserType.Parent, user.ID.Value, dc: dc));

            Assert.False(user.HasAccessToFeature(FeatureType.ManageForms, account, dc: dc));
        }

        [Fact]
        public void HasAcessToFeatureTrueTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);

            msgs.AddRange(account.AddUser(user, UserType.SuperAdmin, user.ID.Value, dc: dc));

            Assert.True(user.HasAccessToFeature(FeatureType.ViewInsights, account, dc: dc));
        }
        [Fact]
        public void GenerateUsernameTest()
        {
            var f = "John";
            var l = "Doe";
            var username = "JohnDoe";

            Assert.Equal(username, WebUserService.GenerateUsername(f, l, dc: dc));
        }

        [Theory]
        [InlineData("test@test.com", true)]
        [InlineData("test@test.com@test.com", false)]
        public void SendVerificationEmailTest(string email, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, msgs, dc: dc);
            Assert.True(user.SendVerificationEmail(true).HasNoErrors() == result);
        }

        [Fact]
        public void MarkAsVerifiedTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            msgs.AddRange(user.MarkAsVerified(dc: dc));

            Assert.True(user.VerifiedAccount);
        }

        [Theory]
        [InlineData("username1", "password1", "password2")]
        public void ChangePasswordTest(string username, string password, string password2)
        {
            var user = Factories.WebUserFactory.CreateWithUsername(username, password, msgs, dc: dc);
            msgs.AddRange(user.ChangePassword(password2, dc: dc));

            Assert.NotNull(WebUser.LoginByUsername(username, password2, dc: dc));
        }

        [Theory]
        [InlineData("test@test.com", true)]
        [InlineData("test@test.com@test.com", false)]
        public void SendPasswordResetTokenTest(string email, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, msgs, dc: dc);

            Assert.True(user.SendPasswordResetToken(true, dc: dc).HasNoErrors() == result);
        }

        [Theory]
        [InlineData(RegistrantType.Individual, false)]
        [InlineData(RegistrantType.Individual, true)]
        [InlineData(RegistrantType.Organization, true)]
        public void CreateRegistrantTest(RegistrantType rt, bool isMaster)
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var registrant = user.CreateRegistrant(rt, isMaster, msgs, dc: dc);

            Assert.NotNull(registrant);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void GetRegistrantsTest(int numberOfRegistrants)
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);

            for (var i = 0; i < numberOfRegistrants; i++)
            {
                user.CreateRegistrant(RegistrantType.Individual, true, msgs, dc: dc);
            }            

            Assert.Equal(numberOfRegistrants, user.GetRegistrants(dc: dc).Count);
        }

        [Fact]
        public void isActiveForTest()
        {
            var user = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var awu = Factories.AccountWebUserFactory.Create(account, user, UserType.ActivityLeader, true, msgs, dc: dc);
            
            Assert.True(user.IsActiveFor(account, dc: dc));
        }

        [Fact]
        public void GetActivityWebUserTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var awu = Factories.ActivityWebUserFactory.Create(activity, user, msgs, dc: dc);

            Assert.NotNull(user.GetActivityWebUser(activity, dc: dc));
        }

        [Fact]
        public void isActivityLeaderTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var awu = Factories.ActivityWebUserFactory.Create(activity, user, true, msgs, dc: dc);

            Assert.True(user.IsActivityLeader(activity, dc: dc));
        }

        [Fact]
        public void saltPasswordTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var awu = Factories.ActivityWebUserFactory.Create(activity, user, true, msgs, dc: dc);

            var orig = ASCIIEncoding.ASCII.GetBytes("test");
            var saltAndHashed = WebUserService.SaltandHashPassword("test");

            Assert.NotEqual(orig , WebUserService.SaltandHashPassword("test").hash);
        }
        /// <summary>
        /// Then decrypt their passwords using the old method, 
        /// then encrypt and store the new passwords in the db with the new 
        /// encryption method, then test that the passwords still work
        /// </summary>
        [Fact]
        public void FullPasswordTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var awu = Factories.ActivityWebUserFactory.Create(activity, user, true, msgs, dc: dc);

            var origInBytes = ASCIIEncoding.ASCII.GetBytes("test");
            var encrypt = AfterSchoolLibrary.Utilities.EncryptToBase64("test");
            var decrypt = AfterSchoolLibrary.Utilities.DecryptFromBase64("test");
            var saltAndHashed = WebUserService.SaltandHashPassword(decrypt);

            Assert.NotEqual(decrypt, saltAndHashed.ToString());
        }

    }
}
