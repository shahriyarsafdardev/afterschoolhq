using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Api.Controllers
{
    public class SurveyFormController : BaseController
    {
        [HttpGet]
        public ActionResult GetForm(Guid? formUid, Guid? surveyResponseUid)
        {
            var msgs = new SystemMessages();
            try
            {
                if (formUid.IsEmpty())
                    throw new Exception("Invalid Form Id.");

                var form = Form.LoadByUid(formUid.Value,msgs);
                
                if (form.FormTypeId != FormType.Survey || !(form.IsPublished ?? false))
                    throw new Exception("Invalid Form.");
                
                SurveyResponse surveyResponse = null;
                
                if(surveyResponseUid.IsNotEmpty())
                    surveyResponse = SurveyResponse.LoadByUid(surveyResponseUid.Value, msgs);
                
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

                dynamic response;
                if (surveyResponse.IsNotEmpty())
                {
                    var formQuestionAnswerDtoList = FormQuestionAnswer.LoadBySurveyResponseId(surveyResponse.ID.Value, msgs)
                        .Select(questionAnswer => new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId, QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText, Text = MapToAmazonFilePath(questionAnswer),
                            Description = questionAnswer.Description,
                        })
                        .ToList();
                    
                    response = new
                    {
                        FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                        SurveyResponseStatus = surveyResponse.SurveyFeedbackStatusTypeId.ToString(),
                    };
                }
                else
                {
                    response = new
                    {
                        FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, null),
                    };
                }
                
                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddUpdateAnswers(SurveyQuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();
            try
            {
                var surveyResponseUid = json.surveyResponseUid;
                var formUid = json.formUid;
                var isLastSection = json.isLastSection ?? false;
                if (!formUid.IsGuid())
                    throw new Exception("Invalid Form Uid.");

                var form = Form.LoadByUid(formUid.ToGuid(), msgs);
                SurveyResponse surveyResponse = null;
                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                var questions = form.GetAllQuestions(msgs);
                questions = questions.Where(question => question.FormQuestionGroupId == json.groupId.ToInteger()).ToList();
                var collection = HttpUtility.ParseQueryString(json.answers);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                Database.ExecuteTransaction
                (
                    (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (surveyResponseUid.IsEmpty())
                        {
                            surveyResponse = new SurveyResponse()
                            {
                                CreateDate = DateTime.Now,
                                LastModifiedDate = DateTime.Now,
                                SurveyRequestId = form.ID,
                                IsDeleted = false,
                                Uid = Guid.NewGuid(),
                                SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.InProgress,
                            };
                            amsgs.AddRange(surveyResponse.Save(adc));
                        }
                        else
                        {
                            surveyResponse = SurveyResponse.LoadByUid(surveyResponseUid.Value, amsgs, adc);
                            if (isLastSection)
                            {
                                surveyResponse.SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.Completed;
                                surveyResponse.LastModifiedDate = DateTime.UtcNow;
                                amsgs.AddRange(surveyResponse.Save(adc));
                            }
                        }

                        if (amsgs.HasNoErrors() && surveyResponse.IsNotEmpty())
                            AddCustomQuestions(surveyResponse, qvms, Request, amsgs, adc);
                    },
                    (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );

                dynamic response;

                if (msgs.HasNoErrors() && surveyResponse.IsNotEmpty())
                {
                    formQuestionAnswerDtoList = FormQuestionAnswer.LoadBySurveyResponseId(surveyResponse.ID.Value,msgs)
                        .Select(questionAnswer => new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId, QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText, Text = MapToAmazonFilePath(questionAnswer),
                            Description = questionAnswer.Description,
                        })
                        .ToList();
                }
                response = new
                {
                    SurveyResponseUid = surveyResponse?.Uid,
                    FormUid = form?.Uid,
                    SurveyResponseStatus = surveyResponse.SurveyFeedbackStatusTypeId.ToString(),
                    FormQuestionAnswers = formQuestionAnswerDtoList,
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
        
        public static void AddCustomQuestions(SurveyResponse surveyResponse, List<FormQuestionViewModel> qvms, HttpRequestBase request, SystemMessages msgs, 
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
                                    null,
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
                        {
                            surveyResponse.UpdateDescription(
                                qvm.Value,
                                qvm.Question.ID.Value,
                                msgs,
                                dc);
                        }
                    }
                    else
                    {
                        surveyResponse.AddOrUpdateAnswer(
                            null,
                            qvm.Value,
                            qvm.Question,
                            msgs,
                            dc);
                    }
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