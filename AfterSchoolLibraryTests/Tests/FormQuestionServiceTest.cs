using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Services;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using System.Collections.Specialized;

namespace AfterSchoolLibraryTests.Tests
{
    public class FormQuestionServiceTest : BaseTest
    {
        public FormQuestionServiceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetEmergencyContactQuestionsTest()
        {
            // Arrange & Act            
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs: msgs, dc: dc);

            // Assert
            Assert.NotNull(questions);
            Assert.True(questions.Count == 18); // 6 question for each EC and we have 3 EC

            var ec1Questions = questions.Where(m => m.FormQuestionGroupQuestionGroupDataTypeId.In(new List<QuestionGroupDataType>() { QuestionGroupDataType.EmergencyContact1 }));
            Assert.True(ec1Questions.Count() == 6);

            var ec2Questions = questions.Where(m => m.FormQuestionGroupQuestionGroupDataTypeId.In(new List<QuestionGroupDataType>() { QuestionGroupDataType.EmergencyContact2 }));
            Assert.True(ec2Questions.Count() == 6);

            var ec3Questions = questions.Where(m => m.FormQuestionGroupQuestionGroupDataTypeId.In(new List<QuestionGroupDataType>() { QuestionGroupDataType.EmergencyContact3 }));
            Assert.True(ec3Questions.Count() == 6);

            // there should not be any questions other then QuestionGroupDataType = EC groups
            var questionsOfOtherGroupTypes = questions.Where(m => m.FormQuestionGroupQuestionGroupDataTypeId.NotIn(FormQuestionService.GetEmergencyContactQuestionGroups()));

            Assert.True(questionsOfOtherGroupTypes.Count() == 0);
        }

        [Fact(DisplayName = "Proper details for Emergency Contact 1")]
        public void ValidateFormQuestionForEmergencyContactsTest1()
        {
            // Arrange     
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs: msgs, dc: dc);
            
            // proper answers for ec1
            var values = new NameValueCollection();

