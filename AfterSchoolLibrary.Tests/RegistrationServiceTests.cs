using AfterSchoolLibrary.Services;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class RegistrationServiceTests
    {
        [TestCase("registrations.csv", "registrations.csv")]
        [TestCase("", "registrations.csv")]
        public void FormatFilenameTests(string input, string expected)
        {
            var actual = RegistrationService.FormatFilename(input);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}