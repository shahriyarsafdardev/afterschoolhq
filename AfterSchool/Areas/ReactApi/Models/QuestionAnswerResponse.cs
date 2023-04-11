namespace AfterSchool.Areas.ReactApi.Models
{
    public class QuestionAnswerResponse
    {
        public string groupid { get; set; }
        public string esignature { get; set; }
        public string answers { get; set; }
        public int? schoolLocationID { get; set; }
        public int? newChild { get; set; }
        public string registrantId { get; set; }
        public string registrantUid { get; set; }
        public string accountName { get; set; }
        public string activityName { get; set; }
    }
}