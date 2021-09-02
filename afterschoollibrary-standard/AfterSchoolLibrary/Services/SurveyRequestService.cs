using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
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
                       Uid = Guid.NewGuid(),
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


        public static string ExportSurveyRequestToCSV(int id, int accountId, string fileName, SystemMessages msgs)
        {
            var account = Account.Load(accountId);

            var getSurvey = SurveyRequest.Load(id, msgs);
            var getSurveyResponses = SurveyResponse.LoadBySurveyRequestId(id, msgs).Where(a => a.SurveyFeedbackStatusTypeId == SurveyFeedbackStatus.Completed).ToList();
            var getFormQuestions = FormQuestion.Load(getSurvey.FormId, msgs).AsList();

            var table = new List<List<string>>();

            var isFirstRow = true;

            getSurveyResponses.ForEach(getSurveyResponse => {
                var getSurveyRequest = SurveyRequest.Load(getSurveyResponse.SurveyRequestId.Value);
                var registrant = Registrant.Load(getSurveyResponse.RegistrantId.Value);
                var getFormQuestionAnswers = FormQuestionAnswer.LoadBySurveyResponseId(getSurveyResponse.ID.Value, msgs);
                if (isFirstRow)
                {
                    table.Add(AddHeaders(getFormQuestionAnswers));
                    isFirstRow = false;
                }
                table.Add(AddQuestionAnswerRow(getSurvey.CreateDate, getSurveyResponse.LastModifiedDate, registrant.FirstName, registrant.LastName, getFormQuestionAnswers));
            });

            var csv = CsvFile.Create(table);

            var fileLink = RegistrationService.UploadCSV(account, fileName, csv, msgs);

            return fileLink;
        }

        private static List<string> AddHeaders(List<FormQuestionAnswer> getFormQuestionAnswers)
        {
            var row = new List<string>();
            row.Add("Create Date");
            row.Add("Submit Date");
            row.Add("First Name");
            row.Add("Last Name");
            getFormQuestionAnswers.ForEach(getFormQuestionAnswer =>
            {
                row.Add(getFormQuestionAnswer.QuestionText);
            });

            return row;
        }

        private static List<string> AddQuestionAnswerRow(DateTime? createDate, DateTime? lastModifiedDate, string firstName, string lastName, List<FormQuestionAnswer> formQuestionAnswers)
        {
            var row = new List<string>();

            row.Add(createDate.IsNotEmpty() ? createDate.Value.ToString("dd/MM/yyyy") : "");
            row.Add(lastModifiedDate.IsNotEmpty() ? lastModifiedDate.Value.ToString("dd/MM/yyyy") : "");
            row.Add(firstName);
            row.Add(lastName);

            formQuestionAnswers.ForEach(formQuestionAnswer =>
            {
                row.Add(formQuestionAnswer.Text);
            });

            return row;
        }

        private static List<string> AddRegistrantRow(string name, string status, DateTime? lastModifiedDate)
        {
            var row = new List<string>();

            row.Add(name);
            row.Add(status);
            row.Add(lastModifiedDate.IsNotEmpty() ? lastModifiedDate.Value.ToString("dd/MM/yyyy"): "");

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
