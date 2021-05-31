using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AfterSchoolUIAutomationTests.Helpers.Extensions
{
    public static class WebDriverExtension
    {
        public static void WaitInSeconds(this IWebDriver webDriver, int seconds)
        {
            Thread.Sleep(1000 * seconds); 
        }
    }
}
