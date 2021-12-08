using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace AfterSchoolAutomationTests.Helpers.Extensions
{
    public static class WebDriverExtension
    {

        /// <summary>
        /// Wait element to be displayed
        /// </summary>
        /// <param name="webDriver">Webdriver</param>
        /// <param name="timeoutInSeconds">How long to wait for the evaluated condition to be true</param>
        public static void WaitToBeDisplayed(this IWebDriver webDriver, By locator, uint timeoutInSeconds = 15)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            wait.IgnoreExceptionTypes
            (
                typeof(NoSuchElementException),
                typeof(ElementNotVisibleException)
            );

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
        }

        /// <summary>
        /// Wait in seconds
        /// </summary>
        /// <param name="webDriver">Web driver</param>
        /// <param name="seconds">Number of seconds to wait</param>
        public static void WaitInSeconds(this IWebDriver webDriver, int seconds)
        {
            Thread.Sleep(1000 * seconds);
        }
    }
}
