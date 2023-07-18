namespace AfterSchool.Areas.Api.Models
{
    public class PickUpDto
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public QuestionDataType? PersonPhoneDataType { get; set; }
        public bool HasAccess { get; set; }
    }
}