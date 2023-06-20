using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

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
        [HttpPost]
        public ActionResult AddUpdateAnswers(UpdateQuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();

            try
            {
                var currentUser = SessionVariables.CurrentUser;
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var registration = Registration.LoadByUid(json.registrationUid.ToGuid(), msgs);
                if(registration.IsEmpty())
                    throw new Exception("No Registration");
                
                var currentRegistrant = registration.GetRegistrant(msgs);
                if(currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");
                
                var collection = HttpUtility.ParseQueryString(json.answers);
                
                var participationObject = registration.GetFirstParticipationObject(msgs);
                
                if(participationObject.IsEmpty())
                    throw new Exception("Invalid Object");
                
                var form = participationObject.Form;
                var questions = form.GetAllQuestions(msgs);
                
                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                
                qvms.Where(qvm => qvm.Question.FormQuestionGroupId == json.groupid.ToInteger()).ToList().ForEach(qvm =>
                {
                    switch (qvm.Question.QuestionDataTypeId)
                    {
                        case QuestionDataType.RegistrantMobilePhone:
                        case QuestionDataType.RegistrantDoctorPhone:
                        case QuestionDataType.RegistrantHomePhone:
                        case QuestionDataType.RegistrantWorkPhone:
                        case QuestionDataType.RegistrantPhone1:
                        case QuestionDataType.RegistrantPhone2:
                        case QuestionDataType.Parent1Phone1:
                        case QuestionDataType.Parent2Phone1:
                        case QuestionDataType.Parent1Phone2:
                        case QuestionDataType.Parent2Phone2:
                        case QuestionDataType.Parent1MobilePhone:
                        case QuestionDataType.Parent2MobilePhone:
                        case QuestionDataType.Parent1WorkPhone:
                        case QuestionDataType.Parent2WorkPhone:
                        case QuestionDataType.Parent1HomePhone:
                        case QuestionDataType.Parent2HomePhone:
                        case QuestionDataType.EmergencyContact1Phone:
                        case QuestionDataType.EmergencyContact2Phone:
                        case QuestionDataType.EmergencyContact3Phone:
                        case QuestionDataType.EmergencyContact1Phone2:
                        case QuestionDataType.EmergencyContact2Phone2:
                        case QuestionDataType.EmergencyContact3Phone2:
                        case QuestionDataType.EmergencyContact1Phone3:
                        case QuestionDataType.EmergencyContact2Phone3:
                        case QuestionDataType.EmergencyContact3Phone3:
                            if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                                msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
                            break;
                        case QuestionDataType.RegistrantEmail:
                        case QuestionDataType.Parent1Email:
                        case QuestionDataType.Parent2Email:
                            if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                                msgs.Add($"{qvm.Question.Text} must be a valid email", MessageType.UserError);
                            break;
                    }
                });

                if (json.schoolLocationID.IsNotEmpty() && currentRegistrant.IsStudent)
                    currentRegistrant.SchoolLocationId = json.schoolLocationID;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        qvms.ForEach(qvm =>
                        {

                            if (qvm.Question.QuestionDataTypeId != QuestionDataType.NotSet)
                            {
                                currentRegistrant.AddOrUpdateAnswer(
                                    qvm.Value.IfEmpty("").Trim(),
                                    qvm.Question,
                                    registration.ID.Value,
                                    currentUser,
                                    amsgs,
                                    adc);
                            }
                        });
                        
                        RegistrationCommon.AddCustomQuestions(
                            qvms,
                            currentRegistrant,
                            registration,
                            currentUser,
                            Request,
                            amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );

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
        public ActionResult CreateOrUpdateFromGoogle(LocationDto json)
        {
            var msgs = new SystemMessages();
            
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var location = Location.LoadByGooglePlaceId(json.googlePlaceId, msgs);

                if (location.IsEmpty())
                    location = new Location()
                    {
                        GooglePlaceId = json.googlePlaceId,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = currentUser?.ID,
                    };

                if (json.locationName.IsEmpty())
                    msgs.Add("Location Name is required", MessageType.UserError);

                if (json.city.IsEmpty())
                    msgs.Add("Location City is required", MessageType.UserError);

                if (json.state.IsEmpty())
                    msgs.Add("Location State is required", MessageType.UserError);

                if (json.country.IsEmpty())
                    msgs.Add("Location Country is required", MessageType.UserError);

                if (msgs.HasNoErrors())
                {
                    location.Name = json.locationName;
                    location.StreetNumber = json.streetNumber;
                    location.StreetName = json.streetName;
                    location.Address1 = $"{json.streetNumber} {json.streetName}";
                    location.Address2 = json.address2;
                    location.Neighborhood = json.neighborhood;
                    location.City = json.city;
                    location.County = json.county;
                    location.State = json.state;
                    location.PostalCode = json.postalCode;
                    location.Country = json.country;
                    location.Latitude = json.latitude;
                    location.Longitude = json.longitude;
                    location.LastModifiedDate = DateTime.UtcNow;
                    location.LastModifiedBy = currentUser?.ID;

                    //..co-ordiantes can be negative
                    if (json.longitude.IsNotEmpty() && json.latitude.IsNotEmpty())
                    {
                        var distance = DistanceAlgorithm.DistanceBetweenPlaces(Convert.ToDouble(json.longitude), Convert.ToDouble(json.latitude), 0, 0);
                        if (distance.IsNotEmpty())
                            location.DistanceFromOrigin = Convert.ToDecimal(distance);
                    }
                
                    msgs.AddRange(location.Save());
                }
                
                return Json(
                    new ApiResponse(msgs,location.ID),
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