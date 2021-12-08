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
            var verifiedTextformQuestionAnswerPreK = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "Pre-K", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswerPreK = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"Pre-K\",\"value\":\"Pre-K\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "K", QuestionDataType.RegistrantGrade, msgs, dc );
            var verifiedJsonFormQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"Kindergarten\",\"value\":\"K\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer9th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "9th", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer9th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"9th\",\"value\":\"9th\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer10th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "10th", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer10th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"10th\",\"value\":\"10th\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer11th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "11th", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer11th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"11th\",\"value\":\"11th\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer12th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "12th", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer12th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"12th\",\"value\":\"12th\"}]", QuestionDataType.RegistrantGrade, msgs, dc);

            var nonVerifiedTextFormQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "Q", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedJsonFormQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"Smindergarten\",\"value\":\"S\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedTextFormQuestionAnswer13th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "13th", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedJsonFormQuestionAnswer13th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"13th\",\"value\":\"13th\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedTextFormQuestionAnswer99th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "99th", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedJsonFormQuestionAnswer99th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"99th\",\"value\":\"99th\"}]", QuestionDataType.RegistrantGrade, msgs, dc);


            var gradeLevelPreK = DefaultGradeLevels.Where(g => g.Name == "Pre-K").FirstOrDefault();
            var gradeLevelKindergarten = DefaultGradeLevels.Where(g => g.Name == "Kindergarten").FirstOrDefault();
            var gradeLevel9th = DefaultGradeLevels.Where(g => g.Name == "9th").FirstOrDefault();
            var gradeLevel10th = DefaultGradeLevels.Where(g => g.Name == "10th").FirstOrDefault();
            var gradeLevel11th = DefaultGradeLevels.Where(g => g.Name == "11th").FirstOrDefault();
            var gradeLevel12th = DefaultGradeLevels.Where(g => g.Name == "12th").FirstOrDefault();

            // Act
            var verifiedTextGradeLevelPreK = gradeLevelPreK.VerifyGradeLevel(verifiedTextformQuestionAnswerPreK);
            var verifiedJsonGradeLevelPreK = gradeLevelPreK.VerifyGradeLevel(verifiedJsonFormQuestionAnswerPreK);
            var verifiedTextGradeLevelKindergarten = gradeLevelKindergarten.VerifyGradeLevel(verifiedTextformQuestionAnswerKindergarten);
            var verifiedJsonGradeLevelKindergarten = gradeLevelKindergarten.VerifyGradeLevel(verifiedJsonFormQuestionAnswerKindergarten);
            var verifiedTextGradeLevel9th = gradeLevel9th.VerifyGradeLevel(verifiedTextformQuestionAnswer9th);
            var verifiedJsonGradeLevel9th = gradeLevel9th.VerifyGradeLevel(verifiedJsonFormQuestionAnswer9th);
            var verifiedTextGradeLevel10th = gradeLevel10th.VerifyGradeLevel(verifiedTextformQuestionAnswer10th);
            var verifiedJsonGradeLevel10th = gradeLevel10th.VerifyGradeLevel(verifiedJsonFormQuestionAnswer10th);
            var verifiedTextGradeLevel11th = gradeLevel11th.VerifyGradeLevel(verifiedTextformQuestionAnswer11th);
            var verifiedJsonGradeLevel11th = gradeLevel11th.VerifyGradeLevel(verifiedJsonFormQuestionAnswer11th);
            var verifiedTextGradeLevel12th = gradeLevel12th.VerifyGradeLevel(verifiedTextformQuestionAnswer12th);
            var verifiedJsonGradeLevel12th = gradeLevel12th.VerifyGradeLevel(verifiedJsonFormQuestionAnswer12th);

            var nonVerifiedGradeLevelKindergarten = gradeLevelKindergarten.VerifyGradeLevel(nonVerifiedTextFormQuestionAnswerKindergarten);
            var nonVerifiedJsonGradeLevelKindergarten = gradeLevelKindergarten.VerifyGradeLevel(nonVerifiedJsonFormQuestionAnswerKindergarten);
            var nonVerifiedGradeLevel13th = gradeLevel12th.VerifyGradeLevel(nonVerifiedTextFormQuestionAnswer13th);
            var nonVerifiedJsonGradeLevel13th = gradeLevel12th.VerifyGradeLevel(nonVerifiedJsonFormQuestionAnswer13th);
            var nonVerifiedGradeLevel99th = gradeLevel9th.VerifyGradeLevel(nonVerifiedTextFormQuestionAnswer99th);
            var nonVerifiedJsonGradeLevel99th = gradeLevel9th.VerifyGradeLevel(nonVerifiedJsonFormQuestionAnswer99th);

            // Assert
            Assert.True(verifiedTextGradeLevelPreK);
            Assert.True(verifiedJsonGradeLevelPreK);
            Assert.True(verifiedTextGradeLevelKindergarten);
            Assert.True(verifiedJsonGradeLevelKindergarten);
            Assert.True(verifiedTextGradeLevel9th);
            Assert.True(verifiedJsonGradeLevel9th);
            Assert.True(verifiedTextGradeLevel10th);
            Assert.True(verifiedJsonGradeLevel10th);
            Assert.True(verifiedTextGradeLevel11th);
            Assert.True(verifiedJsonGradeLevel11th);
            Assert.True(verifiedTextGradeLevel12th);
            Assert.True(verifiedJsonGradeLevel12th);

            Assert.False(nonVerifiedGradeLevelKindergarten);
            Assert.False(nonVerifiedJsonGradeLevelKindergarten);
            Assert.False(nonVerifiedGradeLevel13th);
            Assert.False(nonVerifiedJsonGradeLevel13th);
            Assert.False(nonVerifiedGradeLevel99th);
            Assert.False(nonVerifiedJsonGradeLevel99th);


        }

    }
}
