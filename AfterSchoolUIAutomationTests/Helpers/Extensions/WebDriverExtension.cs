using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AfterSchoolUIAutomationTests.Helpers.Extensions
{
    public static class WebDriverExtension
    {
        static Random rand = new Random();
        public static void WaitInSeconds(this IWebDriver webDriver, int seconds)
        {
            Thread.Sleep(1000 * seconds);
        }

        public static bool IsElementPresent(this IWebDriver webDriver, By by)
        {
            try
            {
                webDriver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public static string GetRandomTelNo()
        {
            //StringBuilder telNo = new StringBuilder(12);
            string telNo = "202-555-";
            int number;
            for (int i = 0; i < 3; i++)
            {
                number = rand.Next(0, 9); // digit between 0 (incl) and 8 (excl)
                telNo = telNo + number;
            }
            return telNo.ToString();
        }
    }
}