            var ec1FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1FirstName).FirstOrDefault();
            values.Add($"question-{ec1FirstName.ID}", "Charles");

            var ec1LastName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1LastName).FirstOrDefault();
            values.Add($"question-{ec1LastName.ID}", "Xaviers");

            var ec1Phone = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1Phone).FirstOrDefault();
            values.Add($"question-{ec1Phone.ID}", "9979437356");

            // Act
            FormQuestionService.ValidateFormQuestionForEmergencyContacts(questions: questions.Select(question => question.ToFormQuestionViewModelForService(values)).ToList(), msgs: msgs);

            // Assert
            Assert.True(msgs.HasNoErrors());            
        }

        [Fact(DisplayName = "Required field validation check for EC1 fields")]
        public void ValidateFormQuestionForEmergencyContactsTest2()
        {
            // Arrange     
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs: msgs, dc: dc);

            // proper answers for ec1
            var values = new NameValueCollection();

            var ec1FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1FirstName).FirstOrDefault();            
            var ec1LastName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1LastName).FirstOrDefault();
            var ec1Phone = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1Phone).FirstOrDefault();

            // Act
            FormQuestionService.ValidateFormQuestionForEmergencyContacts(questions: questions.Select(question => question.ToFormQuestionViewModelForService(values)).ToList(), msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"{ec1FirstName.Text} is required"));
            Assert.True(msgs.HasMessageContainingText($"{ec1LastName.Text} is required"));
            Assert.True(msgs.HasMessageContainingText($"{ec1Phone.Text} is required"));
        }

        [Fact(DisplayName = "Phone number validation check for EC1 phone")]
        public void ValidateFormQuestionForEmergencyContactsTest3()
        {
            // Arrange     
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs: msgs, dc: dc);

            // proper answers for ec1
            var values = new NameValueCollection();

            var ec1FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1FirstName).FirstOrDefault();
            values.Add($"question-{ec1FirstName.ID}", "Charles");

            var ec1LastName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1LastName).FirstOrDefault();
            values.Add($"question-{ec1LastName.ID}", "Xaviers");

            var ec1Phone = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1Phone).FirstOrDefault();
            values.Add($"question-{ec1Phone.ID}", "997943735"); // 1 number missing at the end

            // Act
            FormQuestionService.ValidateFormQuestionForEmergencyContacts(questions: questions.Select(question => question.ToFormQuestionViewModelForService(values)).ToList(), msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"{ec1Phone.FormQuestionGroupQuestionGroupDataTypeId.ToString().SplitByUpperCase()} {ec1Phone.Text} must be a valid phone number"));
        }

        [Fact(DisplayName = "Phone number should not be same validation check for emergency contacts")]
        public void ValidateFormQuestionForEmergencyContactsTest4()
        {
            // Arrange     
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs: msgs, dc: dc);

            // proper answers for ec1
            var values = new NameValueCollection();

            var ec1FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1FirstName).FirstOrDefault();
            values.Add($"question-{ec1FirstName.ID}", "Charles");

            var ec1LastName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1LastName).FirstOrDefault();
            values.Add($"question-{ec1LastName.ID}", "Xaviers");

            var ec1Phone = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1Phone).FirstOrDefault();
            values.Add($"question-{ec1Phone.ID}", "9979437356");

            var ec1Phone2 = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1Phone2).FirstOrDefault();
            values.Add($"question-{ec1Phone2.ID}", "9979437356");

            // Act
            FormQuestionService.ValidateFormQuestionForEmergencyContacts(questions: questions.Select(question => question.ToFormQuestionViewModelForService(values)).ToList(), msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"Emergency phone numbers should not be the same"));
        }

        [Fact(DisplayName = "Validation for EC2 fields, user must enter first-last name and phone numbers if they add any single field")]
        public void ValidateFormQuestionForEmergencyContactsTest5()
        {
            // Arrange     
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs: msgs, dc: dc);

            // proper answers for ec1
            var values = new NameValueCollection();

            var ec1FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1FirstName).FirstOrDefault();
            values.Add($"question-{ec1FirstName.ID}", "Charles");

            var ec1LastName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1LastName).FirstOrDefault();
            values.Add($"question-{ec1LastName.ID}", "Xaviers");

            var ec1Phone = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1Phone).FirstOrDefault();
            values.Add($"question-{ec1Phone.ID}", "9979437356");

            var ec2FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact2FirstName).FirstOrDefault();
            values.Add($"question-{ec2FirstName.ID}", "Charles");            

            // Act
            FormQuestionService.ValidateFormQuestionForEmergencyContacts(questions: questions.Select(question => question.ToFormQuestionViewModelForService(values)).ToList(), msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"Last name for contact 2 is required."));
            Assert.True(msgs.HasMessageContainingText($"Primary phone number for contact 2 is required."));
        }

        [Fact(DisplayName = "Validation for EC3 fields, user must enter first-last name and phone numbers if they add any single field")]
        public void ValidateFormQuestionForEmergencyContactsTest6()
        {
            // Arrange     
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs: msgs, dc: dc);

            // proper answers for ec1
            var values = new NameValueCollection();

            var ec1FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1FirstName).FirstOrDefault();
            values.Add($"question-{ec1FirstName.ID}", "Charles");

            var ec1LastName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1LastName).FirstOrDefault();
            values.Add($"question-{ec1LastName.ID}", "Xaviers");

            var ec1Phone = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact1Phone).FirstOrDefault();
            values.Add($"question-{ec1Phone.ID}", "9979437356");

            var ec3FirstName = questions.Where(m => m.QuestionDataTypeId == QuestionDataType.EmergencyContact3FirstName).FirstOrDefault();
            values.Add($"question-{ec3FirstName.ID}", "Charles");

            // Act
            FormQuestionService.ValidateFormQuestionForEmergencyContacts(questions: questions.Select(question => question.ToFormQuestionViewModelForService(values)).ToList(), msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"Last name for contact 3 is required."));
            Assert.True(msgs.HasMessageContainingText($"Primary phone number for contact 3 is required."));
        }

        [Fact(DisplayName = "First name is not empty")]
        public void ValidateEmergencyContactsTest1()
        {
            // Arrange & Act
            FormQuestionService.ValidateEmergencyContacts(firstName: "Charles", lastName: "", phone: "", ecNumber: 2, msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"Last name for contact 2 is required."));
            Assert.True(msgs.HasMessageContainingText($"Primary phone number for contact 2 is required."));
        }

        [Fact(DisplayName = "Last name is not empty")]
        public void ValidateEmergencyContactsTest2()
        {
            // Arrange & Act
            FormQuestionService.ValidateEmergencyContacts(firstName: "", lastName: "Xavier", phone: "", ecNumber: 2, msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"First name for contact 2 is required."));
            Assert.True(msgs.HasMessageContainingText($"Primary phone number for contact 2 is required."));
        }

        [Fact(DisplayName = "Phone is not empty")]
        public void ValidateEmergencyContactsTest3()
        {
            // Arrange & Act
            FormQuestionService.ValidateEmergencyContacts(firstName: "", lastName: "", phone: "9979437546", ecNumber: 2, msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"First name for contact 2 is required."));
            Assert.True(msgs.HasMessageContainingText($"Last name for contact 2 is required."));
        }

        [Fact(DisplayName = "First name is empty")]
        public void ValidateEmergencyContactsTest4()
        {
            // Arrange & Act
            FormQuestionService.ValidateEmergencyContacts(firstName: "", lastName: "Xavier", phone: "9979437546", ecNumber: 2, msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"First name for contact 2 is required."));            
        }

        [Fact(DisplayName = "Last name is empty")]
        public void ValidateEmergencyContactsTest5()
        {
            // Arrange & Act
            FormQuestionService.ValidateEmergencyContacts(firstName: "Charles", lastName: "", phone: "9979437546", ecNumber: 3, msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"Last name for contact 3 is required."));
        }

        [Fact(DisplayName = "Phone is empty")]
        public void ValidateEmergencyContactsTest()
        {
            // Arrange & Act
            FormQuestionService.ValidateEmergencyContacts(firstName: "Charles", lastName: "Xavier", phone: "", ecNumber: 1, msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors());
            Assert.True(msgs.HasMessageContainingText($"Primary phone number for contact 1 is required."));
        }

    }
}
