using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SurveyRequestService
    {
        public static SurveyRequest AddSurvey(int formId, int activityId, string name, string dueDate, string optionalText, bool allowmultiple, int accountId, int userId, SystemMessages msgs)
        {
            DateTime? duedate = null;
            if (dueDate.IsNotEmpty())
                duedate = dueDate.ToDateTime();

            return CreateNewSurveyRequest(formId, activityId, accountId, name, duedate, optionalText, allowmultiple, userId, msgs);
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


        public static SurveyRequest CreateNewSurveyRequest(int formId, int activityId, int accountId, string name, DateTime? dueDate, string instrcutionsText, bool alloMultipleResponse, int userId, SystemMessages msgs, DatabaseConnection dc = null)
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
                       DueDate = dueDate ?? dueDate,
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

            var survey = SurveyRequest.Load(id, msgs);
            var surveyResponses = SurveyResponse.LoadBySurveyRequestId(id, msgs).Where(a => a.SurveyFeedbackStatusTypeId == SurveyFeedbackStatus.Completed).ToList();
            var formQuestions = FormQuestion.LoadByFormIdByIsActive(survey.FormId, true, msgs);

            var table = new List<List<string>>();
            table.Add(AddHeaders(formQuestions));

            surveyResponses.ForEach(surveyResponse => {
                var getSurveyRequest = SurveyRequest.Load(surveyResponse.SurveyRequestId.Value);
                var registrant = Registrant.Load(surveyResponse.RegistrantId.Value);
                var formQuestionAnswers = FormQuestionAnswer.LoadBySurveyResponseId(surveyResponse.ID.Value, msgs);

                table.Add(AddQuestionAnswerRow(survey.CreateDate, surveyResponse.LastModifiedDate, registrant.FirstName, registrant.LastName, formQuestionAnswers));
            });

            var csv = CsvFile.Create(table);

            var fileLink = RegistrationService.UploadCSV(account, fileName, csv, msgs);

            return fileLink;
        }

        private static List<string> AddHeaders(List<FormQuestion> formQuestions)
        {
            var row = new List<string>();
            row.Add("Create Date");
            row.Add("Submit Date");
            row.Add("First Name");
            row.Add("Last Name");
            formQuestions.OrderBy(a=>a.DisplayOrder).ToList().ForEach(formQuestion =>
            {
                if(formQuestion.QuestionTypeId != QuestionType.TextBlock)
                    row.Add(formQuestion.Text);
            });

            return row;
        }

        private static List<string> AddQuestionAnswerRow(DateTime? createDate, DateTime? lastModifiedDate, string firstName, string lastName, List<FormQuestionAnswer> formQuestionAnswers)
        {
            var row = new List<string>();

            row.Add(createDate.IsNotEmpty() ? createDate.Value.ToString("MM/dd/yyyy") : "");
            row.Add(lastModifiedDate.IsNotEmpty() ? lastModifiedDate.Value.ToString("MM/dd/yyyy") : "");
            row.Add(firstName);
            row.Add(lastName);

            formQuestionAnswers.OrderBy(a => a.FormQuestionDisplayOrder).ToList().ForEach(formQuestionAnswer =>
            {
                if (formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.TextArea)
                {
                    dynamic json = JsonConvert.DeserializeObject(formQuestionAnswer.Text);
                    row.Add(Convert.ToString(json.blocks[0].text));
                }
                else if (formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.Checkbox)
                {
                    row.Add(formQuestionAnswer.Text == "1" ? "True" : "False");
                }
                else if (formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.DropDownList || formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.RadioList || formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.Rating || formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.Emoji)
                {
                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(formQuestionAnswer.Text);
                    if (formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.Rating || formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.Emoji)
                    {
                        row.Add(Convert.ToString(json[0].key));
                    }
                    else
                    {
                        row.Add(Convert.ToString(json[0].value));
                    }
                }
                else if (formQuestionAnswer.FormQuestionQuestionTypeId == QuestionType.CheckboxList)
                {
                    var str = "";
                    dynamic json =JsonConvert.DeserializeObject(formQuestionAnswer.Text);
                    foreach (var item in json)
                    {
                        if(str.IsNotEmpty())
                            str = str +", " + item.value;
                        else
                            str = item.value;
                    }
                    row.Add(Convert.ToString(str));
                }
                else if (formQuestionAnswer.FormQuestionQuestionTypeId != QuestionType.TextBlock)
                {
                    row.Add(formQuestionAnswer.Text);
                }
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
