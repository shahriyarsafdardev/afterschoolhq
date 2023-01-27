using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using System.Collections.Generic;

namespace AfterSchool.Models
{
    public class ReportPageResults
    {
        public Form Form { get; set; }
        public int? FormId { get; set; }
        public List<Activity> SelectedActivities { get; set; }
        public List<Activity> ActivityOptions { get; set; }
        public List<FormQuestion> Questions { get; set; }
        public SystemMessages Messages { get; set; }
        public bool CollapseRecords { get; set; }
        public RegistrantPersonType PersonType { get; set; }
        public bool ShowCollapseRecordOption { get; set; }
    }
    public class ReportPageDataResult
    {
        public string ID { get; set; }
        public string FirstRegistrantId { get; set; }
        public string RegistrantId { get; set; }
        public string SurveyResponseId { get; set; }
        public string SubmissionDate { get; set; }
        public string Activities { get; set; }
        public string RecordId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string School { get; set; }
    }

}