namespace AfterSchool.Areas.ReactApi.Models
{
    public class FormQuestionDto
    {
        public int ID { get; set; }
        public int? FormQuestionGroupId { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string GroupTitle { get; set; }
        public string Options { get; set; }
        public bool IsRequired { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public QuestionType QuestionTypeId { get; set; }
        public QuestionDataType QuestionDataTypeId { get; set; }
        public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get; set; }
    }
}