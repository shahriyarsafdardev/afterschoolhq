using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests
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
            var user2 = WebUser.LoadByEmail(user.Email, dc: dc);

            Assert.NotNull(user2);
        }

        [Theory(DisplayName = "Check if email is already in use by any other than the current user")]
        [InlineData("test1@test.com", "test2@test.com", "test2@test.com", false)]
        [InlineData("test1@test.com", "test2@test.com", "test3@test.com", true)]
        public void CanEmailBeUsedTest(string email1, string email2, string testEmail, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email1, msgs, dc: dc);
            var user2 = Factories.WebUserFactory.CreateWithEmail(email2, msgs, dc: dc);

            Assert.True(user.CanEmailBeUsed(testEmail, dc: dc) == result);
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

            Assert.True(WebUser.ExistsByUid(user.Uid, dc: dc));
        }

        [Fact(DisplayName = "Check if UID exists in webuser table - failing")]
        public void ExistsByUidFailingTest()
        {
            Assert.False(WebUser.ExistsByUid(Guid.NewGuid(), dc: dc));
        }

        [Fact(DisplayName = "Load by username")]
        public void LoadByUsernameTest()
        {
            var user = Factories.WebUserFactory.CreateWithUsername("username1", msgs, dc: dc);

            Assert.NotNull(WebUser.LoadByUsername(user.Username, dc: dc));
        }

        [Theory(DisplayName = "Login with username and password")]
        [InlineData("username1", "password1", "username1", "password1", true)]
        [InlineData("username1", "password1", "username2", "password1", false)]
        [InlineData("username1", "password1", "username1", "password2", false)]
        [InlineData("username1", "password1", "username2", "password2", false)]
        public void LoginByUsernameTest(string username, string password, string testUsername, string testPassword, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithUsername(username, password, msgs, dc: dc);

            Assert.True(WebUser.LoginByUsername(testUsername, testPassword, dc: dc).IsNotEmpty() == result);
        }

        [Theory(DisplayName = "Login with email and password")]
        [InlineData("test1@test.com", "password1", "test1@test.com", "password1", true)]
        [InlineData("test1@test.com", "password1", "test2@test.com", "password1", false)]
        [InlineData("test1@test.com", "password1", "test1@test.com", "password2", false)]
        [InlineData("test1@test.com", "password1", "test2@test.com", "password2", false)]
        public void LoginByEmailTest(string email, string password, string testEmail, string testPassword, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, password, msgs, dc: dc);

            Assert.True(WebUser.LoginByEmail(testEmail, testPassword, dc: dc).IsNotEmpty() == result);
        }

        [Theory(DisplayName = "Encrypt password")]
        [InlineData("password1")]
        public void EncryptPasswordTest(string password)
        {
            var passwordEncrypted = WebUserService.EncryptPassword(password);

            Assert.NotEqual(password, passwordEncrypted);
        }

        [Fact(DisplayName = "Descrypt password")]
        public void DecryptPasswordTest()
        {
            var testPassword = "test2";
            var testPasswordEncrypted = WebUserService.EncryptPassword(testPassword);
            var testPasswordDecrypted = WebUserService.DecryptPassword(testPasswordEncrypted);

            Assert.Equal(testPassword, testPasswordDecrypted);
        }

        [Theory(DisplayName = "Send invite to user to create account")]
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
                var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);
                msgs.AddRange(account.AddUser(user, UserType.Administrator, user.ID.Value, dc: dc));
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

            Assert.Equal(ut, user.GetUserType(account, dc: dc));
        }
        
        [Fact(DisplayName = "Updates the last activity date for the user")]
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

        [Fact(DisplayName = "Updates the last login date for the user")]
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
        //[Fact(Skip = "Not implemented")]
        //public void HasAcessToFeatureFalseTest()
        //{
        //    var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
        //    var user = Factories.WebUserFactory.Create(msgs, dc: dc);
        //    var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);

        //    msgs.AddRange(account.AddUser(user, UserType.Parent, user.ID.Value, dc: dc));

        //    Assert.False(user.HasAccessToFeature(FeatureType.ManageForms, account, dc: dc));
        //}

        //[Fact(Skip = "Not implemented")]
        //public void HasAcessToFeatureTrueTest()
        //{
        //    var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
        //    var user = Factories.WebUserFactory.Create(msgs, dc: dc);
        //    var account = Factories.AccountFactory.Create(accountOwner, msgs, dc: dc);

        //    msgs.AddRange(account.AddUser(user, UserType.SuperAdmin, user.ID.Value, dc: dc));

        //    Assert.True(user.HasAccessToFeature(FeatureType.ViewInsights, account, dc: dc));
        //}

        [Fact(DisplayName = "Generate unique username")]
        public void GenerateUsernameTest()
        {
            var f = "John";
            var l = "Doe";
            var username = "JohnDoe";

            Assert.Equal(username, WebUserService.GenerateUsername(f, l, dc: dc));
        }

        [Theory(DisplayName = "Sends email to verify user email")]
        [InlineData("test@test.com", true)]
        public void SendVerificationEmailTest(string email, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, msgs, dc: dc);
            Assert.Equal(user.SendVerificationEmail(true).HasNoErrors(), result);
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

            Assert.NotNull(WebUser.LoginByUsername(username, password2, dc: dc));
        }

        [Theory(DisplayName = "Sends password reset token")]
        [InlineData("test@test.com", true)]
        public void SendPasswordResetTokenTest(string email, bool result)
        {
            var user = Factories.WebUserFactory.CreateWithEmail(email, msgs, dc: dc);

            Assert.Equal(user.SendPasswordResetToken(true, dc: dc).HasNoErrors(), result);
        }

        [Theory(DisplayName = "Creates a registrant from a user account")]
        [InlineData(RegistrantType.Individual, false)]
        [InlineData(RegistrantType.Individual, true)]
        [InlineData(RegistrantType.Organization, true)]
        public void CreateRegistrantTest(RegistrantType rt, bool isMaster)
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);
            var registrant = user.CreateRegistrant(rt, isMaster, false, msgs, dc);

            Assert.NotNull(registrant);
        }

        [Theory(DisplayName = "Gets all registrants associated to the user account")]
        [InlineData(1)]
        [InlineData(3)]
        public void GetRegistrantsTest(int numberOfRegistrants)
        {
            var user = Factories.WebUserFactory.Create(msgs, dc: dc);

            for (var i = 0; i < numberOfRegistrants; i++)
            {
                user.CreateRegistrant(RegistrantType.Individual, true, false, msgs, dc);
            }

            Assert.Equal(numberOfRegistrants, user.GetRegistrants(msgs, dc).Count);
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

            Assert.NotNull(user.GetActivityWebUser(activity, dc: dc));
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

        [Fact]
        public void saltPasswordTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            var orig = ASCIIEncoding.ASCII.GetBytes("test");
            var saltAndHashed = WebUserService.SaltandHashPassword("test");

            Assert.NotEqual(orig, WebUserService.SaltandHashPassword("test").hashedbytes);
        }

        /// <summary>
        /// Then decrypt their passwords using the old method, 
        /// then encrypt and store the new passwords in the db with the new 
        /// encryption method, then test that the passwords are different
        /// </summary>
        [Fact]
        public void FullPasswordTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            var origInBytes = ASCIIEncoding.ASCII.GetBytes("test");
            var encrypt = AfterSchoolLibrary.Utilities.EncryptToBase64("test");
            var decrypt = AfterSchoolLibrary.Utilities.DecryptFromBase64(encrypt);
            var saltAndHashed = WebUserService.SaltandHashPassword(decrypt);

            Assert.NotEqual(decrypt, saltAndHashed.hashedbytes.ToString());
        }

        [Fact(Skip = "Need to figure out why it's not working")]
        public void LoadAndHashAllTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.CreateWithEmail("notDarye", "NothisPassword", msgs, dc);
            var orig = user.Password;
            var origInBytes = ASCIIEncoding.ASCII.GetBytes(orig);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            var message = AfterSchoolLibrary.Utilities.LoadAndHashAllUsers(dc);
            var newBytes = user.Password;

            Assert.NotEqual(orig, newBytes);
        }

        [Fact(Skip = "Need to figure out why it's not working")]
        public void LoadAndHashAllT2est()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var user = Factories.WebUserFactory.CreateWithEmail("notDarye", "NothisPassword", msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var awu = Factories.ActivityWebUserFactory.Create(activity, user, true, msgs, dc);

            var origInBytes = ASCIIEncoding.ASCII.GetBytes("test");
            var message = AfterSchoolLibrary.Utilities.LoadAndHashAllUsers(dc);
            user.Reload(dc: dc);

            user = WebUser.LoginByEmailHashed("notDarye", "nothisPassword", dc);

            Assert.NotNull(user);
        }

        [Fact(DisplayName = "Give user access to existing account")]
        public void AddAccountTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            msgs.AddRange(accountOwner.AddAccount(account, UserType.Owner, loggedInUser, dc));

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
                           isAfterSchool: true,
                           loggedInUser: loggedInUser,
                           msgs: msgs,
                           dc: dc);

            msgs.AddRange(accountOwner.CompleteSignUp(account, "jeff", "gordon", "jeff@gordon.com", "1234", null, dc));

            Assert.True(accountOwner.HasAccessTo(account, msgs, dc));
        }
    }
}
