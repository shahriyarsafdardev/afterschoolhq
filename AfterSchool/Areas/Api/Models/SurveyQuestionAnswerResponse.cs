using System;

namespace AfterSchool.Areas.Api.Models
{
    public class SurveyQuestionAnswerResponse
    {
        public Guid? surveyResponseUid { get; set; }
        public Guid? formUid { get; set; }
        public string groupId { get; set; }
        public string answers { get; set; }
        public bool? isLastSection { get; set; }
    }
}