using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;

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
                            phone: "111-111-1111",
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
    }
}
