using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Api.Controllers
{
    public class SurveyRequestController : Controller
    {
        [HttpGet]
        public ActionResult GetForm(string uid, string pin)
        {
            var msgs = new SystemMessages();
            try
            {
                if (!uid.IsGuid())
                    throw new Exception("Invalid Id.");

                var surveyRequest = SurveyRequest.LoadByUid(uid.ToGuid(), msgs);

                if (surveyRequest.IsEmpty())
                    throw new Exception("Invalid Request.");

                var surveyResponse = SurveyResponse.LoadByPin(pin, msgs);

                if (surveyResponse.IsEmpty())
                    throw new Exception("Invalid Pin.");

                var currentRegistrant = Registrant.Load(surveyResponse.RegistrantId.Value, msgs);
                if(currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant.");

                var form = Form.Load(surveyRequest.FormId, msgs);
                if (form.IsEmpty())
                    throw new Exception("Invalid Form.");

                if (form.FormTypeId != FormType.Survey || !(form.IsPublished ?? false))
                    throw new Exception("Invalid Form.");

                var questions = form.GetAllQuestions(msgs);
                var questionGroups = form.GetQuestionGroups(msgs);

                var formQuestionDtoList = questions.Select(question => new FormQuestionDto
                    {
                        ID = question.ID ?? 0,
                        FormQuestionGroupId = question.FormQuestionGroupId,
                        Text = question.Text,
                        Description = question.Description,
                        GroupTitle = question.FormQuestionGroupName,
                        Options = question.Options,
                        IsRequired = question.IsRequired,
                        DisplayOrder = question.DisplayOrder,
                        IsActive = question.IsActive,
                        IsDeleted = question.IsDeleted,
                        QuestionTypeId = question.QuestionTypeId,
                        QuestionDataTypeId = question.QuestionDataTypeId,
                        FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId,
                    })
                    .ToList();

                var formQuestionGroupDtoList = questionGroups.Select(questionGroup => new FormQuestionGroupDto
                    {
                        Id = questionGroup.ID ?? 0,
                        FormId = questionGroup.FormId,
                        Name = questionGroup.Name,
                        DisplayOrder = questionGroup.DisplayOrder,
                        QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                    })
                    .ToList();

                var formQuestionAnswerDtoList = FormQuestionAnswer.LoadBySurveyResponseId(surveyResponse.ID.Value, msgs)
                       .Select(questionAnswer => new FormQuestionAnswerDto()
                       {
                           FormQuestionId = questionAnswer.FormQuestionId, 
                           QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                           QuestionText = questionAnswer.QuestionText, 
                           Text = questionAnswer.Text,
                           Description = questionAnswer.Description,
                       })
                       .ToList();

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    SurveyResponseStatus = surveyResponse.SurveyFeedbackStatusTypeId.ToString(),
                    Pin = pin,
                    SurveyRequestUid = uid,
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddUpdateAnswers(SurveyRequestQuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();
            try
            {
                var isLastSection = json.isLastSection ?? false;
                var pin = json.pin;
                var surveyRequestUid = json.surveyRequestUid;

                if (!surveyRequestUid.IsGuid())
                    throw new Exception("Invalid Id.");

                var surveyRequest = SurveyRequest.LoadByUid(surveyRequestUid.ToGuid(), msgs);

                if (surveyRequest.IsEmpty())
                    throw new Exception("Invalid Request.");

                var surveyResponse = SurveyResponse.LoadByPin(pin, msgs);

                if (surveyResponse.IsEmpty())
                    throw new Exception("Invalid Pin.");

                var currentRegistrant = Registrant.Load(surveyResponse.RegistrantId.Value, msgs);

                if(currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant.");

                var registration = currentRegistrant.GetRegistrations(msgs)?.LastOrDefault();

                var form = Form.Load(surveyRequest.FormId, msgs);

                if (form.IsEmpty())
                    throw new Exception("Invalid Form.");

                var questions = form.GetAllQuestions(msgs);
                questions = questions.Where(question => question.FormQuestionGroupId == json.groupId.ToInteger()).ToList();
                var collection = HttpUtility.ParseQueryString(json.answers);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();

                Database.ExecuteTransaction
                (
                    (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (isLastSection)
                        {
                            surveyResponse.SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.Completed;
                            surveyResponse.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(surveyResponse.Save(adc));
                        }
                        else if (surveyResponse.SurveyFeedbackStatusTypeId == SurveyFeedbackStatus.New)
                        {
                            surveyResponse.SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.InProgress;
                            surveyResponse.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(surveyResponse.Save(adc));
                        }
                        if (amsgs.HasNoErrors())
                            AddCustomQuestions(surveyResponse, currentRegistrant, qvms, Request, amsgs, adc);
                    },
                    (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
                
                var formQuestionAnswerDtoList = FormQuestionAnswer.LoadBySurveyResponseId(surveyResponse.ID.Value, msgs)
                    .Select(questionAnswer => new FormQuestionAnswerDto()
                    {
                        FormQuestionId = questionAnswer.FormQuestionId, 
                        QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                        QuestionText = questionAnswer.QuestionText, 
                        Text = questionAnswer.Text,
                        Description = questionAnswer.Description,
                    })
                    .ToList();

                dynamic response = new
                {
                    SurveyResponseStatus = surveyResponse.SurveyFeedbackStatusTypeId.ToString(),
                    FormQuestionAnswers = formQuestionAnswerDtoList,
                    Pin = pin,
                    SurveyRequestUid = surveyRequestUid,
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        public static void AddCustomQuestions(SurveyResponse surveyResponse, Registrant registrant, List<FormQuestionViewModel> qvms, HttpRequestBase request, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.NotSet)
                .ToList()
                .ForEach(qvm =>
                {
                    if (qvm.Question.QuestionTypeId == QuestionType.FileUpload || qvm.Question.QuestionTypeId == QuestionType.Photo ||
                        qvm.Question.QuestionTypeId == QuestionType.Audio || qvm.Question.QuestionTypeId == QuestionType.Video)
                    {
                        if (request.Files[qvm.Name].IsNotEmpty() && request.Files[qvm.Name].ContentLength > 0)
                        {
                            var currentFileSizeInBytes = request.Files[qvm.Name].ContentLength;
                            //5 MB = 5242880 Bytes
                            //File uploaded here should be less than of 5MB
                            const int maxFileSize = Utilities.MaxFileSize;
                            if (currentFileSizeInBytes <= maxFileSize)
                                surveyResponse.AddOrUpdateAnswer(
                                    registrant,
                                    request.Files[qvm.Name].InputStream,
                                    request.Files[qvm.Name].FileName,
                                    qvm.Question,
                                    qvm.Value,
                                    msgs,
                                    dc);
                            else
                                msgs.Add("Upload a file less than or equal to 5 megabytes", MessageType.UserError);
                        }
                        else
                            surveyResponse.UpdateDescription(
                                    qvm.Value,
                                    qvm.Question.ID.Value,
                                    msgs,
                                    dc);
                    }
                    else
                        surveyResponse.AddOrUpdateAnswer(
                                registrant,
                                qvm.Value,
                                qvm.Question,
                                msgs,
                                dc);
                });
        }

        private static string MapToAmazonFilePath(FormQuestionAnswer question)
        {
            if (question.FormQuestionQuestionTypeId != QuestionType.Photo &&
                question.FormQuestionQuestionTypeId != QuestionType.Audio &&
                question.FormQuestionQuestionTypeId != QuestionType.Video &&
                question.FormQuestionQuestionTypeId != QuestionType.FileUpload)
                return question.Text;
            var amazonHelper = Utilities.GetNewAmazonHelper();
            return amazonHelper.GetFileLink(question.Text,DateTime.UtcNow.AddMinutes(5), false);
        }
    }
}