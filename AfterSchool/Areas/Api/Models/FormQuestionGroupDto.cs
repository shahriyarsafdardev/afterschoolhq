using AfterSchoolLibrary.Models;
using RestSharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionGroupDto
    {
        public int Id { get; set; }
        public int? FormId { get; set; }
        public int? ID { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsDeleted { get; set; }
        public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }

        public bool IsNull()
        {
            return 
                FormId == null &&
                Name == null &&
                DisplayOrder == null &&
                IsDeleted == null &&
                QuestionGroupDataTypeId == QuestionGroupDataType.NotSet;
        }

        public bool IsValid()
        {
            return Name.HasValue() || !Name.Equals("") || DisplayOrder.HasValue || IsDeleted.HasValue ;
        }

        public bool IsNotNull()
        {
            return !IsNull();
        }

        public static FormQuestionGroupDto GetByQuestionGroup(FormQuestionGroup QuestionGroup)
        {
            return new FormQuestionGroupDto()
            {
                ID = QuestionGroup.ID,
                FormId = QuestionGroup.FormId,
                Name = QuestionGroup.Name,
                DisplayOrder = QuestionGroup.DisplayOrder,
                QuestionGroupDataTypeId = QuestionGroup.QuestionGroupDataTypeId,
                IsDeleted = QuestionGroup.IsDeleted,
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