using System.Collections.Generic;
using System.Collections.Specialized;
using AfterSchool.Controllers;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class GetStartedAssemblerTests
    {
        [Test]
        public void FilterAndTransformQuestionsForParentRegistrationTest()
        {
            var featureFlagRepository = new Mock<IFeatureFlagRepository>();
            var assembler = new GetStartedAssembler(featureFlagRepository.Object);
            var user = new WebUser
            {
                FirstName = "Test",
                LastName = "Testerson",
                Phone = "404-555-1234",
                Email = "test@example.com"
            };
            var formQuestions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    ID = 1,
                    Text = "First Name",
                    QuestionDataTypeId = QuestionDataType.RegistrantFirstName,
                },
                new FormQuestion
                {
                    ID = 2,
                    Text = "Last Name",
                    QuestionDataTypeId = QuestionDataType.RegistrantLastName,
                },
                new FormQuestion
                {
                    ID = 3,
                    Text = "Email",
                    QuestionDataTypeId = QuestionDataType.RegistrantEmail,
                },
                new FormQuestion
                {
                    ID = 4,
                    Text = "Mobile Phone",
                    QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone,
                },
                new FormQuestion
                {
                    ID = 10,
                    Text = "Question to remove",
                    QuestionDataTypeId = QuestionDataType.RegistrantGrade, 
                }
            };
            var result = assembler.FilterAndTransformQuestionsForParentRegistration(user, formQuestions, new NameValueCollection());
            Approvals.Verify(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings {Error = (sender, args) => args.ErrorContext.Handled = true}));
        }
    }
}