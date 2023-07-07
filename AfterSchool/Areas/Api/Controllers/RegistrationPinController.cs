using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.Api.Controllers
{
    public class RegistrationProcessController : BaseController
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
                
                if(!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");
                
                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                
                if(registration.IsEmpty())
                    throw new Exception("No Registration");
                
                var currentRegistrant = registration.GetRegistrant(msgs);
                
                if(currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");
                
                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                if(currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Master Registrant");

                var currentUserRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
                var participationObject = registration.GetFirstParticipationObject(msgs);
                
                if(participationObject.IsEmpty())
                    throw new Exception("Invalid Object");
                
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
                        MaxGrade = participationObject.GetMaxGradeLevel().Abbreviation,
                        MinAge = participationObject.AgeStart,
                        MaxAge = participationObject.AgeEnd,
                        EnforcedGender = participationObject.GenderRestriction,
                    },
                    setupDto = new 
                    {
                        IsPhoneSetup = currentUser.Phone.IsNotEmpty(),
                        IsPinSetup = currentUser.PinCode.IsNotEmpty(),
                        UserAuthorizationType = currentUser.AuthorizationTypeId.ToString(),
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