using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
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
                var currentAccount = SessionVariables.CurrentAccount;
                var currentRegistrant = currentUser.GetAllRegistrants().FirstOrDefault();
                var studentRegistrant = currentAccount.GetStudentRegistrants(msgs).FirstOrDefault();
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var activity = participationObject.GetActivity();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                var questionGroups = participationObject.Form.GetQuestionGroups(msgs);
                
                if (currentRegistrant.PersonTypeId == RegistrantPersonType.Adult)
                {
                    questionGroups = questionGroups.Where(group =>
                        group.QuestionGroupDataTypeId.NotIn(FormConstants.QuestionsGroupToNotIncludeInAdultPanel())
                    ).ToList();
                }
                
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
                
                foreach (var questionGroup in questionGroups)
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
                
                foreach (var questionAnswer in FormQuestionAnswer.LoadByRegistrantId((currentRegistrant.PersonTypeId == RegistrantPersonType.Parent ? studentRegistrant.ID.Value : currentRegistrant.ID.Value)))
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
                //var currentRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
                var currentRegistrant = currentUser.GetAllRegistrants().FirstOrDefault();
                var studentRegistrant = currentAccount.GetStudentRegistrants(msgs).FirstOrDefault();
                var parentRegistrant = currentAccount.GetFirstParentOrGuardianRegistrant(msgs);
                var parent2Registrant = currentAccount.GetSecondParentOrGuardianRegistrant(msgs);

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");

                var group = Regex.Match(json.groupid, @"\d+").Value;
                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                NameValueCollection collection = HttpUtility.ParseQueryString(json.answers);
                var questionGroups = participationObject.Form.GetQuestionGroups(msgs);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                
                var parent1FirstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1FirstName).Value;
                var parent1LastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1LastName).Value;
                var parent1MiddleName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1MiddleName).Value;
                var parent1Phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1MobilePhone).Value;
                var parent1Email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1Email).Value;

                var parent2FirstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2FirstName).Value;
                var parent2LastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2LastName).Value;
                var parent2MiddleName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2MiddleName).Value;
                var parent2Phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2MobilePhone).Value;
                var parent2Email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2Email).Value;

                if (currentRegistrant.PersonTypeId == RegistrantPersonType.Parent)
                {
                    if (Convert.ToInt32(group) == (int) QuestionGroupDataType.Parent2Information && parent2Registrant.IsEmpty())
                    {
                        var parent2Reg = currentAccount.CreateParentOrStudentRegistrant(
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

                        parent2Reg.FirstName = parent2FirstName;
                        parent2Reg.LastName = parent2LastName;
                        parent2Reg.MiddleName = parent2MiddleName;
                        parent2Reg.Email = parent2Email;
                        parent2Reg.Phone = parent2Phone;

                        parent2Reg.Save();
                    }
                }
                
                if (currentRegistrant.PersonTypeId == RegistrantPersonType.Student)
                {
                    if (Convert.ToInt32(group) == (int) QuestionGroupDataType.Parent1Information && parentRegistrant.IsNotEmpty())
                    {
                        var parent1Reg = currentAccount.CreateParentOrStudentRegistrant(
                            registrantOwner: currentUser,
                            canBeAccessedBy: currentUser.AsList(),
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: true,
                            allowTextMessages: false,
                            schoolLocationId: null,
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: currentUser,
                            msgs: msgs);
        
                        parent1Reg.FirstName = parent1FirstName;
                        parent1Reg.LastName = parent1LastName;
                        parent1Reg.MiddleName = parent1MiddleName;
                        parent1Reg.Email = parent1Email;
                        parent1Reg.Phone = parent1Phone;
    
                        parent1Reg.Save();

                        // if (msgs.HasErrors())
                        // {
                        //     currentRegistrant.ParentRegistrantId = parent1Reg.ID;
                        //     currentRegistrant.Save();
                        // }
                    }
                    
                    if (Convert.ToInt32(group) == (int) QuestionGroupDataType.Parent2Information && parent2Registrant.IsNotEmpty())
                    {
                        var parent2Reg = currentAccount.CreateParentOrStudentRegistrant(
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

                        parent2Reg.FirstName = parent2FirstName;
                        parent2Reg.LastName = parent2LastName;
                        parent2Reg.MiddleName = parent2MiddleName;
                        parent2Reg.Email = parent2Email;
                        parent2Reg.Phone = parent2Phone;

                        parent2Reg.Save();
                    }
                }
                
                qvms.ForEach(qvm =>
                {
                    studentRegistrant.AddOrUpdateAnswer(
                        qvm.Value.IfEmpty("").Trim(),
                        qvm.Question,
                        currentUser,
                        msgs);
                });

                if ((int)questionGroups.LastOrDefault().QuestionGroupDataTypeId == Convert.ToInt32(group) && currentRegistrant.PersonTypeId != RegistrantPersonType.Adult)
                {
                    RegistrationCommon.MapStudentAnswersToParent1(studentRegistrant, parentRegistrant, questions, currentUser, msgs);
                    RegistrationCommon.MapStudentAnswersToParent2(studentRegistrant, parent2Registrant, questions, currentUser, msgs);
                }
                
                return Json(
                    new ApiResponse(msgs,null),
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
        public ActionResult Register(QuestionAnswerResponse json)
        {
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                Registrant submittedRecord;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var msgs = new SystemMessages();
                
                if (json.esignature.IsEmpty())
                    msgs.Add("E-signature is required", MessageType.UserError);
                
                var currentRegistrant = currentUser.GetAllRegistrants().FirstOrDefault();
                var studentRegistrant = currentAccount.GetStudentRegistrants(msgs).FirstOrDefault();

                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                NameValueCollection collection = HttpUtility.ParseQueryString(json.answers);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                var registration = RegistrationCommon.CreateRegistration(participationObject, qvms, json.esignature, Request, msgs);
                
                // (submittedRecord ?? student).AddOrUpdateAnswer(field, question, registration?.ID, currentUser, amsgs, adc);

                /*if (currentRegistrant.PersonTypeId == RegistrantPersonType.Parent)
                {
                    submittedRecord = studentRegistrant.SubmitRecordToAccount(
                        participationObject.OwnerAccount,
                        currentUser,
                        msgs);
                }
                else
                {
                    submittedRecord = currentRegistrant.SubmitRecordToAccount(
                        participationObject.OwnerAccount,
                        currentUser,
                        msgs);
                }*/
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
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
            
            public string esignature { get; set; }
            public string answers { get; set; }
        }
        
    }
}