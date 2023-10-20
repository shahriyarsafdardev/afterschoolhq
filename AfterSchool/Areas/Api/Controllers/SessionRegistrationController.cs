using System;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Controllers;
using AfterSchool.Helpers;
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
    public class SessionRegistrationController : BaseController
    {
        [HttpGet]
        public ActionResult GetFormAndCheckOutObject(string accountName, string activityGroupName, string registrantUid)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                var currentRegistrant = Registrant.LoadByUid(registrantUid.ToGuid(), msgs);

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");

                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                var currentUserRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
                var account = Account.LoadByUniqueName(accountName, msgs);
                var activityGroup = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, activityGroupName, msgs);

                Registrant parentRegistrant = null;
                Registration registration = null;
                RegistrantActivity registrantActivity = null;
                Registrant submittedRecord;

                if (currentRegistrant.IsStudent)
                    parentRegistrant = currentRegistrant.GetParentOrGuardian(msgs);

                var participationObject = activityGroup.GetParticipationObject(msgs);
                var form = participationObject.Form;
                var questions = form.GetAllQuestions(msgs);
                var questionGroups = form.GetQuestionGroups(msgs);

                if (currentRegistrant.IsAdult)
                    questionGroups = questionGroups.Where(group =>
                        group.QuestionGroupDataTypeId.NotIn(FormConstants.QuestionsGroupToNotIncludeInAdultPanel())
                    ).ToList();

                var formQuestionDtoList = GetAllFormQuestions(currentUserRegistrant, questions);
                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                var formQuestionGroupDtoList = new List<FormQuestionGroupDto>();
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

                foreach (var questionAnswer in FormQuestionAnswer.LoadByRegistrantId(currentRegistrant.ID.Value, msgs))
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

                if (currentUserRegistrant.PersonTypeId == RegistrantPersonType.Parent)
                    formQuestionAnswerDtoList = MapParentEmailToStudent(formQuestionDtoList, formQuestionAnswerDtoList, currentRegistrant, currentUser);



                (submittedRecord, registration) = currentRegistrant.GetSubmittedRecord(participationObject, msgs);

                var checkOut = new List<CheckOutDto>();
                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    checkOut = GetCartItems(),
                    activityGroupDto = new
                    {
                        Name = participationObject.Name,
                        Organization = participationObject.OwnerAccount.Name,
                        RegistrantType = currentUserRegistrant.PersonTypeId.ToString(),
                        FindAnotherActivityUrl = participationObject.OwnerAccount.ShortHomePageUrl,
                        ActivityUrl = participationObject.GetShortHomePageUrl(),
                        RegisterForNewChildUrl = participationObject.GetRegistrationPageUrl(),
                        RegistrationIsCompleted = registration?.CompletionStatusId == RegistrationCompletionStatus.Completed ? true : false,
                        CanAccess = currentUser.HasAccessToRegistrant(currentRegistrant, msgs),
                        RegistrationUid = registration?.Uid,
                        ApplyMode = participationObject.RegistrationMode.ToString(),
                        HeaderImageURL = AccountProfileHelper.GetHeaderImageURL(participationObject),
                    }
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
        private static List<FormQuestionDto> GetAllFormQuestions(Registrant currentUserRegistrant, List<FormQuestion> questions)
        {
            var msgs = new SystemMessages();
            var formQuestionDtoList = new List<FormQuestionDto>();
            var audienceType = GetAudienceType(currentUserRegistrant.PersonTypeId);
            foreach (var question in questions)
            {
                var audienceSettings = FormQuestionSettings.LoadByQuestionIdByAudienceType(question.ID, audienceType, msgs);
                if (audienceSettings?.Show ?? question.IsActive)
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
                            IsRequired = audienceSettings?.Required ?? question.IsRequired,
                            DisplayOrder = question.DisplayOrder,
                            IsActive = question.IsActive,
                            IsDeleted = question.IsDeleted,
                            QuestionTypeId = question.QuestionTypeId,
                            QuestionDataTypeId = question.QuestionDataTypeId,
                            FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId,
                            Link = question.Link,
                        }
                    );
                }
            }
            return formQuestionDtoList;
        }
        private static AudienceType GetAudienceType(RegistrantPersonType type)
        {
            return type switch
            {
                RegistrantPersonType.Parent => AudienceType.Parents,
                RegistrantPersonType.Student => AudienceType.Students,
                RegistrantPersonType.Adult => AudienceType.Adults,
                RegistrantPersonType.Volunteer => AudienceType.Volunteer,
                _ => AudienceType.NotSet,
            };
        }
        private static List<FormQuestionAnswerDto> MapParentEmailToStudent(List<FormQuestionDto> formQuestionDtoList,
            List<FormQuestionAnswerDto> formQuestionAnswerDtoList, Registrant currentRegistrant, WebUser currentUser)
        {
            var msgs = new SystemMessages();
            var registrantEmailFormQuestion =
                formQuestionDtoList.FirstOrDefault(question => question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
            var registrantEmailFormQuestionAnswer =
                FormQuestionAnswer.LoadByRegistrantIdByQuestionDataTypeId(currentRegistrant.ID.Value, QuestionDataType.RegistrantEmail, msgs);

            if (registrantEmailFormQuestion.IsNotEmpty() && registrantEmailFormQuestionAnswer.IsEmpty())
                formQuestionAnswerDtoList.Add(
                    new FormQuestionAnswerDto
                    {
                        FormQuestionId = registrantEmailFormQuestion.ID,
                        QuestionDataTypeId = (int)registrantEmailFormQuestion.QuestionDataTypeId,
                        QuestionText = registrantEmailFormQuestion.Text,
                        Text = currentUser.Email,
                    }
                );

            return formQuestionAnswerDtoList;
        }
        [HttpGet]
        private List<CheckOutDto> GetCartItems()
        {
            var msgs = new SystemMessages();
            var cartItems = SessionVariables.Cart?.GetCartItems(msgs);
            var activities = new List<CheckOutDto>();
            foreach (var cartItem in cartItems)
            {
                var participationObject = cartItem.GetParticipationObject(msgs);
                if (participationObject.ObjectType == ParticipationObjectType.Activity)
                    activities.Add(
                        new CheckOutDto()
                        {
                            ActivityName = participationObject.UniqueName,
                            DaysOfWeek = participationObject.Schedule.DaysOfTheWeek,
                            IsGradeRestricted = participationObject.RestrictByGrade(),
                            MinGrade = participationObject.GetMinGradeLevel().Abbreviation,
                            MaxGrade = participationObject.GetMaxGradeLevel().Abbreviation,
                            IsAgeRestricted = (participationObject.AgeStart != null ? true : false),
                            MinAge = participationObject.AgeStart,
                            MaxAge = participationObject.AgeEnd,
                            IsGenderRestricted = participationObject.IsGenderEnforced,
                            EnforcedGender = participationObject.GenderRestriction,
                        });
            }
            return activities;
        }
    }
}