using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SurveyResponseRegistrantService
    {
        public static SurveyResponseRegistrant CreateSurveyResponseRegistrant(int surveyResponseId, int masterRegistrantId, int userId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SurveyResponseRegistrant surveyResponseRegistrant = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyResponseRegistrant = new SurveyResponseRegistrant()
                   {
                       SurveyResponseId = surveyResponseId,
                       MasterRegistrantId = masterRegistrantId,
                       CreatedBy = userId,
                       CreateDate = DateTime.Now,
                       LastModifiedDate = DateTime.Now,
                       LastModifiedBy = userId,
                   };

                   msgs.AddRange(surveyResponseRegistrant.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );

            return surveyResponseRegistrant;
        }
    }
}
