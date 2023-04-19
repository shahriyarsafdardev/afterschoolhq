namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionAnswerDto
    {
        public int? FormQuestionId { get; set; }
        public int QuestionDataTypeId { get; set; }
        public string QuestionText { get; set; }
        public string Text { get; set; }
    }
}