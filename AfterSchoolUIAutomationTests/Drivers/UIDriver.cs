using OpenQA.Selenium;
using System.Collections.Generic;
using System.Diagnostics;

namespace AfterSchoolUIAutomationTests.Drivers
{
    public static class UIDriver
    {
        public static IWebDriver WebDriver { get; set; }

        public static void CloseDriver()
        {
            if (WebDriver != null)
            {
                WebDriver.Quit();
                WebDriver = null;
            }

            var processess = new List<string> { "chromedriver", "geckodriver" };
            foreach (var process in processess)
            {
                Process[] driverProcesses = Process.GetProcessesByName(process);
                foreach (var driverProcess in driverProcesses)
                {
                    driverProcess.Kill();
                }
            }
        }
    }
}