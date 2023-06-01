using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public ActionResult GetForm(int formId)
        {
            var msgs = new SystemMessages();
            try
            {
                if (formId < 1)
                    throw new Exception("Invalid Form Id.");

                var form = Form.Load(formId);
                
                if (form.FormTypeId != FormType.Survey) //|| !(form.IsPublished ?? false) 
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
                        FormId = questionGroup.FormId,
                        Name = questionGroup.Name,
                        DisplayOrder = questionGroup.DisplayOrder,
                        QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                    })
                    .ToList();

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList),
                };

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
                var surveyResponseId = json.surveyResponseId;
                var form = Form.Load(json.formId);
                var questions = form.GetAllQuestions(msgs);
                var collection = HttpUtility.ParseQueryString(json.answers);
                
                SurveyRequest surveyRequest;
                SurveyResponse surveyResponse = null;
                
                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        
                        if (surveyResponseId.IsEmpty() || surveyResponseId < 1)
                        {
                            surveyRequest = new SurveyRequest
                            {
                                FormId = form.ID.Value,
                                CreateDate = DateTime.Now,
                                LastModifiedDate = DateTime.Now,
                                Uid = Guid.NewGuid(),
                            };
                            msgs.AddRange(surveyRequest.Save(adc));
                            
                            surveyResponse = new SurveyResponse()
                            {
                                CreateDate = DateTime.Now,
                                LastModifiedDate = DateTime.Now,
                                SurveyRequestId = surveyRequest.ID,
                                IsDeleted = false,
                                Uid = Guid.NewGuid(),
                                SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.Completed,
                            };
                            msgs.AddRange(surveyResponse.Save(adc));
                        }
                        else
                        {
                            surveyResponse = SurveyResponse.Load(surveyResponseId);
                        }
                        if(msgs.HasNoErrors() && surveyResponse.IsNotEmpty())
                            AddCustomQuestions(surveyResponse, qvms, Request, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
                
                return Json(
                    new ApiResponse(msgs,new {SurveyResponseId = surveyResponse?.ID, FormId = form?.ID}),
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
                    if (qvm.Question.QuestionTypeId == QuestionType.FileUpload)
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

    }
}