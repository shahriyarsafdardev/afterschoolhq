using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Newtonsoft.Json;

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
                var currentRegistrant = currentUser.GetAllRegistrants().FirstOrDefault();
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var activity = participationObject.GetActivity();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                
                // if (currentRegistrant.PersonTypeId == RegistrantPersonType.Parent)
                // {
                //     questions = questions.Where(question =>
                //             question.QuestionDataTypeId.In(FormConstants.QuestionsToIncludeParentPanel()))
                //         .ToList();
                // }
                
                var formQuestionDtoList = new List<FormQuestionDto>();
                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                var formQuestionGroupDtoList = new List<FormQuestionGroupDto>();

                foreach (var question in questions)
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
                             DispalayOrder = question.DisplayOrder,
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
                            DispalayOrder = questionGroup.DisplayOrder,
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

        [HttpPost]

        public ActionResult AddUpdateAnswers(QuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();

            try
            {
                
                var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            //parentUser.GetRegistrantAccount(amsgs, adc)
            //parentUser.GetMasterParentOrStudentRegistrant(amsgs, adc)

            if (currentUser.IsEmpty())
                throw new Exception("Unauthorized");
            
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var currentRegistrant = currentUser.GetAllRegistrants().FirstOrDefault();
            var questions = participationObject.Form.GetAllQuestions(msgs);
            NameValueCollection collection = HttpUtility.ParseQueryString(json.answers);
            
            var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
            
            var firstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
            var lastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
            var phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value;
            var email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value;
            
            var parent1FirstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1FirstName).Value;
            var parent1LastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1LastName).Value;
            var parent1phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1MobilePhone).Value;
            var parent1email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1Email).Value;
            
            var parent2FirstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2FirstName).Value;
            var parent2LastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2LastName).Value;
            var parent2phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2MobilePhone).Value;
            var parent2email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2Email).Value;

            if (currentRegistrant.PersonTypeId == RegistrantPersonType.Parent)
            {
                if (Convert.ToInt32(json.groupid) == (int) QuestionGroupDataType.PersonalInformation)
                {
                    currentAccount.CreateParentOrStudentRegistrant(
                        registrantOwner: null,
                        canBeAccessedBy: currentUser.AsList(),
                        registrantType: RegistrantType.Individual,
                        isMaster: true,
                        isParentOrGuardian: false,
                        allowTextMessages: false,
                        schoolLocationId: null,
                        profileStatus: RegistrantProfileStatus.Started,
                        loggedInUser: currentUser,
                        msgs: msgs);
                }
                
                if (Convert.ToInt32(json.groupid) == (int)QuestionGroupDataType.Parent2Information)
                {
                    currentAccount.CreateParentOrStudentRegistrant(
                        registrantOwner: null,
                        canBeAccessedBy: currentUser.AsList(),
                        registrantType: RegistrantType.Individual,
                        isMaster: true,
                        isParentOrGuardian: true,
                        allowTextMessages: true,
                        schoolLocationId: null,
                        profileStatus: RegistrantProfileStatus.Started,
                        loggedInUser: currentUser,
                        msgs: msgs);
                }
            }
            
            if (currentRegistrant.PersonTypeId == RegistrantPersonType.Student)
            {
                if (Convert.ToInt32(json.groupid) == (int) QuestionGroupDataType.PersonalInformation)
                {
                    currentAccount.CreateParentOrStudentRegistrant(
                        registrantOwner: null,
                        canBeAccessedBy: currentUser.AsList(),
                        registrantType: RegistrantType.Individual,
                        isMaster: true,
                        isParentOrGuardian: true,
                        allowTextMessages: false,
                        schoolLocationId: null,
                        profileStatus: RegistrantProfileStatus.Started,
                        loggedInUser: currentUser,
                        msgs: msgs);
                }
                
                if (Convert.ToInt32(json.groupid) == (int)QuestionGroupDataType.Parent2Information)
                {
                    currentAccount.CreateParentOrStudentRegistrant(
                        registrantOwner: null,
                        canBeAccessedBy: currentUser.AsList(),
                        registrantType: RegistrantType.Individual,
                        isMaster: true,
                        isParentOrGuardian: true,
                        allowTextMessages: true,
                        schoolLocationId: null,
                        profileStatus: RegistrantProfileStatus.Started,
                        loggedInUser: currentUser,
                        msgs: msgs);
                }
            }
            
            qvms.ForEach(qvm =>
            {
                currentRegistrant.AddOrUpdateAnswer(
                    qvm.Value.IfEmpty("").Trim(),
                    qvm.Question,
                    SessionVariables.CurrentUser,
                    msgs);
            });
                
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
            

            return null;
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
            public int? DispalayOrder { get; set; }
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
            public int? DispalayOrder { get; set; }
            public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }
        }

        public class QuestionAnswerResponse
        {
            public string groupid { get; set; }
            public string answers { get; set; }
        }
        
    }
}