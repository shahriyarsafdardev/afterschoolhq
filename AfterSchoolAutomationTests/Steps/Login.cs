using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AfterSchoolAutomationTests.Pages;

namespace AfterSchoolAutomationTests.Steps
{
    public class Login
    {
        [Fact]
        public void LoginTest()
        {
            LoginPage login = new LoginPage();

            login.Navigate("https://qa.afterschoolhq.com/login");

            //        .PopulateName("parent3_hq@yopmail.com");
            //_helper.PopulatePassword("123456789");
            //_helper.ClickSubmit();
            //Assert.Equal("Passed", "Passed");
        }
    }
}
