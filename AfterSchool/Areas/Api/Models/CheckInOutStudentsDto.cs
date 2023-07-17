namespace AfterSchool.Areas.Api.Models
{
    public class CheckInOutStudentsDto
    {
        public string StudentUid { get; set; }
        public string ActivityUid { get; set; }
        public bool CheckInOut { get; set; }
    }
}