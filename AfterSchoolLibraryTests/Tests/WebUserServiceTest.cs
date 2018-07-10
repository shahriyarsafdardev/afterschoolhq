using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;

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
                            password: null,
                            isActive: false,
                            isRegistered: false,
                            isVerified: false,
                            loggedInUser: null,
                            msgs: msgs,
                            dc: dc);

            Assert.True(newUser.ID.HasValue);

        }
    }
}
