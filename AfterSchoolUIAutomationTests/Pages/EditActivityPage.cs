using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class EditActivityPage
    {
        private IWebDriver webDriver;

        public EditActivityPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Create Activity WebElements-----------//

        //------------Settings Section Web Elements-----------//
        private IWebElement AccountButtonElement => webDriver.FindElement(By.XPath("(//div[@id='accounts']//button)[1]"));
        private IWebElement ActivitiesElement => webDriver.FindElement(By.XPath("//ul[@class='sidebar-elements']//li[3]"));
        private IWebElement ManageActivityElement => webDriver.FindElement(By.XPath("//div[@class='table-responsive']//table/tbody/tr[6]/td[1]"));
        private IWebElement EditActivityElement => webDriver.FindElement(By.XPath("//div[@class='pull-right visible-lg visible-md']"));


        //------------Edit Account Profile Helper Methods-----------//

        //------------Settings Section-----------//
        public EditActivityPage ClickAccountButton()
        {
            AccountButtonElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }
        public EditActivityPage ClickActivitiesLink()
        {
            webDriver.WaitInSeconds(1);
            ActivitiesElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        public EditActivityPage ClickManageButton()
        {
            ManageActivityElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        public EditActivityPage ClickEditActivityLink()
        {
            EditActivityElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }

        public void Dispose()
        {
            if (webDriver != null)
            {
                webDriver.Dispose();
                webDriver = null;
            }
        }
    }
}