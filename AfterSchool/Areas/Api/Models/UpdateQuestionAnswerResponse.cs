namespace AfterSchool.Areas.Api.Models
{
    public class UpdateQuestionAnswerResponse
    {
        public string groupid { get; set; }
        public string answers { get; set; }
        public int? schoolLocationID { get; set; }
        public string registrationUid { get; set; }
    }
}