using AfterSchoolLibrary.Models;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionDto
    {
        public int ID { get; set; }
        public int? FormQuestionGroupId { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string GroupTitle { get; set; }
        public string Options { get; set; }
        public bool IsRequired { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<FormQuestionSettingDto> questionSettings { get; set; }
        public bool IsAnswerDescriptionEnabled { get; set; }
        public QuestionType QuestionTypeId { get; set; }
        public QuestionDataType QuestionDataTypeId { get; set; }
        public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get; set; }

        public void UpdateQuestionIfNotEmpty(FormQuestion question)
        {
            question.ID = this.ID;
            question.FormQuestionGroupId = this.FormQuestionGroupId ?? question.FormQuestionGroupId;
            question.Text = this.Text;
            question.Description = this.Description;
            question.Options = this.Options;
            question.IsRequired = this.IsRequired;
            question.DisplayOrder = this.DisplayOrder;
            question.IsActive = this.IsActive;
            question.IsDeleted = this.IsDeleted;
            question.QuestionTypeId = this.QuestionTypeId;
            question.QuestionDataTypeId = this.QuestionDataTypeId;
            question.IsAnswerDescriptionEnabled = this.IsAnswerDescriptionEnabled;
            question.Link = this.Link;
        }

        public static FormQuestionDto GetByFormQuestion(FormQuestion question)
        {
            return new FormQuestionDto()
            {
                ID = question.ID ?? 0,
                FormQuestionGroupId = question.FormQuestionGroupId,
                Text = question.Text,
                Description = question.Description,
                GroupTitle = question.FormQuestionGroupName,
                Options = question.Options,
                IsRequired = question.IsRequired,
                DisplayOrder = question.DisplayOrder,
                IsActive = question.IsActive,
                IsDeleted = question.IsDeleted,
                QuestionTypeId = question.QuestionTypeId,
                QuestionDataTypeId = question.QuestionDataTypeId,
                FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId,
                questionSettings = FormQuestionSettingDto.fromFormSettingsList(FormQuestionSettings.LoadByQuestionIds(question.ID)),
            };
        }

        public static List<FormQuestionDto> GetByFormQuestionList(List<FormQuestion> questionList)
        {
            return questionList
                .Select(question=>
                GetByFormQuestion(question))
                .ToList();
        }

    }
}