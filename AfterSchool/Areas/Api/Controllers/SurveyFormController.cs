using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
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

                var form = Form.LoadByUid(formUid.Value);
                
                if (form.FormTypeId != FormType.Survey) //|| !(form.IsPublished ?? false) 
                    throw new Exception("Invalid Form.");
                
                SurveyResponse surveyResponse = null;
                
                if(surveyResponseUid.IsNotEmpty())
                    surveyResponse = SurveyResponse.LoadByUid(surveyResponseUid.Value);
                
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
                        FormId = questionGroup.FormId,
                        Name = questionGroup.Name,
                        DisplayOrder = questionGroup.DisplayOrder,
                        QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                    })
                    .ToList();

                dynamic response;
                if (surveyResponse.IsNotEmpty())
                {
                    var formQuestionAnswerDtoList = FormQuestionAnswer.LoadBySurveyResponseId(surveyResponse.ID.Value)
                        .Select(questionAnswer => new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId, QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText, Text = MapToAmazonFilePath(questionAnswer),
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
        
        [HttpGet]
        public ActionResult Index(Guid? Uid)
        {
            if (Uid.IsEmpty()) return Redirect("~/login");
            var form = Form.LoadByUid(Uid.Value);
            return Redirect(form.FormTypeId == FormType.Survey ? $"~/ReactApp/survey/{form.ID}" : "~/login");
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
                if(formUid.IsEmpty())
                    throw new Exception("Invalid Form Uid.");
                
                var form = Form.LoadByUid(formUid.Value);
                SurveyResponse surveyResponse = null;
                var questions = form.GetAllQuestions(msgs);
                var collection = HttpUtility.ParseQueryString(json.answers);
                
                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
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
                            msgs.AddRange(surveyResponse.Save(adc));
                        }
                        else
                        {
                            surveyResponse = SurveyResponse.LoadByUid(surveyResponseUid.Value);
                            if (isLastSection)
                            {
                                surveyResponse.SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.Completed;
                                surveyResponse.LastModifiedDate = DateTime.Now;
                                msgs.AddRange(surveyResponse.Save(adc));
                            }

                        }
                        if(msgs.HasNoErrors() && surveyResponse.IsNotEmpty())
                            AddCustomQuestions(surveyResponse, qvms, Request, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
                
                return Json(
                    new ApiResponse(msgs,new {SurveyResponseUid = surveyResponse?.Uid, FormUid = form?.Uid, 
                        SurveyResponseStatus = surveyResponse.SurveyFeedbackStatusTypeId.ToString()}),
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
                            if (currentFileSizeInBytes <= 5242880)
                                surveyResponse.AddOrUpdateAnswer(
                                    request.Files[qvm.Name].InputStream,
                                    request.Files[qvm.Name].FileName,
                                    qvm.Question,
                                    msgs,
                                    dc);
                            else
                                msgs.Add("Upload a file less than or equal to 5 megabytes", MessageType.UserError);
                        }
                    }
                    else
                    {
                        surveyResponse.AddOrUpdateAnswer(
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
                question.FormQuestionQuestionTypeId != QuestionType.Video) 
                return question.Text;
            var amazonHelper = Utilities.GetNewAmazonHelper();
            return amazonHelper.GetFileLink(question.Text);
        }

    }
}