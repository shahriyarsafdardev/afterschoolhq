using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SurveyFeedbackService
    {
        public static SurveyFeedback CreateSurveyFeedback(int surveyId, int activityId, int registrantId, int attendanceId, SurveyFeedbackStatus surveyFeedbackStatus, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SurveyFeedback surveyFeedback = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyFeedback = new SurveyFeedback()
                   {
                      SurveyId = surveyId,
                      ActivityId = activityId,
                      RegistrantId = registrantId,
                      AttendanceId = attendanceId,
                      DueDate = null,
                      SurveyFeedbackStatusTypeId = surveyFeedbackStatus
                   };

                   msgs.AddRange(surveyFeedback.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );

            return surveyFeedback;
        }

        public static void UpdateFeedbackStatus(this SurveyFeedback surveyFeedback, SurveyFeedbackStatus surveyFeedbackStatus, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyFeedback.SurveyFeedbackStatusTypeId = surveyFeedbackStatus;
                   msgs.AddRange(surveyFeedback.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );
           
        }
    }
}
