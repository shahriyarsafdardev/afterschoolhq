using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class UpdateFormQuestionAnswers_20210918_Test : BaseTest
    {
        private WebUser User { get; set; }
        private Account Account { get; set; }
        private Form Form { get; set; }
        private FormQuestionGroup FormQuestionGroup { get; set; }

        public UpdateFormQuestionAnswers_20210918_Test(ITestOutputHelper output) : base(output)
        {
            User = WebUserFactory.Create(msgs, dc);
            Account = AccountFactory.CreateWithFreePackage(User, AccountType.School, msgs, dc);
            Form = FormFactory.Create(Account, msgs, dc);
            FormQuestionGroup = FormQuestionGroupFactory.Create(formId: Form.ID.Value, isDeleted: false, msgs: msgs, dc: dc);
            
        }

        [Fact(DisplayName = "Radio list button")]
        public void RunMigration1()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var FormQuestion = FormQuestionFactory.Create(formId: Form.ID.Value, formQuestionGroupId: FormQuestionGroup.ID.Value, text: "Gender", options: "Male,Female,Other", questionTypeId: QuestionType.RadioList, msgs: msgs, dc: dc);            
            var formQuestionAnswer = FormQuestionAnswerFactory.Create(formQuestionId: FormQuestion.ID.Value, questionText: FormQuestion.Text, answerText: "Male", questionDataTypeId: FormQuestion.QuestionDataTypeId, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.UpdateFormQuestionAnswers_20210918.Do(msgs, dc);

            var expected = FormQuestionAnswer.Load(formQuestionAnswer.ID.Value, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(expected);
            Assert.Equal(expected: "[{\"key\":\"Male\",\"value\":\"Male\"}]", actual: expected.Text);
        }

        [Fact(DisplayName = "Checkbox list button")]
        public void RunMigration2()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var FormQuestion = FormQuestionFactory.Create(formId: Form.ID.Value, formQuestionGroupId: FormQuestionGroup.ID.Value, text: "Technologies", options: "HTML,JAVA,CSS", questionTypeId: QuestionType.CheckboxList, msgs: msgs, dc: dc);
            var formQuestionAnswer = FormQuestionAnswerFactory.Create(formQuestionId: FormQuestion.ID.Value, questionText: FormQuestion.Text, answerText: "CSS,HTML", questionDataTypeId: FormQuestion.QuestionDataTypeId, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.UpdateFormQuestionAnswers_20210918.Do(msgs, dc);

            var expected = FormQuestionAnswer.Load(formQuestionAnswer.ID.Value, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(expected);
            Assert.Equal(expected: "[{\"key\":\"CSS\",\"value\":\"CSS\"},{\"key\":\"HTML\",\"value\":\"HTML\"}]", actual: expected.Text);
        }

        [Fact(DisplayName = "Dropdown list button")]
        public void RunMigration3()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var FormQuestion = FormQuestionFactory.Create(formId: Form.ID.Value, formQuestionGroupId: FormQuestionGroup.ID.Value, text: "Grade", options: "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th", questionTypeId: QuestionType.DropDownList, msgs: msgs, dc: dc);
            var formQuestionAnswer = FormQuestionAnswerFactory.Create(formQuestionId: FormQuestion.ID.Value, questionText: FormQuestion.Text, answerText: "6th", questionDataTypeId: FormQuestion.QuestionDataTypeId, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.UpdateFormQuestionAnswers_20210918.Do(msgs, dc);

            var expected = FormQuestionAnswer.Load(formQuestionAnswer.ID.Value, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(expected);
            Assert.Equal(expected: "[{\"key\":\"6th\",\"value\":\"6th\"}]", actual: expected.Text);
        }

        [Fact(DisplayName = "No updates on empty answer")]
        public void RunMigration4()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var FormQuestion = FormQuestionFactory.Create(formId: Form.ID.Value, formQuestionGroupId: FormQuestionGroup.ID.Value, text: "Grade", options: "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th", questionTypeId: QuestionType.DropDownList, msgs: msgs, dc: dc);
            var formQuestionAnswer = FormQuestionAnswerFactory.Create(formQuestionId: FormQuestion.ID.Value, questionText: FormQuestion.Text, answerText: "", questionDataTypeId: FormQuestion.QuestionDataTypeId, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.UpdateFormQuestionAnswers_20210918.Do(msgs, dc);

            var expected = FormQuestionAnswer.Load(formQuestionAnswer.ID.Value, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.Empty(expected.Text);
        }

        [Fact(DisplayName = "No updates on json answer")]
        public void RunMigration5()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var FormQuestion = FormQuestionFactory.Create(formId: Form.ID.Value, formQuestionGroupId: FormQuestionGroup.ID.Value, text: "Grade", options: "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th", questionTypeId: QuestionType.DropDownList, msgs: msgs, dc: dc);
            var formQuestionAnswer = FormQuestionAnswerFactory.Create(formQuestionId: FormQuestion.ID.Value, questionText: FormQuestion.Text, answerText: "[{\"key\":\"6th\",\"value\":\"6th\"}]", questionDataTypeId: FormQuestion.QuestionDataTypeId, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.UpdateFormQuestionAnswers_20210918.Do(msgs, dc);

            var expected = FormQuestionAnswer.Load(formQuestionAnswer.ID.Value, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(formQuestionAnswer.Text, expected.Text);
        }
    }
}
