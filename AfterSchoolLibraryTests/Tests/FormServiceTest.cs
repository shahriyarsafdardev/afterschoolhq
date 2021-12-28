using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Collections.Generic;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class FormServiceTest : BaseTest
    {
        public FormServiceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetAdministratorAccountFormTest()
        {
            // Arrange & Act
            var form = FormService.GetAdministratorAccountForm(msgs, dc);

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        public void GetInterestFormFormTest()
        {
            // Arrange
            var currentUser = WebUserFactory.Create(msgs, dc);

            // Act
            var form = FormService.GetInterestForm(currentUser, msgs, dc);

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        public void AddNewAfterSchoolDefaultFormTest()
        {
            // Arrange
            // Act
            var form = FormService.AddNewAfterSchoolDefaultForm(msgs: msgs, dc: dc);

            // Assert
            Assert.NotNull(form);

            // check question groups for emergency contacts
            var questionGroups = form.GetQuestionGroups(msgs, dc);
            var emergencyContact1GroupType = questionGroups.Find(m => m.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1);
            var emergencyContact2GroupType = questionGroups.Find(m => m.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2);
            var emergencyContact3GroupType = questionGroups.Find(m => m.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact3);
            Assert.NotNull(emergencyContact1GroupType);
            Assert.NotNull(emergencyContact2GroupType);
            Assert.NotNull(emergencyContact3GroupType);

            // check questions for emergency contacts
            var fqs = form.GetAllQuestions(msgs, dc);
            Assert.NotNull(fqs);
            Assert.True(fqs.Count > 0);

            // ec1
            // FirstName
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1FirstName, options: null, isRequired: true, isActive: true);

            // LastName
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1LastName, options: null, isRequired: true, isActive: true);

            // Phone
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone, options: null, isRequired: true, isActive: true);

            // Phone2
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone2, options: null, isRequired: false, isActive: true);

            // Phone3
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone3, options: null, isRequired: false, isActive: true);

            // RelationshipToRegistrant
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact1RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);

            // ec2
            // FirstName
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2FirstName, options: null, isRequired: false, isActive: true);

            // LastName
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2LastName, options: null, isRequired: false, isActive: true);

            // Phone
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone, options: null, isRequired: false, isActive: true);

            // Phone2
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone2, options: null, isRequired: false, isActive: true);

            // Phone3
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone3, options: null, isRequired: false, isActive: true);

            // RelationshipToRegistrant
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact2RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);

            // ec3
            // FirstName
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3FirstName, options: null, isRequired: false, isActive: true);

            // LastName
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3LastName, options: null, isRequired: false, isActive: true);

            // Phone
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone, options: null, isRequired: false, isActive: true);

            // Phone2
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone2, options: null, isRequired: false, isActive: true);

            // Phone3
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone3, options: null, isRequired: false, isActive: true);

            // RelationshipToRegistrant
            CheckEmergencyContactQuestion(formQuestions: fqs, text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact3RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);
        }

        [Fact]
        public void CreateEmergencyContact1GroupTest()
        {
            // Arrange
            // Act
            var qhs = FormService.CreateEmergencyContact1Group();

            // Assert
            Assert.NotNull(qhs);
            Assert.True(qhs.Count == 6);

            CheckEmergencyContactFields(contact: qhs[0], text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1FirstName, options: null, isRequired: true, isActive: true);
            CheckEmergencyContactFields(contact: qhs[1], text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1LastName, options: null, isRequired: true, isActive: true);
            CheckEmergencyContactFields(contact: qhs[2], text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone, options: null, isRequired: true, isActive: true);
            CheckEmergencyContactFields(contact: qhs[3], text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone2, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[4], text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone3, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[5], text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact1RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);
        }

        [Fact]
        public void CreateEmergencyContact2GroupTest()
        {
            // Arrange
            // Act
            var qhs = FormService.CreateEmergencyContact2Group();

            // Assert
            Assert.NotNull(qhs);
            Assert.True(qhs.Count == 6);

            CheckEmergencyContactFields(contact: qhs[0], text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2FirstName, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[1], text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2LastName, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[2], text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[3], text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone2, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[4], text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone3, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[5], text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact2RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);
        }

        [Fact]
        public void CreateEmergencyContact3GroupTest()
        {
            // Arrange
            // Act
            var qhs = FormService.CreateEmergencyContact3Group();

            // Assert
            Assert.NotNull(qhs);
            Assert.True(qhs.Count == 6);

            CheckEmergencyContactFields(contact: qhs[0], text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3FirstName, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[1], text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3LastName, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[2], text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[3], text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone2, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[4], text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone3, options: null, isRequired: false, isActive: true);
            CheckEmergencyContactFields(contact: qhs[5], text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact3RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);
        }

        #region Helpers

        public static void CheckEmergencyContactQuestion(List<FormQuestion> formQuestions, string text, QuestionType questionTyped, QuestionDataType questionDataTypeId, string options, bool isRequired, bool isActive)
        {
            // Arrange
            var question = formQuestions.Find(m => m.QuestionDataTypeId == questionDataTypeId);

            // Assert
            Assert.NotNull(question);
            CheckEmergencyContactFields(question: question, text: text, questionTyped: questionTyped, questionDataTypeId: questionDataTypeId, options: options, isRequired: isRequired, isActive: isActive);
        }

        public static void CheckEmergencyContactFields(FormQuestion question, string text, QuestionType questionTyped, QuestionDataType questionDataTypeId, string options, bool isRequired, bool isActive)
        {
            // Assert
            Assert.NotNull(question);
            Assert.True(question.Text == text);
            Assert.True(question.QuestionTypeId == questionTyped);
            Assert.True(question.QuestionDataTypeId == questionDataTypeId);
            Assert.True(question.IsRequired == isRequired);
            Assert.True(question.IsActive == isActive);
            Assert.True(question.Options == options);
        }

        public static void CheckEmergencyContactFields(FormService.QuestionHolder contact,  string text, QuestionType questionTyped, QuestionDataType questionDataTypeId, string options, bool isRequired, bool isActive)
        {
            // Assert
            Assert.NotNull(contact);
            Assert.True(contact.Text == text);
            Assert.True(contact.QuestionTypeId == questionTyped);
            Assert.True(contact.QuestionDataTypeId == questionDataTypeId);
            Assert.True(contact.IsRequired == isRequired);
            Assert.True(contact.IsActive == isActive);
            Assert.True(contact.Options == options);
        }

        #endregion
    }
}
