using AfterSchoolLibrary.Models;
using System.Collections.Generic;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionDto
    {
        public int ID { get; set; }
        public int? FormQuestionGroupId { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string GroupTitle { get; set; }
        public string Options { get; set; }
        public bool IsRequired { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public QuestionType QuestionTypeId { get; set; }
        public QuestionDataType QuestionDataTypeId { get; set; }
        public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get; set; }

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
                FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId
            };
        }

        public static List<FormQuestionDto> GetByFormQuestionList(List<FormQuestion> questionList)
        {
            var questionGroupDtoList = new List<FormQuestionDto>();
            foreach (var question in questionList)
                questionGroupDtoList.Add(GetByFormQuestion(question));
            return questionGroupDtoList;
        }
    }
}