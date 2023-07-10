using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.Api.Controllers
{
    public class RegistrationPinController : BaseController
    {
        [HttpGet]
        public ActionResult GetRegistrationSettings(string registrationUid)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");

                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);

                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);

                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                if (currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Master Registrant");

                var currentUserRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
                var participationObject = registration.GetFirstParticipationObject(msgs);

                if (participationObject.IsEmpty())
                    throw new Exception("Invalid Object");

                var answers = FormQuestionAnswer.LoadByRegistrantId(currentRegistrant.ID.Value, msgs);
                var additionalContactInfo = new List<PickUpDto>();

                additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.Parent1FirstName,
                    QuestionDataType.Parent1LastName, QuestionDataType.Parent1MobilePhone, FormConstants.Parent1InfoToIncludeInStudentPanel()));

                additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.Parent2FirstName,
                    QuestionDataType.Parent2LastName, QuestionDataType.Parent2MobilePhone, FormConstants.Parent2InfoToIncludeInStudentPanel()));

                additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.EmergencyContact1FirstName,
                    QuestionDataType.EmergencyContact1LastName, QuestionDataType.EmergencyContact1Phone,
                    FormConstants.EmergencyContact1InfoToIncludeInStudentPanel()));

                additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.EmergencyContact2FirstName,
                    QuestionDataType.EmergencyContact2LastName, QuestionDataType.EmergencyContact2Phone,
                    FormConstants.EmergencyContact2InfoToIncludeInStudentPanel()));

                additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.EmergencyContact3FirstName,
                    QuestionDataType.EmergencyContact3LastName, QuestionDataType.EmergencyContact3Phone,
                    FormConstants.EmergencyContact3InfoToIncludeInStudentPanel()));

                dynamic response = new
                {
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
                        MaxGrade = participationObject.GetMaxGradeLevel(msgs).Abbreviation,
                        MinAge = participationObject.AgeStart,
                        MaxAge = participationObject.AgeEnd,
                        EnforcedGender = participationObject.GenderRestriction,
                        HeaderImageURL = AccountProfileHelper.GetHeaderImageURL(participationObject),
                        IsRequireCheckInCheckOut = participationObject.IsRequireCheckInCheckOutEnabled.IsNotEmpty() &&
                                                   participationObject.IsRequireCheckInCheckOutEnabled,
                    },
                    setupDto = new
                    {
                        IsPhoneSetup = currentUser.Phone.IsNotEmpty(),
                        IsPinSetup = currentUser.PinCode.IsNotEmpty(),
                        UserAuthorizationType = currentUser.AuthorizationTypeId.ToString(),
                    },
                    additionalContactDto = additionalContactInfo,
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

        [HttpPost]
        public ActionResult AddOrUpdateRegistrantWebUser(string registrationUid, string phone, string pinCode, List<PickUpDto> peopleList)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");

                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                if (phone.IsNotEmpty() && WebUser.ExistsByPhone(phone.Trim(), msgs))
                    throw new Exception("Phone is already in use");

                if (peopleList.Count > 0)
                    peopleList.ForEach(currentPickUp =>
                    {
                        if (currentPickUp.Phone.IsNotEmpty() && WebUser.ExistsByPhone(currentPickUp.Phone.Trim(), msgs))
                            throw new Exception("Phone is already in use");
                    });
                
                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                if (currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Master Registrant");

                Database.ExecuteTransaction
                (
                    (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (currentUser.Phone.IsEmpty() || currentUser.PinCode.IsEmpty())
                        {
                            if (currentUser.Phone.IsEmpty())
                                currentUser.Phone = phone;

                            if (currentUser.PinCode.IsEmpty())
                                currentUser.PinCode = pinCode;

                            amsgs.AddRange(currentUser.Save(adc));
                        }

                        if (amsgs.HasNoErrors() && peopleList.Count > 0)
                            peopleList.ForEach(currentPickUp =>
                            {
                                var user = new WebUser
                                {
                                    FirstName = currentPickUp.FirstName,
                                    LastName = currentPickUp.LastName,
                                    Phone = currentPickUp.Phone,
                                    CreateDate = DateTime.UtcNow,
                                    LastModifiedDate = DateTime.UtcNow,
                                };
                                amsgs.AddRange(user.Save(adc));

                                var registrantWebUser = new RegistrantWebUser
                                {
                                    CreateDate = DateTime.UtcNow,
                                    RegistrantId = currentMasterRegistrant.ID,
                                    WebUserId = user.ID,
                                    IsDeleted = false,
                                };
                                amsgs.AddRange(registrantWebUser.Save(adc));
                            });
                    },
                    (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );

                return Json(
                    new ApiResponse(msgs, null),
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

        private static List<PickUpDto> GetAdditionalContactInfo(List<FormQuestionAnswer> answers, QuestionDataType firstName, QuestionDataType lastName,
            QuestionDataType phone, List<QuestionDataType> questionDataTypes)
        {
            var pickUpContactList = new List<PickUpDto>();
            var contactInfo = answers.Where(x => x.QuestionDataTypeId.In(questionDataTypes)).ToList();

            if (contactInfo.Count > 0)
                pickUpContactList.Add(new PickUpDto()
                {
                    FirstName = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == firstName)?.Text,
                    LastName = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == lastName)?.Text,
                    Phone = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == phone)?.Text,
                });
            return pickUpContactList;
        }
    }
}