using System.Collections.Generic;

namespace AfterSchool.Areas.Api.Models
{
    public class ManageCheckInOutAccessDto
    {
        public string StudentName { get; set; }
        public string StudentUid { get; set; }
        public List<PickUpDto> AuthorizedPersons { get; set; }
    }
}