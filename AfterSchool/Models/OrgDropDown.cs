using System;

namespace AfterSchool.Models
{
    public class OrgDropDown
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Slug { get; set; }
        public bool IsClaimed { get; set; }
    }
}