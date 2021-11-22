using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Collections;

namespace AfterSchoolLibraryTests.Tests
{
    public class GradeLevelServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account HostAccount { get; set; }
        private Form DefaultForm { get; set; }
        private FormQuestionGroup DefaultFormQuestionGroup { get; set; }
        private FormQuestion DefaultFormQuestion { get; set; }
        private List<GradeLevel> DefaultGradeLevels { get; set; }

        public GradeLevelServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            HostAccount = AccountFactory.Create(AccountOwner, msgs, dc);
            DefaultForm = FormFactory.Create(HostAccount, msgs, dc);
            DefaultFormQuestionGroup = FormQuestionGroupFactory.Create(DefaultForm.ID.Value, msgs, dc);
            DefaultFormQuestion = FormQuestionFactory.Create(DefaultForm.ID.Value, DefaultFormQuestionGroup.ID.Value, "test grade question", QuestionDataType.RegistrantGrade, QuestionType.DropDownList, msgs, dc);
            DefaultGradeLevels = GradeLevelFactory.CreateAllGradesLevels(msgs, dc);
        }

        [Fact(DisplayName = "VerifyGradeLevelTest")]
        public void VerifyGradeLevelTest()
        {
            // Arrange
            var verifiedTextformQuestionAnswer = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "K", QuestionDataType.RegistrantGrade, msgs, dc );
            var verifiedJsonFormQuestionAnswer = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"Kindergarten\",\"value\":\"K\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedTextFormQuestionAnswer = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "Q", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedJsonFormQuestionAnswer = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"Smindergarten\",\"value\":\"S\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var gradeLevel = GradeLevel.Load(new Hashtable() {
                    { "Name", "Kindergarten" } },
                    null,
                    null,
                    msgs,
                    dc ).First();

            // Act
            var verifiedTextGradeLevel = gradeLevel.VerifyGradeLevel(verifiedTextformQuestionAnswer);
            var verifiedJsonGradeLevel = gradeLevel.VerifyGradeLevel(verifiedJsonFormQuestionAnswer);
            var nonVerifiedGradeLevel = gradeLevel.VerifyGradeLevel(nonVerifiedTextFormQuestionAnswer);
            var nonVerifiedJsonGradeLevel = gradeLevel.VerifyGradeLevel(nonVerifiedJsonFormQuestionAnswer);

            // Assert
            Assert.True(verifiedTextGradeLevel);
            Assert.True(verifiedJsonGradeLevel);
            Assert.False(nonVerifiedGradeLevel);
            Assert.False(nonVerifiedJsonGradeLevel);
           
        }

    }
}
