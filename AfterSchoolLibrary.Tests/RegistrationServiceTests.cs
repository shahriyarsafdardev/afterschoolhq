using AfterSchoolLibrary.Services;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class RegistrationServiceTests
    {
        [TestCase("$email-th/is report!", "email-this report.csv")]
        [TestCase("new.report.csv", "new.report.csv")]
        [TestCase("numbers.csv", "numbers.csv")]
        [TestCase("attendance", "attendance.csv")]
        [TestCase("", "registrations.csv")]
        [TestCase("new.csv.report", "new.csv.report.csv")]
        public void CreateRegistrationsCsvFilenameTests(string input, string expected)
        {
            var actual = RegistrationService.CreateRegistrationsCsvFilename(input);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}