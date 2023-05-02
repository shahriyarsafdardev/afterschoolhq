using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionGroupDto
    {
        public int? FormId { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
        public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }

        public static FormQuestionGroupDto byQuestionGroup(FormQuestionGroup questionGroup)
        {
            return new FormQuestionGroupDto()
            {
                FormId = questionGroup.FormId,
                Name = questionGroup.Name,
                DisplayOrder = questionGroup.DisplayOrder,
                QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId
            };
        }
    }
}