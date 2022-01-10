using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Tests
{
    public class WebUserTest : BaseTest
    {
        public WebUserTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(DisplayName = "Reloads user from database")]
        public void ReloadTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var user2 = user.Reload(dc: dc);

            Assert.NotNull(user2);
        }

        [Fact(DisplayName = "Loads by uid")]
        public void LoadByUidTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var user2 = WebUser.LoadByUid(user.Uid, dc: dc);

            Assert.NotNull(user2);
        }

        [Theory(DisplayName = "Loads by uid and isactive")]
        [InlineData(false)]
        [InlineData(true)]
        public void LoadByUidIsActiveTest(bool isActive)
        {
            var user = Factories.WebUserFactory.Create(isActive, msgs, dc: dc);
            var user2 = WebUser.LoadByUidByIsActive(user.Uid, isActive, dc: dc);

            Assert.NotNull(user2);
        }

        [Theory(DisplayName = "Loads all users by isactive")]
        [InlineData(false)]
        [InlineData(true)]
        public void LoadByIsActiveTest(bool isActive)
        {
            var user = Factories.WebUserFactory.Create(isActive, msgs, dc: dc);
            var user2 = WebUser.LoadByIsActive(isActive, dc: dc);

            Assert.NotNull(user2);
        }

        [Fact(DisplayName = "Loads by facebook id")]
        public void LoadByFacebookIdTest()
        {
            var testId = "testid";

            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            user.FacebookId = "testid";
            msgs.AddRange(user.Save(dc: dc));

            var user2 = WebUser.LoadByFacebookId(testId, dc: dc);

            Assert.NotNull(user2);
        }

        [Fact(DisplayName = "Gets the users invite url")]
        public void GetInviteUrlTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var url = user.GetInviteUrl(account);

            Assert.NotEqual(0, url.Length);
        }

        [Fact(DisplayName = "Load by email address")]
        public void LoadByEmailTest()
        {
            var user = Factories.WebUserFactory.CreateWithEmail("test@test.com", msgs, dc: dc);
            var user2 = WebUser.LoadByEmail(user.Email, msgs, dc);

            Assert.NotNull(user2);
        }

        [Theory(DisplayName = "Check if email is already in use by any other than the current user")]
        [InlineData("test1@test.com", "test2@test.com", "test2@test.com", false)]
        [InlineData("test1@test.com", "test2@test.com", "test3@test.com", true)]
        public void CanEmailBeUsedTest(string email1, string email2, string testEmail, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email1, msgs, dc: dc);
            var user2 = Factories.WebUserFactory.CreateWithEmail(email2, msgs, dc: dc);

            Assert.True(user.CanEmailBeUsed(testEmail, msgs, dc) == result);
        }

        [Theory(DisplayName = "Check if email already exists at all in system")]
        [InlineData("test1@test.com", "test1@test.com", true)]
        [InlineData("test1@test.com", "test2@test.com", false)]
        public void ExistsByEmailTest(string email1, string testEmail, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email1, msgs, dc);

            Assert.Equal(result, WebUser.ExistsByEmail(testEmail, msgs, dc));
        }

        [Fact(DisplayName = "Check if UID exists in webuser table - passing")]
        public void ExistsByUidPassingTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);

            Assert.True(WebUser.ExistsByUid(user.Uid, msgs, dc));
        }

        [Fact(DisplayName = "Check if UID exists in webuser table - failing")]
        public void ExistsByUidFailingTest()
        {
            Assert.False(WebUser.ExistsByUid(Guid.NewGuid(), msgs, dc));
        }

        [Fact(DisplayName = "Load by username")]
        public void LoadByUsernameTest()
        {
            var user = Factories.WebUserFactory.CreateWithUsername("username1", msgs, dc: dc);

            Assert.NotNull(WebUser.LoadByUsername(user.Username, msgs, dc));
        }

        [Theory(DisplayName = "Login with username and password")]
        [InlineData("username1", "password1", "username1", "password1", true)]
        [InlineData("username1", "password1", "username2", "password1", false)]
        [InlineData("username1", "password1", "username1", "password2", false)]
        [InlineData("username1", "password1", "username2", "password2", false)]
        public void LoginByUsernameTest(string username, string password, string testUsername, string testPassword, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithUsername(username, password, msgs, dc: dc);

            Assert.True(WebUserService.LoginByUsername(testUsername, testPassword, dc: dc).IsNotEmpty() == result);
        }

        [Theory(DisplayName = "Login with email and password")]
        [InlineData("test1@test.com", "password1", "test1@test.com", "password1", true)]
        [InlineData("test1@test.com", "password1", "test2@test.com", "password1", false)]
        [InlineData("test1@test.com", "password1", "test1@test.com", "password2", false)]
        [InlineData("test1@test.com", "password1", "test2@test.com", "password2", false)]
        public void LoginByEmailTest(string email, string password, string testEmail, string testPassword, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, password, msgs, dc: dc);

            Assert.True(WebUserService.LoginByEmail(testEmail, testPassword, dc: dc).IsNotEmpty() == result);
        }

        [Theory(DisplayName = "Encrypt password")]
        [InlineData("password1")]
        public void EncryptPasswordTest(string password)
        {
            var passwordEncrypted = WebUserService.EncryptPassword(password);

            Assert.NotEqual(password, passwordEncrypted);
        }

        [Fact]
        public void IsValidPasswordTest()
        {
            var testPassword = "test2";
            var testPasswordEncrypted = WebUserService.EncryptPassword(testPassword);
            var validatePassword = WebUserService.IsValidPassword(testPassword, testPasswordEncrypted);

            Assert.True(validatePassword);
        }


        [Theory]
        [InlineData("test@test.com", false)]
        public void SendInviteTest(string email, bool expectedResult)
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.CreateWithEmail(email, msgs, dc: dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);

            var hasErrors = user.SendInviteToAccount(account, "invite message", loggedInUser, dc).HasErrors();

            Assert.Equal(hasErrors, expectedResult);
        }

        [Theory(DisplayName = "Get associated accounts")]
        [InlineData(1)]
        [InlineData(2)]
        public void GetAccountsTest(int numberOfAccounts)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            for (var i = 0; i < numberOfAccounts; i++)
            {
                var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
                account.AddUser(user, UserType.Administrator, user, msgs, dc);
            }

            Assert.Equal(numberOfAccounts, user.GetAccounts(dc: dc).Count);
        }

        [Theory(DisplayName = "Get the user type as definied by the account relationship")]
        [InlineData(UserType.Administrator)]
        [InlineData(UserType.Parent)]
        public void GetUserTypeTest(UserType ut)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var awu = Factories.AccountWebUserFactory.Create(account, user, ut, true, msgs, dc: dc);

            Assert.Equal(ut, user.GetUserType(account, msgs, dc));
        }
        
        [Fact(DisplayName = "Updates the last activity date for the user")]
        public void UpdateLastActivityTest()
        {
            var testdate = new DateTime();
            var user = Factories.WebUserFactory.Create(msgs, dc);
            user.LastActivity = testdate;
            msgs.AddRange(user.Save(dc));

            var pre = user.LastActivity;
            user.UpdateLastActivity(msgs, dc);
            var post = user.LastActivity;

            Assert.NotEqual(pre, post);
        }

        [Fact(DisplayName = "Updates the last login date for the user")]
        public void UpdateLastLoginTest()
        {
            var testdate = new DateTime();
            var user = Factories.WebUserFactory.Create(msgs, dc);
            user.LastLogin = testdate;
            msgs.AddRange(user.Save(dc: dc));

            var pre = user.LastLogin;
            user.UpdateLastLogin(msgs, dc);
            var post = user.LastLogin;

            Assert.NotEqual(pre, post);
        }

        //cannot test because Feature is not fully implemeted always returns true currently
        //user.HasAccessToFeature doesn't exist added HasPermission Test
        [Fact]
        public void HasPermissionFalseTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var accountUser = Factories.AccountWebUserFactory.Create(account, user, UserType.Parent, true, msgs, dc);


            Assert.False(user.HasPermission(PermissionType.StudentsManage, account, msgs, dc));
        }

        [Fact]
        public void HasPermissionTrueTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var accountUser = Factories.AccountWebUserFactory.Create(account, user, UserType.Administrator, true, msgs, dc);


            Assert.True(user.HasPermission(PermissionType.StudentsManage, account, msgs, dc));
        }

        [Fact]
        public void HasAccessToActivityTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);

            var activityUser = Factories.ActivityWebUserFactory.Create(activity, user, msgs, dc);

            Assert.True(user.HasAccessToActivity(activity, msgs, dc));

        }
        [Fact(DisplayName = "Generate unique username")]
        public void GenerateUsernameTest()
        {
            var f = "John";
            var l = "Doe";
            var username = "JohnDoe";

            Assert.Equal(username, WebUserService.GenerateUsername(f, l, dc: dc));
        }

        [Theory]
        [InlineData("test@test.com", true)]
        public void SendVerificationEmailTest(string email, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, msgs, dc: dc);
            Assert.Equal(user.SendVerificationEmail().HasNoErrors(), result);
        }

        [Fact(DisplayName = "Marks user's email as verified")]
        public void MarkAsVerifiedTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            msgs.AddRange(user.MarkAsVerified(dc: dc));

            Assert.True(user.VerifiedAccount);
        }

        [Theory(DisplayName = "Changes a user's password")]
        [InlineData("username1", "password1", "password2")]
        public void ChangePasswordTest(string username, string password, string password2)
        {
            var user = Factories.WebUserFactory.CreateWithUsername(username, password, msgs, dc: dc);
            msgs.AddRange(user.ChangePassword(password2, dc: dc));

            Assert.NotNull(WebUserService.LoginByUsername(username, password2, dc: dc));
        }

        [Theory]
        [InlineData("test@test.com", true)]
        public void SendPasswordResetTokenTest(string email, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, msgs, dc: dc);

            Assert.Equal(user.SendPasswordResetToken(true, dc: dc).HasNoErrors(), result);
        }

        [Fact(DisplayName = "Checks if user is active for an account")]
        public void isActiveForTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var awu = Factories.AccountWebUserFactory.Create(account, user, UserType.ProgramDirector, true, msgs, dc: dc);

            Assert.True(user.IsActiveFor(account, dc: dc));
        }

        [Fact(DisplayName = "Gets the ActivityWebUser (join table) for a user and activity")]
        public void GetActivityWebUserTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var awu = Factories.ActivityWebUserFactory.Create(activity, user, msgs, dc: dc);

            Assert.NotNull(user.GetActivityWebUser(activity, msgs, dc));
        }

        [Fact(DisplayName = "Is User the activity leader for an activity")]
        public void isActivityLeaderTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var awu = Factories.ActivityWebUserFactory.Create(activity, user, true, msgs, dc: dc);

            Assert.True(user.IsActivityLeader(activity, dc: dc));
        }

        [Fact(DisplayName = "Give user access to existing account")]
        public void AddAccountTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            accountOwner.AddAccount(account, UserType.Owner, loggedInUser, msgs, dc);

            Assert.NotNull(AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, accountOwner.ID.Value, msgs, dc));
        }

        [Fact(DisplayName = "Create a new account that this user owns")]
        public void AddNewAccountTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var package = Factories.PackageFactory.Create(msgs, dc);

            var account = accountOwner.AddNewAccount(
                            orgName: "Test Org",
                            phone: null,
                            isActive: true,
                            accountType: AccountType.School,
                            packageId: package.ID,
                            packageBillingPeriod: BillingPeriodType.Month,
                            timeZone: null,
                            isAfterSchool: true,
                            loggedInUser: loggedInUser,
                            msgs: msgs,
                            dc: dc);

            Assert.NotNull(AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, accountOwner.ID.Value, msgs, dc));
        }
        [Fact(DisplayName = "User completes signup from signup link")]
        public void CompleteSignUpTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var package = Factories.PackageFactory.CreateWithStripe(msgs, dc); // because adding a new account saves with stripe

            var account = accountOwner.AddNewAccount(
                            orgName: "Test Org",
                            phone: null,
                            isActive: true,
                            accountType: AccountType.School,
                            packageId: package.ID,
                            packageBillingPeriod: BillingPeriodType.Month,
                            timeZone: null,
                            isAfterSchool: true,
                            loggedInUser: loggedInUser,
                            msgs: msgs,
                            dc: dc);

            accountOwner.CompleteSignUp(account, "jeff", "gordon", "jeff@gordon.com", "1234", null, msgs, dc);

            Assert.True(accountOwner.HasAccessTo(account, msgs, dc));
        }
    }
}
