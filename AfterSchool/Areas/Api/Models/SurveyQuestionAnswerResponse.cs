namespace AfterSchool.Areas.Api.Models
{
    public class SurveyQuestionAnswerResponse
    {
        public int surveyResponseId { get; set; }
        public int formId { get; set; }
        public string answers { get; set; }
    }
}