using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Api.Controllers
{
    public class SurveyRequestController : Controller
    {
        [HttpGet]
        public ActionResult GetForm(string uid, int pin)
        {
            var msgs = new SystemMessages();
            try
            {
                if (!uid.IsGuid())
                    throw new Exception("Invalid Id.");
                var surveyRequest = SurveyRequest.LoadByUid(uid.ToGuid(), msgs);
                if (surveyRequest.IsEmpty())
                    throw new Exception("Invalid Request.");
                var surveyResponse = SurveyResponse.LoadByPin(pin,msgs);
                if (surveyResponse.IsEmpty())
                    throw new Exception("Invalid Pin.");
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
                        FormQuestionId = questionAnswer.FormQuestionId, QuestionDataTypeId = (int) questionAnswer.QuestionDataTypeId,
                        QuestionText = questionAnswer.QuestionText, Text = MapToAmazonFilePath(questionAnswer),
                        Description = questionAnswer.Description,
                    })
                    .ToList();

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    SurveyResponseStatus = surveyResponse.SurveyFeedbackStatusTypeId.ToString(),
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
            return amazonHelper.GetFileLink(question.Text);
        }
    }
}