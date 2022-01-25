using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibraryTests;
using Newtonsoft.Json;

namespace after_school_hq.Tests
{
    public class UtilitiesTests : BaseTest
    {
        public UtilitiesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("[{\"key\":\"K\",\"value\":\"K\"}]", "[{\"key\":\"K\",\"value\":\"K\"}]")]
        [InlineData("{\"key\":\"1\",\"value\":\"1\"}", "[{\"key\":\"1\",\"value\":\"1\"}]")]
        public void GetKeyValuePairsTest(string gradeText, string output)
        {            
            Assert.Equal(JsonConvert.SerializeObject(AfterSchoolLibrary.Helpers.Utilities.GetKeyValuePairs(gradeText)), output);
        }
    }
}