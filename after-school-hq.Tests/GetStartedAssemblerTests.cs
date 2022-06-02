using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using AfterSchool.Controllers;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                Email = "test@example.com",
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
                },
                new FormQuestion
                {
                    ID = 11,
                    Text = "Student ID",
                    QuestionDataTypeId = QuestionDataType.StudentId,
                }
            };
            var result = assembler.FilterAndTransformQuestionsForParentRegistration(user, formQuestions, new NameValueCollection());
            var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
            {
                Error = (sender, args) => args.ErrorContext.Handled = true,
                ContractResolver = new IgnorePropertiesResolver(new[] {"LastModifiedDate", "CreateDate", "Prop"}),
            });
            Approvals.Verify(json);
        }
    }

    public class IgnorePropertiesResolver : DefaultContractResolver
    {
        private readonly HashSet<string> ignoreProps;

        public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
        {
            ignoreProps = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (ignoreProps.Contains(property.PropertyName)) property.ShouldSerialize = _ => false;
            return property;
        }
    }
}