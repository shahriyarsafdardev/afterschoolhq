using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SurveyResponseService
    {
        public static SurveyResponse CreateSurveyFeedback(int surveyId, int registrantId, SurveyFeedbackStatus surveyFeedbackStatus, int userId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SurveyResponse surveyResponse = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyResponse = new SurveyResponse()
                   {
                       SurveyRequestId = surveyId,
                       RegistrantId = registrantId,
                       CreatedBy = userId,
                       CreateDate = DateTime.Now,
                       LastModifiedDate = DateTime.Now,
                       LastModifiedBy = userId,
                       Uid = Guid.NewGuid(),
                       SurveyFeedbackStatusTypeId = surveyFeedbackStatus,
                       LastAttendedFormQuestionAnswerUid = null
                   };

                   msgs.AddRange(surveyResponse.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );

            return surveyResponse;
        }

        public static void UpdateFeedbackStatus(this SurveyResponse surveyResponse, SurveyFeedbackStatus surveyFeedbackStatus, int userId, DateTime lastModified, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyResponse.SurveyFeedbackStatusTypeId = surveyFeedbackStatus;
                   surveyResponse.LastModifiedBy = userId;
                   surveyResponse.LastModifiedDate = lastModified;
                   msgs.AddRange(surveyResponse.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );

        }

     

        public static void NotifyStudents(int surveyResponseId, int registrantId, int surveyRequestId, string code, SystemMessages msgs)
        {
            EmailHelper.SendSurveyEmail(surveyResponseId: surveyResponseId,
                registrantId: registrantId, 
                surveyRequestId: surveyRequestId, 
                code: code, 
                msgs: msgs);
        }

        public static List<SurveyResponse> GetSurveyFeedbackBySurveyId(this SurveyResponse survey, Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (survey.IsNotEmpty())
                return SurveyResponse.LoadBySurveyRequestId(survey.ID.Value, msgs, dc);

            return null;
        }

        public static WebUser GetCreatedBy(this SurveyResponse surveyResponse, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (surveyResponse.CreatedBy.HasValue)
                return WebUser.Load(surveyResponse.CreatedBy.Value, msgs, dc);

            return null;
        }

        public static Registrant GetRegistrantName(this SurveyResponse surveyResponse, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (surveyResponse.RegistrantId.HasValue)
                return Registrant.Load(surveyResponse.RegistrantId.Value, msgs, dc);

            return null;
        }


    }
}
