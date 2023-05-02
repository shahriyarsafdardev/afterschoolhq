using AfterSchoolLibrary.Models;
using System.Collections.Generic;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionGroupDto
    {
        public int Id { get; set; }
        public int? FormId { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
        public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }

        public static FormQuestionGroupDto GetByQuestionGroup(FormQuestionGroup questionGroup)
        {
            return new FormQuestionGroupDto()
            {
                FormId = questionGroup.FormId,
                Name = questionGroup.Name,
                DisplayOrder = questionGroup.DisplayOrder,
                QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId
            };
        }

        public static List<FormQuestionGroupDto> GetByQuestionGroupList(List<FormQuestionGroup> questionGroupList)
        {
            var questionGroupDtoList = new List<FormQuestionGroupDto>();
            foreach (var questionGroup in questionGroupList)
                questionGroupDtoList.Add(GetByQuestionGroup(questionGroup));
            return questionGroupDtoList;

        }


    }
}