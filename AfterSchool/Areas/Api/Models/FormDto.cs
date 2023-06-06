using System.Collections.Generic;

namespace AfterSchool.Areas.Api.Models
{
    public class FormDto
    {
        public List<FormQuestionDto> formQuestion { get; set; }
        public List<FormQuestionGroupDto> formQuestionGroup { get; set; }
        public List<FormQuestionAnswerDto> formQuestionAnswer { get; set; }
        public List<SurveyFormQuestionAnswerDto> surveyFormQuestionAnswer { get; set; }

        public FormDto(List<FormQuestionDto> formQuestion, List<FormQuestionGroupDto> formQuestionGroup, List<FormQuestionAnswerDto> formQuestionAnswer)
        {
            this.formQuestion = formQuestion;
            this.formQuestionGroup = formQuestionGroup;
            this.formQuestionAnswer = formQuestionAnswer;
        }

        public FormDto(List<FormQuestionDto> formQuestion, List<FormQuestionGroupDto> formQuestionGroup, List<SurveyFormQuestionAnswerDto> surveyFormQuestionAnswer)
        {
            this.formQuestion = formQuestion;
            this.formQuestionGroup = formQuestionGroup;
            this.surveyFormQuestionAnswer = surveyFormQuestionAnswer;
        }
        
        public FormDto(List<FormQuestionDto> formQuestion, List<FormQuestionGroupDto> formQuestionGroup)
        {
            this.formQuestion = formQuestion;
            this.formQuestionGroup = formQuestionGroup;
        }
    }
}