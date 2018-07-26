using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class ExtensionsTest : BaseTest
    {
        public ExtensionsTest(ITestOutputHelper output) : base(output, false)
        {
        }

        [Theory]
        [InlineData("George Washington Carver School 87", 25, "GeorgeWashingtonCarver")]
        [InlineData("Center For Inquiry School 70", 25, "CenterForInquirySchool70")]
        public void GetMaxWordsTest(string valueToCheck, int maxLengthOfWordsCombined, string expectedResult)
        {
            Assert.Equal(valueToCheck.GetMaxWords(maxLengthOfWordsCombined), expectedResult);
        }

    }
}
