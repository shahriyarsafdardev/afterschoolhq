using System;

namespace AfterSchool.Models
{
    public class SearchDirectoryResponse
    {
        public Guid AccountUid { get; set; }
        public string AccountName { get; set; }
        public string HomePageUrl { get; set; }

        public string ProfileHeaderImageUrl { get; set; }
        public string Location { get; set; }
        public string ProfileHeadline { get; set; }

        public bool IsOpenDuringCovid { get; set; }
        public bool IsELearningCenter { get; set; }
        public bool IsAccountNowEnrolling { get; set; }
        public bool IsVirtualProgram { get; set; }

        public bool IsRestrictByGrade { get; set; }
        public string GradeStart { get; set; }
        public string GradeEnd { get; set; }
        public string AgeStart { get; set; }
        public string AgeEnd { get; set; }

        public int Priority { get; set; }
        public decimal TotalDifference { get; set; }
        public string CostLow { get; set; }
        public string CostHigh { get; set; }
    }
}