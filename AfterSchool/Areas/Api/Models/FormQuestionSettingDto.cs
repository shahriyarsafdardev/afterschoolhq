using AfterSchoolLibrary.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionSettingDto
    {
        public AudienceType Audience { get; set; }
        public bool Show { get; set; }
        public bool Required { get; set; }

        public static FormQuestionSettingDto MapToDto(FormQuestionSettings QuestionSettings)
        {
            return new FormQuestionSettingDto()
            {
                Audience = QuestionSettings.Audience,
                Show = QuestionSettings.Show,
                Required = QuestionSettings.Required,
            };
        }

        public static List<FormQuestionSettingDto> MapToDtoList(List<FormQuestionSettings> QuestionSettingsList, int QuestionId = 0)
        {
            AudienceType[] AudienceTypes = { AudienceType.Parents, AudienceType.Students, AudienceType.Adults };
            if (QuestionSettingsList.Count != AudienceTypes.Count())
            {
                AudienceTypes.ForEach(AudienceType =>
                                {
                                    var exists = QuestionSettingsList.Any(QuestionSetting => QuestionSetting.Audience.Equals(AudienceType));
                                    if (!exists)
                                    {
                                        QuestionSettingsList.Add(new FormQuestionSettings()
                                        {
                                            Audience = AudienceType,
                                            Show = true,
                                            Required = true,
                                        });
                                    }
                                });
            }
            return QuestionSettingsList.Select(
                QuestionSetting =>
                MapToDto(QuestionSetting))
                .ToList();
        }
    }
}