using AfterSchoolLibrary.Models;
using Microsoft.Ajax.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace AfterSchool.Areas.Api.Models
{
    public class FormQuestionSettingDto
    {
        public AudienceType Audience { get; set; }
        public bool Show { get; set; }
        public bool Required { get; set; }

        public static FormQuestionSettingDto MapToDto(FormQuestionSettings questionSettings)
        {
            return new FormQuestionSettingDto()
            {
                Audience = questionSettings.Audience,
                Show = questionSettings.Show,
                Required = questionSettings.Required,
            };
        }

        public static List<FormQuestionSettingDto> MapToDtoList(List<FormQuestionSettings> questionSettingsList, int questionId = 0, FormType currFormType = FormType.NotSet)
        {
            var audienceTypes = currFormType.Equals(FormType.VolunteerForm) ? new List<AudienceType>(){AudienceType.Volunteer} :
                new List<AudienceType>(){ AudienceType.Parents, AudienceType.Students, AudienceType.Adults };
            if (questionSettingsList.Count != audienceTypes.Count())
            {
                audienceTypes.ForEach(audienceType =>
                                {
                                    var exists = questionSettingsList.Any(questionSetting => questionSetting.Audience.Equals(audienceType));
                                    if (!exists)
                                    {
                                        questionSettingsList.Add(new FormQuestionSettings()
                                        {
                                            Audience = audienceType,
                                            Show = true,
                                            Required = true,
                                        });
                                    }
                                });
            }
            return questionSettingsList.Select(
                questionSetting =>
                MapToDto(questionSetting))
                .ToList();
        }
    }
}