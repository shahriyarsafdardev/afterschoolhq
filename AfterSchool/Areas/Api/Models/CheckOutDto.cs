using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.Api.Models
{
    public class CheckOutDto
    {
        public string ActivityName { get; set; }
        public string DaysOfWeek { get; set; }
        public string MinGrade { get; set; }
        public string MaxGrade { get; set; }
        public bool IsGradeRestricted { get; set; }
        public bool IsAgeRestricted { get; set; }
        public bool IsGenderRestricted { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public GenderRestrictionType EnforcedGender { get; set; }
    }
}