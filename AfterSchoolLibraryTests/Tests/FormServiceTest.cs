using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using Xunit;
using Xunit.Abstractions;

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
            var form = FormService.AddNewAfterSchoolDefaultStudentForm(msgs, dc);

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
            ReviewQuestionsByQuestionDataTypeId(fqs, "First Name", QuestionType.TextBox, QuestionDataType.EmergencyContact1FirstName, null, true, true);

            // LastName
            ReviewQuestionsByQuestionDataTypeId(fqs, "Last Name", QuestionType.TextBox, QuestionDataType.EmergencyContact1LastName, null, true, true);

            // Phone
            ReviewQuestionsByQuestionDataTypeId(fqs, "Primary Phone", QuestionType.TextBox, QuestionDataType.EmergencyContact1Phone, null, true, true);

            // Phone2
            ReviewQuestionsByQuestionDataTypeId(fqs, "Additional Phone 1", QuestionType.TextBox, QuestionDataType.EmergencyContact1Phone2, null, false, true);

            // Phone3
            ReviewQuestionsByQuestionDataTypeId(fqs, "Additional Phone 2", QuestionType.TextBox, QuestionDataType.EmergencyContact1Phone3, null, false, true);

            // RelationshipToRegistrant
            ReviewQuestionsByQuestionDataTypeId(fqs, "Relationship To Registrant", QuestionType.DropDownList,
                QuestionDataType.EmergencyContact1RelationshipToRegistrant, FormService.RelationshipToRegistrantForEmergencyContactsList, false, true);

            // ec2
            // FirstName
            ReviewQuestionsByQuestionDataTypeId(fqs, "First Name", QuestionType.TextBox, QuestionDataType.EmergencyContact2FirstName, null, false, true);

            // LastName
            ReviewQuestionsByQuestionDataTypeId(fqs, "Last Name", QuestionType.TextBox, QuestionDataType.EmergencyContact2LastName, null, false, true);

            // Phone
            ReviewQuestionsByQuestionDataTypeId(fqs, "Primary Phone", QuestionType.TextBox, QuestionDataType.EmergencyContact2Phone, null, false, true);

            // Phone2
            ReviewQuestionsByQuestionDataTypeId(fqs, "Additional Phone 1", QuestionType.TextBox, QuestionDataType.EmergencyContact2Phone2, null, false, true);

            // Phone3
            ReviewQuestionsByQuestionDataTypeId(fqs, "Additional Phone 2", QuestionType.TextBox, QuestionDataType.EmergencyContact2Phone3, null, false, true);

            // RelationshipToRegistrant
            ReviewQuestionsByQuestionDataTypeId(fqs, "Relationship To Registrant", QuestionType.DropDownList,
                QuestionDataType.EmergencyContact2RelationshipToRegistrant, FormService.RelationshipToRegistrantForEmergencyContactsList, false, true);

            // ec3
            // FirstName
            ReviewQuestionsByQuestionDataTypeId(fqs, "First Name", QuestionType.TextBox, QuestionDataType.EmergencyContact3FirstName, null, false, true);

            // LastName
            ReviewQuestionsByQuestionDataTypeId(fqs, "Last Name", QuestionType.TextBox, QuestionDataType.EmergencyContact3LastName, null, false, true);

            // Phone
            ReviewQuestionsByQuestionDataTypeId(fqs, "Primary Phone", QuestionType.TextBox, QuestionDataType.EmergencyContact3Phone, null, false, true);

            // Phone2
            ReviewQuestionsByQuestionDataTypeId(fqs, "Additional Phone 1", QuestionType.TextBox, QuestionDataType.EmergencyContact3Phone2, null, false, true);

            // Phone3
            ReviewQuestionsByQuestionDataTypeId(fqs, "Additional Phone 2", QuestionType.TextBox, QuestionDataType.EmergencyContact3Phone3, null, false, true);

            // RelationshipToRegistrant
            ReviewQuestionsByQuestionDataTypeId(fqs, "Relationship To Registrant", QuestionType.DropDownList,
                QuestionDataType.EmergencyContact3RelationshipToRegistrant, FormService.RelationshipToRegistrantForEmergencyContactsList, false, true);

            // personal info
            // First Name
            ReviewQuestionsByQuestionDataTypeId(fqs, "First Name", QuestionType.TextBox, QuestionDataType.RegistrantFirstName, null, true, true);

            // Last Name
            ReviewQuestionsByQuestionDataTypeId(fqs, "Last Name", QuestionType.TextBox, QuestionDataType.RegistrantLastName, null, true, true);

            // Email
            ReviewQuestionsByQuestionDataTypeId(fqs, "Email", QuestionType.Email, QuestionDataType.RegistrantEmail, null, true, true);

            // Mobile Phone
            ReviewQuestionsByQuestionDataTypeId(fqs, "Mobile Phone", QuestionType.TextBox, QuestionDataType.RegistrantMobilePhone, null, false, false);

            // Home Phone
            ReviewQuestionsByQuestionDataTypeId(fqs, "Home Phone", QuestionType.TextBox, QuestionDataType.RegistrantHomePhone, null, false, false);

            // Work Phone
            ReviewQuestionsByQuestionDataTypeId(fqs, "Work Phone", QuestionType.TextBox, QuestionDataType.RegistrantWorkPhone, null, false, false);

            // Date of Birth
            ReviewQuestionsByQuestionDataTypeId(fqs, "Date of Birth", QuestionType.Date, QuestionDataType.RegistrantDateOfBirth, null, false, true);

            // Grade
            ReviewQuestionsByQuestionDataTypeId(fqs, "Grade", QuestionType.DropDownList, QuestionDataType.RegistrantGrade, FormService.RegistrantGrade, false,
                true);

            // Gender
            ReviewQuestionsByQuestionDataTypeId(fqs, "Gender", QuestionType.RadioList, QuestionDataType.RegistrantGender, FormService.RegistrantGender, false,
                true);

            // T-Shirt Size
            ReviewQuestionsByQuestionDataTypeId(fqs, "T-Shirt Size", QuestionType.RadioList, QuestionDataType.RegistrantTShirtSize,
                FormService.RegistrantTShirtSize, false, true);

            // Race
            ReviewQuestionsByQuestionDataTypeId(fqs, "Race", QuestionType.DropDownList, QuestionDataType.RegistrantRaceUsFed, FormService.RegistrantRace, false,
                true);

            // Ethnicity
            ReviewQuestionsByQuestionDataTypeId(fqs, "Ethnicity", QuestionType.DropDownList, QuestionDataType.RegistrantEthnicityUsFed,
                FormService.RegistrantEthnicity, false, true);

            // Organization
            ReviewQuestionsByQuestionDataTypeId(fqs, "Organization", QuestionType.TextBox, QuestionDataType.RegistrantOrganization, null, false, false);

            ReviewQuestionsByQuestionDataTypeId(fqs, "Student ID", QuestionType.TextBox, QuestionDataType.StudentId, null, false, false);
        }

        [Fact]
        public void CreateEmergencyContact1GroupTest()
        {
            // Arrange
            // Act
            var qhs = FormService.CreateEmergencyContact1Group(FormType.StudentForm);

            // Assert
            Assert.NotNull(qhs);
            Assert.True(qhs.Count == 6);

            ReviewEachField(qhs[0], "First Name", QuestionType.TextBox, QuestionDataType.EmergencyContact1FirstName, null, true, true);
            ReviewEachField(qhs[1], "Last Name", QuestionType.TextBox, QuestionDataType.EmergencyContact1LastName, null, true, true);
            ReviewEachField(qhs[2], "Primary Phone", QuestionType.TextBox, QuestionDataType.EmergencyContact1Phone, null, true, true);
            ReviewEachField(qhs[3], "Additional Phone 1", QuestionType.TextBox, QuestionDataType.EmergencyContact1Phone2, null, false, true);
            ReviewEachField(qhs[4], "Additional Phone 2", QuestionType.TextBox, QuestionDataType.EmergencyContact1Phone3, null, false, true);
            ReviewEachField(qhs[5], "Relationship To Registrant", QuestionType.DropDownList, QuestionDataType.EmergencyContact1RelationshipToRegistrant,
                FormService.RelationshipToRegistrantForEmergencyContactsList, false, true);
        }

        [Fact]
        public void CreateEmergencyContact2GroupTest()
        {
            // Arrange
            // Act
            var qhs = FormService.CreateEmergencyContact2Group(FormType.StudentForm);

            // Assert
            Assert.NotNull(qhs);
            Assert.True(qhs.Count == 6);

            ReviewEachField(qhs[0], "First Name", QuestionType.TextBox, QuestionDataType.EmergencyContact2FirstName, null, false, true);
            ReviewEachField(qhs[1], "Last Name", QuestionType.TextBox, QuestionDataType.EmergencyContact2LastName, null, false, true);
            ReviewEachField(qhs[2], "Primary Phone", QuestionType.TextBox, QuestionDataType.EmergencyContact2Phone, null, false, true);
            ReviewEachField(qhs[3], "Additional Phone 1", QuestionType.TextBox, QuestionDataType.EmergencyContact2Phone2, null, false, true);
            ReviewEachField(qhs[4], "Additional Phone 2", QuestionType.TextBox, QuestionDataType.EmergencyContact2Phone3, null, false, true);
            ReviewEachField(qhs[5], "Relationship To Registrant", QuestionType.DropDownList, QuestionDataType.EmergencyContact2RelationshipToRegistrant,
                FormService.RelationshipToRegistrantForEmergencyContactsList, false, true);
        }

        [Fact]
        public void CreateEmergencyContact3GroupTest()
        {
            // Arrange
            // Act
            var qhs = FormService.CreateEmergencyContact3Group(FormType.StudentForm);

            // Assert
            Assert.NotNull(qhs);
            Assert.True(qhs.Count == 6);

            ReviewEachField(qhs[0], "First Name", QuestionType.TextBox, QuestionDataType.EmergencyContact3FirstName, null, false, true);
            ReviewEachField(qhs[1], "Last Name", QuestionType.TextBox, QuestionDataType.EmergencyContact3LastName, null, false, true);
            ReviewEachField(qhs[2], "Primary Phone", QuestionType.TextBox, QuestionDataType.EmergencyContact3Phone, null, false, true);
            ReviewEachField(qhs[3], "Additional Phone 1", QuestionType.TextBox, QuestionDataType.EmergencyContact3Phone2, null, false, true);
            ReviewEachField(qhs[4], "Additional Phone 2", QuestionType.TextBox, QuestionDataType.EmergencyContact3Phone3, null, false, true);
            ReviewEachField(qhs[5], "Relationship To Registrant", QuestionType.DropDownList, QuestionDataType.EmergencyContact3RelationshipToRegistrant,
                FormService.RelationshipToRegistrantForEmergencyContactsList, false, true);
        }

        [Fact(DisplayName = "Confirm that new form has both race and ethnicity")]
        public void CreateStudentInformationGroupTest()
        {
            // Arrange & Act
            var qhs = FormService.CreateStudentInformationGroup();

            // Assert
            Assert.NotNull(qhs);

            ReviewEachField(qhs[10], "Ethnicity", QuestionType.DropDownList,
                QuestionDataType.RegistrantEthnicityUsFed, FormService.RegistrantEthnicity, false, true);

            ReviewEachField(qhs[11], "Race", QuestionType.DropDownList,
                QuestionDataType.RegistrantRaceUsFed, FormService.RegistrantRace, false, true);
        }

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

        #region Helpers

        public static void ReviewQuestionsByQuestionDataTypeId(List<FormQuestion> formQuestions, string text, QuestionType questionTyped,
            QuestionDataType questionDataTypeId, string options, bool isRequired, bool isActive)
        {
            // Arrange
            var question = formQuestions.Find(m => m.QuestionDataTypeId == questionDataTypeId);

            // Assert
            Assert.NotNull(question);
            ReviewEachField(question, text, questionTyped, questionDataTypeId, options, isRequired, isActive);
        }

        public static void ReviewEachField(FormQuestion question, string text, QuestionType questionTyped, QuestionDataType questionDataTypeId, string options,
            bool isRequired, bool isActive)
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

        public static void ReviewEachField(FormService.QuestionHolder contact, string text, QuestionType questionTyped, QuestionDataType questionDataTypeId,
            string options, bool isRequired, bool isActive)
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