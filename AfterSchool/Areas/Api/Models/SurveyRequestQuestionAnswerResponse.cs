using System;

namespace AfterSchool.Areas.Api.Models
{
    public class SurveyRequestQuestionAnswerResponse
    {
        public string surveyRequestUid { get; set; }
        public int pin { get; set; }
        public string groupId { get; set; }
        public string answers { get; set; }
        public bool? isLastSection { get; set; }
    }
}