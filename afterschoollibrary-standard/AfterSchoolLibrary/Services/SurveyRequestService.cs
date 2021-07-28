using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SurveyRequestService
    {
        public static SurveyRequest CreateNewSurveyRequest(int formId, int activityId, int accountId, string name, string instrcutionsText, bool alloMultipleResponse, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var survey = new SurveyRequest()
            {
                FormId = formId,
                ActivityId = activityId,
                AccountId = accountId,
                Name = name,
                InstructionsText = instrcutionsText,
                MultipleResponsesEnabled = alloMultipleResponse
            };

            msgs.AddRange(survey.Save(dc));

            return survey;
        }
    }
}
