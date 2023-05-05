using System.Collections.Generic;

namespace AfterSchool.Areas.Api.Models
{
    public class FormDto
    {
        public List<FormQuestionDto> FormQuestion { get; set; }
        public List<FormQuestionGroupDto> FormQuestionGroup { get; set; }
        public List<FormQuestionAnswerDto> FormQuestionAnswer { get; set; }

        public FormDto(List<FormQuestionDto> formQuestion, List<FormQuestionGroupDto> formQuestionGroup, List<FormQuestionAnswerDto> formQuestionAnswer)
        {
            this.FormQuestion = formQuestion;
            this.FormQuestionGroup = formQuestionGroup;
            this.FormQuestionAnswer = formQuestionAnswer;
        }
    }
}