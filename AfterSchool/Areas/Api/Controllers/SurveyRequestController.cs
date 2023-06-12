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
                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                foreach (var question in questions)
                {
                    var questionAnswer = FormQuestionAnswer.LoadByFormQuestionIdByRegistrantId(question.ID.Value,currentRegistrant.ID.Value, msgs);
                    if (questionAnswer.IsNotEmpty())
                        formQuestionAnswerDtoList.Add(new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId,
                            QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText,
                            Text = MapToAmazonFilePath(questionAnswer),
                            Description = questionAnswer.Description,
                        });
                }

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
                        if (amsgs.HasNoErrors())
                            RegistrationCommon.AddCustomQuestions(
                            qvms,
                            currentRegistrant,
                            registration,
                            null, 
                            Request,
                            amsgs, adc);
                    },
                    (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );

                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                foreach (var question in questions)
                {
                    var questionAnswer = FormQuestionAnswer.LoadByFormQuestionIdByRegistrantId(question.ID.Value,currentRegistrant.ID.Value, msgs);
                    if (questionAnswer.IsNotEmpty())
                        formQuestionAnswerDtoList.Add(new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId,
                            QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText,
                            Text = MapToAmazonFilePath(questionAnswer),
                            Description = questionAnswer.Description,
                        });
                }

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

        private static string MapToAmazonFilePath(FormQuestionAnswer question)
        {
            if (question.FormQuestionQuestionTypeId != QuestionType.Photo &&
                question.FormQuestionQuestionTypeId != QuestionType.Audio &&
                question.FormQuestionQuestionTypeId != QuestionType.Video &&
                question.FormQuestionQuestionTypeId != QuestionType.FileUpload)
                return question.Text;
            var amazonHelper = Utilities.GetNewAmazonHelper();
            return amazonHelper.GetFileLink(question.Text,DateTime.UtcNow.AddMinutes(5), true);
        }
    }
}