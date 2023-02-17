using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.ReactApi.Controllers
{
    public class ReactController : BaseController
    {
        [HttpGet]
        public ActionResult GetCurrentUser()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;

                if (currentUser.IsEmpty())
                {
                    msgs.Add("User Not Found" , MessageType.ApiError);
                    throw new Exception("Unauthorized");
                }

                return Json(
                new ApiResponse(msgs,
                    new WebUserDto(currentUser)),
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
        public ActionResult GetForm()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var activity = participationObject.GetActivity();
                var formQuestionDtoList = new List<FormQuestionDto>();
                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                var formQuestionGroupDtoList = new List<FormQuestionGroupDto>();

                foreach (var question in participationObject.Form.GetAllQuestions(msgs))
                {
                    formQuestionDtoList.Add(
                        new FormQuestionDto()
                        {
                             ID = question.ID.Value,
                             FormQuestionGroupId = question.FormQuestionGroupId,
                             Text = question.Text,
                             Description = question.Description,
                             Options = question.Options,
                             IsRequired = question.IsRequired,
                             QuestionTypeId = question.QuestionTypeId,
                             QuestionDataTypeId = question.QuestionDataTypeId,
                             FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId
                        }
                    );
                }
                
                foreach (var questionGroup in participationObject.Form.GetQuestionGroups(msgs))
                {
                    formQuestionGroupDtoList.Add(
                        new FormQuestionGroupDto()
                        {
                            FormId = questionGroup.FormId,
                            Name = questionGroup.Name,
                            QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                        }
                    );
                }
                
                foreach (var questionAnswer in FormQuestionAnswer.LoadByRegistrantId((int)currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs).ID))
                {
                    formQuestionAnswerDtoList.Add(
                        new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId,
                            QuestionDataTypeId = (int)questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText,
                            Text = questionAnswer.Text
                        }
                    );
                }

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    activityDto = new
                    {
                        Name = activity.Name,
                        Organization = activity.AccountName
                    }
                };
                
                return Json(
                    new ApiResponse(msgs,response),
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

        public class ApiResponse
        {
            public SystemMessages msgs { get; set; }
            public dynamic obj { get; set; }

            public ApiResponse(SystemMessages msgs, dynamic obj)
            {
                this.msgs = msgs;
                this.obj = obj;
            }
        }

        public class WebUserDto
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string userName { get; set; }

            public WebUserDto(WebUser currentUser)
            {
                this.firstName = currentUser.FirstName;
                this.lastName = currentUser.LastName;
                this.email = currentUser.Email;
                this.phone = currentUser.Phone;
                this.userName = currentUser.Username;
            }
            
        }
        
        public class FormDto
        {
            public List<FormQuestionDto> formQuestion { get; set; }
            public List<FormQuestionGroupDto> formQuestionGroup { get; set; }
            public List<FormQuestionAnswerDto> formQuestionAnswer { get; set; }

            public FormDto(List<FormQuestionDto> formQuestion, List<FormQuestionGroupDto> formQuestionGroup, List<FormQuestionAnswerDto> formQuestionAnswer)
            {
                this.formQuestion = formQuestion;
                this.formQuestionGroup = formQuestionGroup;
                this.formQuestionAnswer = formQuestionAnswer;
            }
            
        }

        public class FormQuestionDto
        {
            public int ID { get; set; }
            public int? FormQuestionGroupId { get; set; }
            public string Text { get; set; }
            public string Description { get; set; }
            public string Options { get; set; }
            public bool IsRequired { get; set; }
            public QuestionType QuestionTypeId { get; set; }
            public QuestionDataType QuestionDataTypeId { get; set; }
            public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get; set; }
        }
        
        public class FormQuestionAnswerDto
        {
            public int? FormQuestionId { get; set; }
            public int QuestionDataTypeId { get; set; }
            public string QuestionText { get; set; }
            public string Text { get; set; }
        }
        
        public class FormQuestionGroupDto
        {
            public int? FormId { get; set; }
            public string Name { get; set; }
            public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }
        }
    }
}