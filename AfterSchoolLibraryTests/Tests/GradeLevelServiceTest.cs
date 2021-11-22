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
            var verifiedTextformQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "K", QuestionDataType.RegistrantGrade, msgs, dc );
            var verifiedJsonFormQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"Kindergarten\",\"value\":\"K\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer9th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "9", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer9th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"9th\",\"value\":\"9\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer10th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "10", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer10th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"10th\",\"value\":\"10\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer11th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "11th", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer11th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"11th\",\"value\":\"11\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedTextformQuestionAnswer12th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "12", QuestionDataType.RegistrantGrade, msgs, dc);
            var verifiedJsonFormQuestionAnswer12th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"12th\",\"value\":\"12\"}]", QuestionDataType.RegistrantGrade, msgs, dc);

            var nonVerifiedTextFormQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "Q", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedJsonFormQuestionAnswerKindergarten = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"Smindergarten\",\"value\":\"S\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedTextFormQuestionAnswer13th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "13", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedJsonFormQuestionAnswer13th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"13th\",\"value\":\"13\"}]", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedTextFormQuestionAnswer99th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "99", QuestionDataType.RegistrantGrade, msgs, dc);
            var nonVerifiedJsonFormQuestionAnswer99th = FormQuestionAnswerFactory.Create(DefaultFormQuestion.ID.Value, "test grade question", "[{ \"key\":\"99th\",\"value\":\"99\"}]", QuestionDataType.RegistrantGrade, msgs, dc);



            var gradeLevelKindergarten = GradeLevel.Load(new Hashtable() {
                    { "Name", "Kindergarten" } },
                    null,
                    null,
                    msgs,
                    dc ).First();
            var gradeLevel9th = GradeLevel.Load(new Hashtable() {
                    { "Name", "9th" } },
                    null,
                    null,
                    msgs,
                    dc).First();
            var gradeLevel10th = GradeLevel.Load(new Hashtable() {
                    { "Name", "10th" } },
                    null,
                    null,
                    msgs,
                    dc).First();
            var gradeLevel11th = GradeLevel.Load(new Hashtable() {
                    { "Name", "11th" } },
                    null,
                    null,
                    msgs,
                    dc).First();
            var gradeLevel12th = GradeLevel.Load(new Hashtable() {
                    { "Name", "12th" } },
                    null,
                    null,
                    msgs,
                    dc).First();

            // Act
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
