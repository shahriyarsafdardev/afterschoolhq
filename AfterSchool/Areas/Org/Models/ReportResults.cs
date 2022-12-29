using AfterSchoolLibrary.Models;
using BasicLibrary;
using System.Collections.Generic;

namespace AfterSchool.Models
{
    public class ReportResults
    {
        public Form Form { get; set; }
        public int? FormId { get; set; }
        public List<Activity> SelectedActivities { get; set; }
        public List<Activity> ActivityOptions { get; set; }
        public string ActivityUid { get; set; }
        public List<FormQuestion> Questions { get; set; }
        public SystemMessages Messages { get; set; }
    }

}