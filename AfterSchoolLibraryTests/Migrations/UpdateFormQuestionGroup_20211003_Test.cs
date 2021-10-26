using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class UpdateFormQuestionGroup_20211003 : BaseTest
    {
        private WebUser User { get; set; }
        private Account Account { get; set; }
        private Form Form { get; set; }
        private FormQuestionGroup FormQuestionGroup { get; set; }

        public UpdateFormQuestionGroup_20211003(ITestOutputHelper output) : base(output)
        {
            User = WebUserFactory.Create(msgs, dc);
            Account = AccountFactory.CreateWithFreePackage(User, AccountType.School, msgs, dc);
            Form = FormFactory.Create(Account, msgs, dc);
        }

        [Fact(DisplayName = "Activate inactive group")]
        public void RunMigration1()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            FormQuestionGroup = FormQuestionGroupFactory.Create(formId: Form.ID.Value, isActive: false, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.UpdateFormQuestionGroup_20211003.Do(msgs, dc);

            var expected = FormQuestionGroup.Load(FormQuestionGroup.ID.Value, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(expected);
            Assert.False(FormQuestionGroup.IsActive);
            Assert.True(expected.IsActive);
        }

        [Fact(DisplayName = "No change on existing active group")]
        public void RunMigration2()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            FormQuestionGroup = FormQuestionGroupFactory.Create(formId: Form.ID.Value, isActive: true, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.UpdateFormQuestionGroup_20211003.Do(msgs, dc);

            var expected = FormQuestionGroup.Load(FormQuestionGroup.ID.Value, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(expected);
            Assert.True(FormQuestionGroup.IsActive);
            Assert.True(expected.IsActive);
        }
    }
}
