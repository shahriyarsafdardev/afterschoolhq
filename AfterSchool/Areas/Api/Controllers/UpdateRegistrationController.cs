using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.Api.Controllers
{
    public class UpdateRegistrationController : Controller
    {
        [HttpGet]
        public ActionResult GetRegistrationForm(string regUid)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                if(!regUid.IsGuid())
                    throw new Exception("Invalid Id");
                
                var registration = Registration.LoadByUid(regUid.ToGuid(), msgs);
                
                if(registration.IsEmpty())
                    throw new Exception("No Registration");
                
                var currentRegistrant = registration.GetRegistrant(msgs);
                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                
                if(currentRegistrant.IsEmpty() || currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                if(!currentUser.HasAccessToRegistrant(currentMasterRegistrant, msgs))
                    throw new Exception("No Access");
                
                var currentUserRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
                var participationObject = registration.GetFirstParticipationObject(msgs);
                
                if(participationObject.IsEmpty())
                    throw new Exception("Invalid Object");
                
                var form = participationObject.Form;
                var questions = form.GetAllQuestions(msgs);
                var questionGroups = form.GetQuestionGroups(msgs);
                
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
                             GroupTitle = question.FormQuestionGroupName,
                             Options = question.Options,
                             IsRequired = question.IsRequired,
                             DisplayOrder = question.DisplayOrder,
                             IsActive = question.IsActive,
                             IsDeleted = question.IsDeleted,
                             QuestionTypeId = question.QuestionTypeId,
                             QuestionDataTypeId = question.QuestionDataTypeId,
                             FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId,
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
                            DisplayOrder = questionGroup.DisplayOrder,
                            QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                        }
                    );
                }
                
                foreach (var questionAnswer in registration.GetAnswers(msgs))
                {
                    formQuestionAnswerDtoList.Add(
                        new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId,
                            QuestionDataTypeId = (int)questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText,
                            Text = questionAnswer.Text,
                        }
                    );
                }

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    activityDto = new
                    {
                        Name = participationObject.Name,
                        Organization = participationObject.OwnerAccount.Name,
                        IsFree = participationObject.IsFree,
                        RegistrantType = currentUserRegistrant.PersonTypeId.ToString(),
                        FindAnotherActivityUrl = participationObject.OwnerAccount.ShortHomePageUrl,
                        ActivityUrl = participationObject.GetShortHomePageUrl(),
                        RegisterForNewChildUrl = participationObject.GetRegistrationPageUrl(),
                        IsGradeRestricted = participationObject.RestrictByGrade(),
                        SchoolLocation = currentRegistrant.GetSchoolLocation(msgs),
                        IsAgeRestricted = participationObject.AgeStart != null,
                        IsGenderRestricted = participationObject.IsGenderEnforced,
                        RegistrationIsCompleted = registration?.CompletionStatusId == RegistrationCompletionStatus.Completed,
                        CanAccess = currentUser.HasAccessToRegistrant(currentMasterRegistrant, msgs),
                        RegistrationUid = registration?.Uid,
                        IsAlreadyPaid = registration?.IsAlreadyPaidFor(msgs),
                        MinGrade = participationObject.GetMinGradeLevel(msgs).Abbreviation,
                        MaxGrade = participationObject.GetMaxGradeLevel().Abbreviation,
                        MinAge = participationObject.AgeStart,
                        MaxAge = participationObject.AgeEnd,
                        EnforcedGender = participationObject.GenderRestriction,
                    },
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
    }
}