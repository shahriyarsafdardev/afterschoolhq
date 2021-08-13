using AfterSchoolLibrary.Helpers;
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
        public static SurveyFeedback CreateSurveyFeedback(int surveyId, int activityId, int registrantId, int? attendanceId, SurveyFeedbackStatus surveyFeedbackStatus, int accountId, int userId, SystemMessages msgs, DatabaseConnection dc = null)
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
                      CreatedBy = userId,
                      CreateDate = DateTime.Now,
                      LastModifiedDate = DateTime.Now,
                      LastModifiedBy = userId,
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

        public static void UpdateFeedbackStatus(this SurveyFeedback surveyFeedback, SurveyFeedbackStatus surveyFeedbackStatus, DateTime lastModified, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyFeedback.SurveyFeedbackStatusTypeId = surveyFeedbackStatus;
                   surveyFeedback.LastModifiedDate = lastModified;
                   msgs.AddRange(surveyFeedback.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );
           
        }

        public static SurveyFeedback GetSurveyFeedbackBySurveyId(this SurveyRequest survey, Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (survey.IsNotEmpty())
                return SurveyFeedback.LoadByUidBySurveyId(Uid, survey.ID.Value, msgs, dc);

            return null;
        }

        public static SurveyFeedback GetSurveyFeedbackByActivityId(this Activity activity, Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.IsNotEmpty())
                return SurveyFeedback.LoadByUidByActivityId(Uid, activity.ID.Value, msgs, dc);

            return null;
        }

        public static SurveyFeedback GetSurveyFeedbackByRegistrantId(this Registrant registrant, Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrant.IsNotEmpty())
                return SurveyFeedback.LoadByUidByRegistrantId(Uid, registrant.ID.Value, msgs, dc);

            return null;
        }

        public static SurveyFeedback GetSurveyFeedbackByAttendanceId(this Attendance attendance, Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (attendance.IsNotEmpty())
                return SurveyFeedback.LoadByUidByAttendanceId(Uid, attendance.ID.Value, msgs, dc);

            return null;
        }

        public static List<SurveyFeedback> GetSurveyFeedbacksByActivity(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.IsNotEmpty())
                return SurveyFeedback.LoadByActivityId(activity.ID.Value, msgs, dc);

            return null;
        }

        public static WebUser GetCreatedBy(this SurveyFeedback surveyFeedback, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (surveyFeedback.CreatedBy.HasValue)
                return WebUser.Load(surveyFeedback.CreatedBy.Value, msgs, dc);

            return null;
        }

        public static Registrant GetRegistrantName(this SurveyFeedback surveyFeedback, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (surveyFeedback.RegistrantId.HasValue)
                return Registrant.Load(surveyFeedback.RegistrantId.Value, msgs, dc);

            return null;
        }
    }
}
