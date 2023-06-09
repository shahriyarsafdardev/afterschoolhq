using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class SurveyResponseService
    {
        public static SurveyResponse CreateSurveyFeedback(int surveyId, int registrantId, SurveyFeedbackStatus surveyFeedbackStatus, int userId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SurveyResponse surveyResponse = null;
            var pinGenerator = new Random();
            var pinCode = pinGenerator.Next(0, 1000000).ToString("D6");
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
                       LastAttendedFormQuestionAnswerUid = null,
                       Pin = pinCode,
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

        public static void NotifyStudents(int surveyResponseId, int registrantId, int surveyRequestId, string code, int codeId, SystemMessages msgs)
        {
            EmailHelper.SendSurveyEmail(surveyResponseId: surveyResponseId,
                registrantId: registrantId, 
                surveyRequestId: surveyRequestId, 
                code: code,
                codeId: codeId,
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

        public static List<FormQuestionAnswer> GetAnswers(this SurveyResponse surveyResponse, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadBySurveyResponseId(surveyResponse.ID.Value, msgs, dc)
                .OrderBy(x => x.FormQuestionDisplayOrder)
                .ToList();
        }

        public static string GetRegistrantName(this SurveyResponse surveyResponse)
        {
            return BasePerson.BuildDisplayName(surveyResponse.RegistrantFirstName, surveyResponse.RegistrantLastName);
        }

        private static string GetFormattedAnswerForCheckboxList(string s)
        {
            return $"[{s}]";
        }

        public static void AddOrUpdateAnswer(this SurveyResponse surveyResponse, Stream file, string filename, FormQuestion question, string description,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
            {
                var amazonFilePath = UploadFile(file, filename, question, msgs, dc);

                if (msgs.HasNoErrors())
                    AddOrUpdateAnswer(
                        surveyResponse.ID.Value,
                        amazonFilePath,
                        question.ID.Value,
                        question.QuestionDataTypeId,
                        question.Text,
                        msgs,
                        dc,
                        description);
            }
        }

        private static string UploadFile(Stream file, string filename, FormQuestion question, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ah = Utilities.GetNewAmazonHelper();

            var regEx = new Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx,
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            var form = question.GetForm(msgs, dc);
            var formFolder = $"{form.Name.Replace(regEx, "")}-{form.ID}";
            var amazonFilePath = $"{formFolder}/{filename}".ToLower();

            msgs.AddRange(ah.AddFile(file, amazonFilePath));

            return amazonFilePath;
        }

        public static void AddOrUpdateAnswer(this SurveyResponse surveyResponse, string answerText, FormQuestion question,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (question.IsNotEmpty())
            {
                if (question.QuestionTypeId == QuestionType.CheckboxList && answerText.IsNotEmpty())
                    answerText = GetFormattedAnswerForCheckboxList(answerText);

                AddOrUpdateAnswer(
                    surveyResponse.ID.Value,
                    answerText,
                    question.ID.Value,
                    question.QuestionDataTypeId,
                    question.Text,
                    msgs,
                    dc);
            }
        }

        private static void AddOrUpdateAnswer(int surveyResponseId, string answerText, int questionId, QuestionDataType questionDataType,
            string questionText, SystemMessages msgs, DatabaseConnection dc = null, string description = "")
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            FormQuestionAnswer answer = null;

            answer = FormQuestionAnswer.LoadBySurveyResponseIdByQuestionId(surveyResponseId, questionId, msgs, dc);

            if (answer.IsEmpty())
                answer = new FormQuestionAnswer
                {
                    SurveyResponseId = surveyResponseId,
                    FormQuestionId = questionId,
                    QuestionDataTypeId = questionDataType,
                    QuestionText = questionText,
                    CreateDate = DateTime.UtcNow,
                };
            if(description.IsNotEmpty())
                answer.Description = description;
            answer.Text = answerText;
            answer.LastModifiedDate = DateTime.UtcNow;

            msgs.AddRange(answer.Save(dc));
        }
        public static void UpdateDescription(this SurveyResponse surveyResponse, string description, int questionId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            FormQuestionAnswer answer = null;

            answer = FormQuestionAnswer.LoadBySurveyResponseIdByQuestionId(surveyResponse.ID.Value, questionId, msgs, dc);

            if (answer.IsNotEmpty())
            {
                answer.Description = description;
                answer.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(answer.Save(dc));
            }

        }
    }
}
