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
            var personalInformationGroupType = questionGroups.Find(m => m.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            Assert.NotNull(emergencyContact1GroupType);
            Assert.NotNull(emergencyContact2GroupType);
            Assert.NotNull(emergencyContact3GroupType);
            Assert.NotNull(personalInformationGroupType);

            // check questions for emergency contacts
            var fqs = form.GetAllQuestions(msgs, dc);
            Assert.NotNull(fqs);
            Assert.True(fqs.Count > 0);
            
            // ec1
            // FirstName
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1FirstName, options: null, isRequired: true, isActive: true);

            // LastName
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1LastName, options: null, isRequired: true, isActive: true);

            // Phone
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone, options: null, isRequired: true, isActive: true);

            // Phone2
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone2, options: null, isRequired: false, isActive: true);

            // Phone3
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone3, options: null, isRequired: false, isActive: true);

            // RelationshipToRegistrant
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact1RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);

            // ec2
            // FirstName
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2FirstName, options: null, isRequired: false, isActive: true);

            // LastName
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2LastName, options: null, isRequired: false, isActive: true);

            // Phone
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone, options: null, isRequired: false, isActive: true);

            // Phone2
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone2, options: null, isRequired: false, isActive: true);

            // Phone3
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone3, options: null, isRequired: false, isActive: true);

            // RelationshipToRegistrant
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact2RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);

            // ec3
            // FirstName
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3FirstName, options: null, isRequired: false, isActive: true);

            // LastName
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3LastName, options: null, isRequired: false, isActive: true);

            // Phone
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone, options: null, isRequired: false, isActive: true);

            // Phone2
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone2, options: null, isRequired: false, isActive: true);

            // Phone3
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone3, options: null, isRequired: false, isActive: true);

            // RelationshipToRegistrant
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact3RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);

            // personal info
            // First Name
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.RegistrantFirstName, options: null, isRequired: true, isActive: true);

            // Last Name
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.RegistrantLastName, options: null, isRequired: true, isActive: true);

            // Email
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Email", questionTyped: QuestionType.Email, questionDataTypeId: QuestionDataType.RegistrantEmail, options: null, isRequired: true, isActive: true);

            // Mobile Phone
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Mobile Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.RegistrantMobilePhone, options: null, isRequired: false, isActive: false);

            // Home Phone
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Home Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.RegistrantHomePhone, options: null, isRequired: false, isActive: false);

            // Work Phone
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Work Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.RegistrantWorkPhone, options: null, isRequired: false, isActive: false);

            // Date of Birth
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Date of Birth", questionTyped: QuestionType.Date, questionDataTypeId: QuestionDataType.RegistrantDateOfBirth, options: null, isRequired: false, isActive: true);

            // Grade
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Grade", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.RegistrantGrade, options: FormService.RegistrantGrade, isRequired: false, isActive: true);

            // Gender
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Gender", questionTyped: QuestionType.RadioList, questionDataTypeId: QuestionDataType.RegistrantGender, options: FormService.RegistrantGender, isRequired: false, isActive: true);

            // T-Shirt Size
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "T-Shirt Size", questionTyped: QuestionType.RadioList, questionDataTypeId: QuestionDataType.RegistrantTShirtSize, options: FormService.RegistrantTShirtSize, isRequired: false, isActive: true);

            // Race
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Race", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.RegistrantRaceUsFed, options: FormService.RegistrantRace, isRequired: false, isActive: true);

            // Ethnicity
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Ethnicity", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.RegistrantEthnicityUsFed, options: FormService.RegistrantEthnicity, isRequired: false, isActive: true);

            // Organization
            ReviewQuestionsByQuestionDataTypeId(formQuestions: fqs, text: "Organization", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.RegistrantOrganization, options: null, isRequired: false, isActive: false);
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

            ReviewEachField(contact: qhs[0], text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1FirstName, options: null, isRequired: true, isActive: true);
            ReviewEachField(contact: qhs[1], text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1LastName, options: null, isRequired: true, isActive: true);
            ReviewEachField(contact: qhs[2], text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone, options: null, isRequired: true, isActive: true);
            ReviewEachField(contact: qhs[3], text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone2, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[4], text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact1Phone3, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[5], text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact1RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);
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

            ReviewEachField(contact: qhs[0], text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2FirstName, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[1], text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2LastName, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[2], text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[3], text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone2, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[4], text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact2Phone3, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[5], text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact2RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);
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

            ReviewEachField(contact: qhs[0], text: "First Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3FirstName, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[1], text: "Last Name", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3LastName, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[2], text: "Primary Phone", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[3], text: "Additional Phone 1", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone2, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[4], text: "Additional Phone 2", questionTyped: QuestionType.TextBox, questionDataTypeId: QuestionDataType.EmergencyContact3Phone3, options: null, isRequired: false, isActive: true);
            ReviewEachField(contact: qhs[5], text: "Relationship To Registrant", questionTyped: QuestionType.DropDownList, questionDataTypeId: QuestionDataType.EmergencyContact3RelationshipToRegistrant, options: FormService.RelationshipToRegistrantForEmergencyContactsList, isRequired: false, isActive: true);
        }

        [Fact(DisplayName = "Confirm that new form has both race and ethnicity")]
        public void CreateStudentInformationGroupTest()
        {
            // Arrange & Act
            var qhs = FormService.CreateStudentInformationGroup();

            // Assert
            Assert.NotNull(qhs);

            ReviewEachField(contact: qhs[10], text: "Ethnicity", questionTyped: QuestionType.DropDownList,
                questionDataTypeId: QuestionDataType.RegistrantEthnicityUsFed, options: FormService.RegistrantEthnicity, isRequired: false, isActive: true);

            ReviewEachField(contact: qhs[11], text: "Race", questionTyped: QuestionType.DropDownList,
                questionDataTypeId: QuestionDataType.RegistrantRaceUsFed, options: FormService.RegistrantRace, isRequired: false, isActive: true);
        }

        #region Helpers

        public static void ReviewQuestionsByQuestionDataTypeId(List<FormQuestion> formQuestions, string text, QuestionType questionTyped, QuestionDataType questionDataTypeId, string options, bool isRequired, bool isActive)
        {
            // Arrange
            var question = formQuestions.Find(m => m.QuestionDataTypeId == questionDataTypeId);

            // Assert
            Assert.NotNull(question);
            ReviewEachField(question: question, text: text, questionTyped: questionTyped, questionDataTypeId: questionDataTypeId, options: options, isRequired: isRequired, isActive: isActive);
        }

        public static void ReviewEachField(FormQuestion question, string text, QuestionType questionTyped, QuestionDataType questionDataTypeId, string options, bool isRequired, bool isActive)
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

        public static void ReviewEachField(FormService.QuestionHolder contact,  string text, QuestionType questionTyped, QuestionDataType questionDataTypeId, string options, bool isRequired, bool isActive)
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

        [Theory]
        [InlineData("1234", 1)]
        [InlineData("9024881888", 0)]
        public void PhoneValidationTest(string phoneNumber, int count)
        {
            var formQuestion = new FormQuestion();
            formQuestion.QuestionDataTypeId = QuestionDataType.RegistrantHomePhone;
            FormService.PhoneValidation(phoneNumber, formQuestion, msgs);

            Assert.Equal(msgs.Count, count);
        }
    }
}
