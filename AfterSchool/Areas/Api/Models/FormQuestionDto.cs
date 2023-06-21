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

        public void UpdateNonDefaultFields(FormQuestion question)
        {
            question.Text = this.Text;
            question.Description = this.Description;
            question.Options = this.Options;
            question.Link = this.Link;
            question.QuestionTypeId = this.QuestionTypeId;
            question.IsDeleted = this.IsDeleted;
            question.IsLocked = this.IsLocked;
        }

        public void UpdateDefaultFields(FormQuestion question)
        {
            question.IsActive = IsActive;
            question.IsRequired = IsRequired;
            question.IsAnswerDescriptionEnabled = IsAnswerDescriptionEnabled;
            question.DisplayOrder = DisplayOrder;
        }

        public FormQuestionDto changeDispOrder(int dispOrder)
        {
            DisplayOrder = dispOrder;
            return this;
        }

        public static List<FormQuestionDto> validateByDisplayOrder(List<FormQuestionDto> formQuestionsToValidate)
        {
            var validatedQuestions = new List<FormQuestionDto>();
            validatedQuestions.AddRange(formQuestionsToValidate.Where(x=>x.IsLocked).ToList().OrderBy(x=>x.DisplayOrder).Select((obj,index)=>obj.changeDispOrder(index+1)));
            foreach(var question in formQuestionsToValidate.Where(x => !x.IsLocked).ToList())
                validatedQuestions.Add(question.changeDispOrder(validatedQuestions.Count + 1));
            return validatedQuestions;
        }

        public static FormQuestionDto MapToDto(FormQuestion question, FormType formType = FormType.NotSet)
        {
            return new FormQuestionDto()
            {
                ID = question.ID ?? 0,
                FormQuestionGroupId = question.FormQuestionGroupId,
                Text = (question.QuestionTypeId == QuestionType.TextBlock ? question.Description : question.Text),
                Description = (question.QuestionTypeId == QuestionType.TextBlock ? question.Text : question.Description),
                GroupTitle = question.FormQuestionGroupName,
                Options = question.Options,
                IsRequired = question.IsRequired,
                DisplayOrder = question.DisplayOrder,
                IsActive = question.IsActive,
                IsDeleted = question.IsDeleted,
                QuestionTypeId = question.QuestionTypeId,
                QuestionDataTypeId = question.QuestionDataTypeId,
                FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId,
                QuestionSettings = FormQuestionSettingDto.MapToDtoList(FormQuestionSettings.LoadByQuestionIds(question.ID), question.ID??0,formType),
                IsLocked = question.IsLocked,
            };
        }

        public static List<FormQuestionDto> MapToDtoList(List<FormQuestion> questionList, FormType formType = FormType.NotSet)
        {
            return questionList
                .Select(question=>
                MapToDto(question, formType))
                .ToList();
        }

    }
}