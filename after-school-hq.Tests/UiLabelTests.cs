using AfterSchool.Models;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    [TestFixture]
    public class UiLabelTests
    {
        [TestCase("Pre-K", "Pre-K")]
        [TestCase("K", "Kindergarten")]
        [TestCase("1st", "1st Grade")]
        [TestCase("2nd", "2nd Grade")]
        [TestCase("", "")]
        [TestCase("[{\"key\":\"K\",\"value\":\"K\"}]", "Kindergarten")]
        [TestCase("[{\"key\":\"Pre-K\",\"value\":\"Pre-K\"}]", "Pre-K")]
        [TestCase("[{\"key\":\"2nd\",\"value\":\"2nd\"}]", "2nd Grade")]
        public void GetGradeDescriptionTest(string gradeText, string expected)
        {
            Assert.That(UILabel.GetGradeDescription(gradeText), Is.EqualTo(expected));
        }
    }
}