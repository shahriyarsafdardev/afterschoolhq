using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionSettingDto
    {
        public int QuestionId { get; set; }
        public AudienceType Audience { get; set; }
        public bool Show { get; set; }
        public bool Required { get; set; }

        public static FormQuestionSettingDto fromFormSettings(FormQuestionSettings QuestionSettings)
        {
            return new FormQuestionSettingDto()
            {
                QuestionId = QuestionSettings.QuestionId,
                Audience = QuestionSettings.Audience,
                Show = QuestionSettings.Show,
                Required = QuestionSettings.Required,
            };
        }

        public static List<FormQuestionSettingDto> fromFormSettingsList(List<FormQuestionSettings> QuestionSettingsList) {
            return QuestionSettingsList.Select(
                QuestionSetting=>
                fromFormSettings(QuestionSetting))
                .ToList();
        }
    }
}