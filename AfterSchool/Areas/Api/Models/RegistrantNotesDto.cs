using System.Collections.Generic;

namespace AfterSchool.Areas.Api.Models
{
    public class RegistrantNotesDto
    {
        public string RegistrantName { get; set; }
        public List<NotesDto> Notes { get; set; }
    }
}