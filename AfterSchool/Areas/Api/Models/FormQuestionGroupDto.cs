namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionGroupDto
    {
        public int? FormId { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
        public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }
    }
}