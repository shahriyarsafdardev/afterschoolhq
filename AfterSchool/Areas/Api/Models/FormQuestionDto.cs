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
        public bool IsLocked { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<FormQuestionSettingDto> QuestionSettings { get; set; }
        public bool IsAnswerDescriptionEnabled { get; set; }
        public QuestionType QuestionTypeId { get; set; }
        public QuestionDataType QuestionDataTypeId { get; set; }
        public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get; set; }

        public void UpdateNonDefaultFields(FormQuestion Question)
        {
            Question.Text = this.Text;
            Question.Description = this.Description;
            Question.Options = this.Options;
            Question.Link = this.Link;
            Question.QuestionTypeId = this.QuestionTypeId;
            Question.IsDeleted = this.IsDeleted;
            Question.IsLocked = this.IsLocked;
        }

        public void UpdateDefaultFields(FormQuestion Question)
        {
            Question.IsActive = IsActive;
            Question.IsRequired = IsRequired;
            Question.IsAnswerDescriptionEnabled = IsAnswerDescriptionEnabled;
            Question.DisplayOrder = DisplayOrder;
        }

        public static FormQuestionDto MapToDto(FormQuestion Question)
        {
            return new FormQuestionDto()
            {
                ID = Question.ID ?? 0,
                FormQuestionGroupId = Question.FormQuestionGroupId,
                Text = Question.Text,
                Description = Question.Description,
                GroupTitle = Question.FormQuestionGroupName,
                Options = Question.Options,
                IsRequired = Question.IsRequired,
                DisplayOrder = Question.DisplayOrder,
                IsActive = Question.IsActive,
                IsDeleted = Question.IsDeleted,
                QuestionTypeId = Question.QuestionTypeId,
                QuestionDataTypeId = Question.QuestionDataTypeId,
                FormQuestionGroupQuestionGroupDataTypeId = Question.FormQuestionGroupQuestionGroupDataTypeId,
                QuestionSettings = FormQuestionSettingDto.MapToDtoList(FormQuestionSettings.LoadByQuestionIds(Question.ID), Question.ID??0),
                IsLocked = Question.IsLocked,
            };
        }

        public static List<FormQuestionDto> MapToDtoList(List<FormQuestion> QuestionList)
        {
            return QuestionList
                .Select(question=>
                MapToDto(question))
                .ToList();
        }

    }
}