using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class CreateAndManageSessionsPage : IDisposable
    {
        private IWebDriver webDriver;
        public CreateAndManageSessionsPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding CreateAndManageSessionsPage WebElements-----------//

        private IWebElement ActivityCenterElement => webDriver.FindElement(By.XPath("//ul[@class='sidebar-elements']//li[3]"));
        private IWebElement SessionLinkElement => webDriver.FindElement(By.XPath("(//div[@class='content nano-content'])[1]//li[3]//a"));
        private IWebElement AddSessionBtnElement => webDriver.FindElement(By.XPath("(//div[@class='pull-right'])[1]//a"));
        private IWebElement SessionNameBtnElement => webDriver.FindElement(By.Id("name"));



        //------------CreateAndManageSessionsPage Helper Methods-----------//
        public CreateAndManageSessionsPage HoverActivityCenterAndClickSession()
        {
            webDriver.WaitInSeconds(1);
            Actions action = new Actions(webDriver);
            action.MoveToElement(ActivityCenterElement).Perform();
            webDriver.WaitInSeconds(2);
            SessionLinkElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public CreateAndManageSessionsPage ClickAddSessionBtn()
        {
            webDriver.WaitInSeconds(1);
            AddSessionBtnElement.Click();
            webDriver.WaitInSeconds(2);
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
