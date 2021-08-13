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
        public static SurveyRequest AddSurvey(int formId, int activityId, string name, string optionalText, bool allowmultiple, int accountId, int userId, SystemMessages msgs)
        {
            return CreateNewSurveyRequest(formId, activityId, accountId, name, optionalText, allowmultiple, userId, msgs);
        }

 
        public static WebUser GetCreatedBy(this SurveyRequest surveyRequest, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (surveyRequest.CreatedBy.HasValue)
                return WebUser.Load(surveyRequest.CreatedBy.Value, msgs, dc);

            return null;
        }

        public static Activity GetActivity(this SurveyRequest surveyRequest, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (surveyRequest.CreatedBy.HasValue)
                return Activity.Load(surveyRequest.ActivityId, msgs, dc);

            return null;
        }


        public static SurveyRequest CreateNewSurveyRequest(int formId, int activityId, int accountId, string name, string instrcutionsText, bool alloMultipleResponse, int userId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SurveyRequest surveyRequest = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyRequest = new SurveyRequest()
                   {
                       FormId = formId,
                       ActivityId = activityId,
                       AccountId = accountId,
                       CreatedBy = userId,
                       CreateDate = DateTime.Now,
                       LastModifiedBy = userId,
                       LastModifiedDate = DateTime.Now,
                       Name = name,
                       Description = instrcutionsText,
                       MultipleResponsesEnabled = alloMultipleResponse
                   };

                   msgs.AddRange(surveyRequest.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
           );

            return surveyRequest;
        }


        public static string ExportSurveyRequestToCSV(List<string> headers, int id, int accountId, string fileName, SystemMessages msgs)
        {
            var account = Account.Load(accountId);

            var table = new List<List<string>>();
            table.Add(AddHeadersToCSVReport(headers));

            var getSurveyFeedbacks = SurveyFeedback.LoadByActivityId(id, msgs);

            getSurveyFeedbacks.ForEach(surveyFeedback => {
                var getSurveyRequest = SurveyRequest.Load(surveyFeedback.SurveyId.Value);
                var registrant = Registrant.Load(surveyFeedback.RegistrantId.Value);

                table.Add(AddRegistrantRow(registrant.Name, surveyFeedback.SurveyFeedbackStatusTypeId.ToString(), surveyFeedback.LastModifiedDate));
            });

            var csv = CsvFile.Create(table);

            var fileLink = RegistrationService.UploadCSV(account, fileName, csv, msgs);

            return fileLink;
        }

        private static List<string> AddRegistrantRow(string name, string status, DateTime? lastModifiedDate)
        {
            var row = new List<string>();

            row.Add(name);
            row.Add(status);
            row.Add(lastModifiedDate.IsNotEmpty() ? lastModifiedDate.Value.ToString("dd/MM/yyyy"): "");

            return row;
        }

        public static List<string> AddHeadersToCSVReport(List<string> headers)
        {
            var row = new List<string>();
            foreach(var header in headers)
            {
                row.Add(header);
            }
            return row;
        }

    

        public static void DeleteCSVFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
