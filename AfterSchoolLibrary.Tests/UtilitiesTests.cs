using AfterSchoolLibrary.Helpers;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class UtilitiesTests
    {
        [TestCase("", 0)]
        [TestCase("{\"key\":", 0)]
        [TestCase("{\"key\":\"key-1\",\"value\":\"value-1\"}", 1)]
        [TestCase("[{\"key\":\"key-1\",\"value\":\"value-1\"}]", 1)]
        [TestCase("{\"key\":\"key-1\",\"value\":\"value-1\"},{\"key\":\"key-2\",\"value\":\"value-2\"}", 2)]
        [TestCase("[{\"key\":\"key-1\",\"value\":\"value-1\"},{\"key\":\"key-2\",\"value\":\"value-2\"}]", 2)]
        public void GetKeyValuePairsTests(string stringInput, int expectedCountOfResultList)
        {
            var result = Utilities.GetKeyValuePairs(stringInput);
            Assert.That(result, Has.Count.EqualTo(expectedCountOfResultList));
        }
    }
}