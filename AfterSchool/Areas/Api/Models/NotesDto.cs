using System;

namespace AfterSchool.Areas.Api.Models
{
    public class NotesDto
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string CreatedByName { get; set; }
        public string PostDate { get; set; }
        public bool IsEditable { get; set; }
    }
}