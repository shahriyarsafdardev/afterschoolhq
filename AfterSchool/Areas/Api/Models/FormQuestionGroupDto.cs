using AfterSchoolLibrary.Models;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionGroupDto
    {
        public int? FormId { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
        public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }

        public static FormQuestionGroupDto GetByQuestionGroup(FormQuestionGroup QuestionGroup)
        {
            return new FormQuestionGroupDto()
            {
                FormId = QuestionGroup.FormId,
                Name = QuestionGroup.Name,
                DisplayOrder = QuestionGroup.DisplayOrder,
                QuestionGroupDataTypeId = QuestionGroup.QuestionGroupDataTypeId
            };
        }

        public static List<FormQuestionGroupDto> MapToDtoList(List<FormQuestionGroup> QuestionGroupList)
        {
            return QuestionGroupList
                .Select(QuestionGroup => 
                GetByQuestionGroup(QuestionGroup))
                .ToList();
        }


    }
}