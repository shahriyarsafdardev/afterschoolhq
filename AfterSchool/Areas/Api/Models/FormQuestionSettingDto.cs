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
        public AudienceType audience { get; set; }
        public bool showFor { get; set; }
        public bool requiredFor { get; set; }

        public static FormQuestionSettingDto fromFormSettings(FormQuestionSettings questionSettings)
        {
            return new FormQuestionSettingDto()
            {
                QuestionId = questionSettings.QuestionId,
                audience = questionSettings.Audience,
                showFor = questionSettings.ShowFor,
                requiredFor = questionSettings.RequiredFor,
            };
        }

        public static List<FormQuestionSettingDto> fromFormSettingsList(List<FormQuestionSettings> questionSettingsList) {
            return questionSettingsList.Select(
                questionSetting=>
                fromFormSettings(questionSetting))
                .ToList();
        }
    }
}