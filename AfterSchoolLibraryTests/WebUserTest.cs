using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class WebUserTest : IDisposable
    {
        DatabaseConnection dc = null;

        public WebUserTest()
        {
            AfterSchoolLibrary.Utilities.InitializeApp();
            dc = new DatabaseConnection(Database.Credentials.ConnectionString);
            dc.Open();
            dc.BeginTransaction();
        }

        public void Dispose()
        {
            dc.RollbackTransaction();
            dc.Close();
            dc.Dispose();
        }

        [Fact]
        public void LoadByEmailTest()
        {
            var emailAddress = "darye@reborncode.com";

            var user = new WebUser() { Email = emailAddress };
            user.Save(dc);

            var user2 = WebUser.LoadByEmail(emailAddress, dc);

            Assert.NotNull(user2);
        }

        [Fact]
        public void IsEmailInUseByAnotherUserPassingTest()
        {
            var emailAddress1 = "test1@test.com";
            var emailAddress2 = "test2@test.com";

            var currentUser = new WebUser() { Email = emailAddress1 };
            currentUser.Save(dc);

            var otherUser = new WebUser() { Email = emailAddress2 };
            otherUser.Save(dc);

            Assert.True(WebUser.IsEmailInUseByAnotherUser(emailAddress2, currentUser.ID.Value, dc));
        }

        [Fact]
        public void IsEmailInUseByAnotherUserFailingTest()
        {
            var emailAddress1 = "test1@test.com";
            var emailAddress2 = "test2@test.com";
            var emailAddress3 = "test3@test.com";

            var currentUser = new WebUser() { Email = emailAddress1 };
            currentUser.Save(dc);

            var otherUser = new WebUser() { Email = emailAddress2 };
            otherUser.Save(dc);

            Assert.False(WebUser.IsEmailInUseByAnotherUser(emailAddress3, currentUser.ID.Value, dc));
        }

        [Fact]
        public void ExistsByEmailPassingTest()
        {
            var emailAddress1 = "test1@test.com";

            var currentUser = new WebUser() { Email = emailAddress1 };
            currentUser.Save(dc);

            Assert.True(WebUser.ExistsByEmail(emailAddress1, dc));
        }

        [Fact]
        public void ExistsByEmailFailingTest()
        {
            var emailAddress1 = "test1@test.com";
            var emailAddress2 = "test2@test.com";

            var currentUser = new WebUser() { Email = emailAddress1 };
            currentUser.Save(dc);

            Assert.False(WebUser.ExistsByEmail(emailAddress2, dc));
        }

        [Fact]
        public void ExistsByUidPassingTest()
        {
            var testUID = System.Guid.NewGuid();

            var currentUser = new WebUser() { Uid = testUID };
            currentUser.Save(dc);

            Assert.True(WebUser.ExistsByUid(testUID, dc));
        }

        [Fact]
        public void ExistsByUidFailingTest()
        {
            var testUID = System.Guid.NewGuid();
            var testUID2 = System.Guid.NewGuid();
            var currentUser = new WebUser() { Uid = testUID2 };
            currentUser.Save(dc);

            Assert.False(WebUser.ExistsByUid(testUID, dc));
        }

        [Fact]
        public void LoadByUsernamePassingTest()
        {
            var testUsername = "test";
            var testUsername2 = "test2";
            var currentUser = new WebUser() { Username = testUsername };
            currentUser.Save(dc);

            Assert.NotNull(WebUser.LoadByUsername(testUsername, dc));
        }

        [Fact]
        public void LoadByUsernameFailingTest()
        {
            var testUsername = "test";
            var testUsername2 = "test2";
            var currentUser = new WebUser() { Username = testUsername };
            currentUser.Save(dc);

            Assert.Null(WebUser.LoadByUsername(testUsername2, dc));
        }

        [Fact]
        public void LoginByUsernamePassingTest()
        {
            var testUsername = "test";
            var testPassword = "test2";
            var currentUser = new WebUser() { Username = testUsername, Password = WebUser.EncryptPassword(testPassword) };
            currentUser.Save(dc);

            Assert.NotNull(WebUser.LoginByUsername(testUsername, testPassword, dc));
        }

        [Fact]
        public void LoginByUsernameFailingTest()
        {
            var testUsername = "test";
            var testPassword = "test2";
            var currentUser = new WebUser() { Username = testUsername, Password = testPassword };
            currentUser.Save(dc);

            Assert.Null(WebUser.LoginByUsername(testUsername, "wrong", dc));
        }

        [Fact]
        public void LoginByEmailPassingTest()
        {
            var testEmail = "test";
            var testPassword = "test2";
            var currentUser = new WebUser() { Email = testEmail, Password = WebUser.EncryptPassword(testPassword) };
            currentUser.Save(dc);

            Assert.NotNull(WebUser.LoginByEmail(testEmail, testPassword, dc));
        }

        [Fact]
        public void LoginByEmailFailingTest()
        {
            var testEmail = "test";
            var testPassword = "test2";
            var currentUser = new WebUser() { Email = testEmail, Password = WebUser.EncryptPassword(testPassword) };
            currentUser.Save(dc);

            Assert.Null(WebUser.LoginByUsername(testEmail, "wrong", dc));
        }

        [Fact]
        public void EncryptPasswordTest()
        {
            var testPassword = "test2";
            var testPasswordEncrypted = WebUser.EncryptPassword(testPassword);

            Assert.NotEqual(testPassword, testPasswordEncrypted);
        }

        [Fact]
        public void DecryptPasswordTest()
        {
            var testPassword = "test2";
            var testPasswordEncrypted = WebUser.EncryptPassword(testPassword);
            var testPasswordDecrypted = WebUser.DecryptPassword(testPasswordEncrypted);

            Assert.Equal(testPassword, testPasswordDecrypted);
        }
    }
}
