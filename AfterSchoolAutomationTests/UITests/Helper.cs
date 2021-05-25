using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolAutomationTests.UITests
{
    public class Helper
    {
        private readonly IWebDriver _driver;
        private const string URI = "https://qa.afterschoolhq.com/login";

        public Helper(IWebDriver driver)
        {
            _driver = driver;
        }

        //------------WebElements-----------
        private IWebElement UserNameElement => _driver.FindElement(By.Id("username"));
        private IWebElement UserPasswordElement => _driver.FindElement(By.Id("password"));
        private IWebElement SubmitElement => _driver.FindElement(By.ClassName("btn-primary"));
        private IWebElement SignUpGetStartedElement => _driver.FindElement(By.XPath("(//button[@id='btn-GetStarted'])[1]"));
        private IWebElement CreateIndividualAccountElement => _driver.FindElement(By.XPath("(//div[@class='row margin-t-35']//button[text()='Create an Individual Account'])[1]"));
        private IWebElement ProvideProgramsForStudentsElement => _driver.FindElement(By.XPath("(//div[@class='row margin-t-35']//button[text()='Provide programs for students'])[1]"));
        private IWebElement SignUpAccountSearchElement => _driver.FindElement(By.Id("signup-account-search"));
        private IWebElement ProgramNameSubmitElement => _driver.FindElement(By.XPath("(//div[@class='visible-md visible-lg']//button[text()='Continue'])[1]"));
        private IWebElement FirstNameElement => _driver.FindElement(By.Name("firstname"));
        private IWebElement LasttNameElement => _driver.FindElement(By.Name("lastname"));
        private IWebElement EmailElement => _driver.FindElement(By.Name("email"));
        private IWebElement PasswordElement => _driver.FindElement(By.Name("password1"));
        private IWebElement Password2Element => _driver.FindElement(By.Name("password2"));
        private IWebElement CaptchaElement => _driver.FindElement(By.Id("recaptcha-anchor"));
        private IWebElement BasicInfoSubmitElement => _driver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Continue'])[1]"));
        private IWebElement ShortDescriptionElement => _driver.FindElement(By.Name("headline"));
        private IWebElement TimeZoneElement => _driver.FindElement(By.Id("select-program-timezone"));
        private IWebElement SaveAndContinueElement => _driver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Save and Continue'])[1]"));


        //------------WebElements-----------



        //------------Login Helper Methods-----------
        public void PopulateName(string name) => UserNameElement.SendKeys(name);
        public void PopulatePassword(string password) => UserPasswordElement.SendKeys(password);
        public void ClickSubmit() => SubmitElement.Click();


        //------------Account Signup Helper Methods-----------
        public void ClickSignUp() => SignUpGetStartedElement.Click();
        public void ClickIndividualAccount() => CreateIndividualAccountElement.Click();
        public void ProvideProgramsForStudents() => ProvideProgramsForStudentsElement.Click();
        public void PopulateProgramName(string name) => SignUpAccountSearchElement.SendKeys(name);
        public void ClickProgramSubmit() => ProgramNameSubmitElement.Click();
        public void PopulateFirstName(string name) => FirstNameElement.SendKeys(name);
        public void PopulateLasttName(string name) => LasttNameElement.SendKeys(name);
        public void PopulateEmail(string name) => EmailElement.SendKeys(name);
        public void PopulateUserPassword(string name) => PasswordElement.SendKeys(name);
        public void PopulateUserPassword2(string name) => Password2Element.SendKeys(name);
        public void CheckCaptcha() => CaptchaElement.Click();
        public void ClickBasicInfoSubmit() => BasicInfoSubmitElement.Click();
        public void PopulateShortDescription(string name) => ShortDescriptionElement.SendKeys(name);
        public void SelectTimeZone()
        {
            var timezone = TimeZoneElement;
            var selectElement = new SelectElement(TimeZoneElement);
            selectElement.SelectByValue("Alaska - AKST");
        }
        public void SaveAndContinueSubmit() => SaveAndContinueElement.Click();

        //------------Account Signup Helper Methods-----------

    }
}
