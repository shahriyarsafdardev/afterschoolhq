using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.Api.Models
{
    public class CheckInOutAttendanceDto
    {
        public int Id { get; set; }
        public CheckType CheckType { get; set; }
    }
}