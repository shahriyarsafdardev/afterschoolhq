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
    }
}
