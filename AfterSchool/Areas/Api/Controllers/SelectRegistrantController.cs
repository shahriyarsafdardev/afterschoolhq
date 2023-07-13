using AfterSchool.Areas.Api.Models;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Clever.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Web.Mvc;
using System.Windows.Interop;

namespace AfterSchool.Areas.Api.Controllers
{
    public class SelectRegistrantController : BaseController
    {
        [HttpGet]

        public ActionResult GetRegistrantsForParent()
        {
            var msgs = new SystemMessages();

            try
            {
                var currentAccount = SessionVariables.CurrentAccount;
                var currentUser = SessionVariables.CurrentUser;


                if (currentAccount.IsEmpty() || currentUser.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var masterRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
                if (masterRegistrant.IsEmpty())
                    throw new Exception("No master registrant found");

                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

                if (participationObject.IsEmpty())
                    throw new Exception("No activity found");


                var students = currentAccount.GetStudentRegistrants(msgs);

                var registrantsDtos = new List<SelectRegistrantsDto>();

                var selectedDates = RegistrationCommon.GetSelectedDatesFromCart();

                //Get things for ParticipationObject
                var poRegistrationMode = participationObject.RegistrationMode;
                var poSeatsRemaining = participationObject.SeatsRemaining;
                var poSubscriptionPlans = participationObject.SubscriptionPlans.Any();
                var poSubscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault();
                var poParticipationObject = participationObject.IsSession();
                var poActivityGroupType = participationObject.ActivityGroupType;
                var activityName = participationObject.Name;
                var orgName = participationObject.OwnerAccount.Name;
                var profile = participationObject.OwnerAccount.GetAccountProfile(msgs);
                var HeaderImageUrl = profile.GetHeaderImageUrl(msgs);
                var resParticipationObject = new SRParticipationObjectDTO
                {
                    ParticipationRegistrationMode = poRegistrationMode,
                    SeatsRemaining = poSeatsRemaining,
                    AnySubscriptionPlans = poSubscriptionPlans,
                    FirstSubscriptionPlan = poSubscriptionPlan,
                    ParticipationObjectIsSession = poParticipationObject,
                    ParticipationActivityGroupType = poActivityGroupType,
                    ActivityName = activityName,
                    OrganizationName = orgName,
                    HeaderImageUrl = HeaderImageUrl
                };



                //Get things for Students
                foreach (var student in students)
                {
                    var registered = "Enrolled";
                    var register = "Enroll";
                    if (participationObject.RegistrationMode == RegistrationMode.Apply)
                    {
                        register = "Apply";
                        registered = "Applied";
                    }

                    if (participationObject.SeatsRemaining == 0)
                    {
                        register = "Join Waiting List";
                    }
                    //changed
                    var disabled = false;
                    var btnText = ""; 
                    var registerUrl = "/account/setstudent/"+student.Uid;
                    var btnUrl = registerUrl;
                    
                    //

                    var registrantGrade = student.GetAnswer(QuestionDataType.RegistrantGrade);
                    var registrantGradeText = registrantGrade?.Text;
                    var canStillRegister = false;


                    var ineligibleText = "";

                    var isGradeRestricted = participationObject.IsNotEmpty() && participationObject.RestrictByGrade() && !participationObject.IsStudentGradeInRange(student);
                    var isAgeRestricted = participationObject.IsNotEmpty() && !participationObject.RestrictByGrade() && !participationObject.IsStudentAgeInRange(student);
                    var isGenderRestricted = participationObject.IsNotEmpty() && participationObject.IsGenderEnforced
                                           && !participationObject.AcceptsGenderType(student.GenderId);


                    if (participationObject.CanRegisterPerClass)
                    {
                        canStillRegister = !student.IsReservedForAllDates(participationObject, selectedDates, null);
                    }


                    if (student.HasSubmittedFor(participationObject, null) && !canStillRegister)
                    {
                        if (student.HasWithdrawn(participationObject))
                        {
                            if (participationObject.SubscriptionPlans.Any() && student.HasCancelledSubscription(participationObject))
                            {
                                btnText = "Re-Enroll »";
                            }
                            else
                            {
                                btnText = "Withdrawn";
                                disabled = true;
                                btnUrl = "#";
                            }
                        }
                        else if (!student.HasConsentFor(participationObject))
                        {
                            btnText = "Give Consent";
                        }
                        else if (student.OnWaitingListFor(participationObject))
                        {
                            btnText = "On Waiting List";
                            disabled = true;
                            btnUrl = "#";
                        }
                        else if (!student.IsReservedAndPaidFor(participationObject, null) && !student.IsAppliedFor(participationObject, null) && !student.MasterRegistrantHasPlan(participationObject))
                        {
                            var subscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault();

                            if (subscriptionPlan.IsNotEmpty() && subscriptionPlan.SplitCostEvenly && !participationObject.RegistrationFinalized)
                            {
                                btnText = "Awaiting Finalization";
                                disabled = true;
                                btnUrl = "#";
                            }
                            else
                            {
                                btnText = "Pay Now »";
                                btnUrl = Url.Action("makepayment", "account", new { id = student.GetRegistrationWithParticipationObject(participationObject).Uid });
                            }
                        }
                        else if (participationObject.IsSession())
                        {
                            btnText = "Add To Existing Registration »";
                        }
                        else
                        {
                            btnText = $"Already {registered}";
                            disabled = true;
                            btnUrl = "#";
                        }
                    }
                    else
                    {

                        if (!student.IsProfileCompleted)
                        {
                            btnText = "Update Profile &; Enroll »";
                            btnUrl = registerUrl;
                        }
                        else
                        {
                            if (participationObject.ActivityGroupType != ActivityGroupType.Session && (isGenderRestricted || isGradeRestricted || isAgeRestricted))
                            {
                                btnText = "Ineligible";
                                disabled = true;
                                ineligibleText = ParticipationObjectUI.PrintIneligibilityMessageString(participationObject,
                                    isGenderRestricted, isAgeRestricted, isGradeRestricted);
                            }
                            else
                            {
                                btnText = register + " »";
                            }

                        }
                    }
                    var updateProfileLink = $"/reactapp/updateprofile/{student.Uid}";
                    var newStudent = new SelectRegistrantsDto
                    {
                        ID = student.ID,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        Registered = registered,
                        Register = register,
                        Disabled = disabled,
                        BtnText = btnText,
                        RegisterUrl = registerUrl,
                        UpdateProfileLink = updateProfileLink,
                        BtnUrl = btnUrl,
                        RegistrantGradeText = UILabel.GetGradeDescription(registrantGradeText),
                        CanStillRegister = canStillRegister,
                        IneligibleText = ineligibleText,
                        isAdult = student.IsAdult,
                        isStudent = student.IsStudent,
                        isParentOrGuardien = student.IsParentOrGuardian
                    };
                    registrantsDtos.Add(newStudent);
                }

                var Response = new
                {
                    Students = registrantsDtos,
                    ParticipationObject = resParticipationObject,
                };

                return Json(
                    new ApiResponse(msgs, Response),
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
        

       

        [HttpGet]

        public ActionResult AddNewRegistrant()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentAccount = SessionVariables.CurrentAccount;
                var currentUser = SessionVariables.CurrentUser;
                if (currentUser.IsEmpty() || currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

                if (participationObject.IsEmpty())
                    throw new Exception("No activity found");


                var parentRegistrant = currentAccount.GetFirstParentOrGuardianRegistrant(msgs);
                var parent2Registrant = currentAccount.GetSecondParentOrGuardianRegistrant(msgs);

                Registrant studentReg = null;

                Database.ExecuteTransaction
                    (
                        (amsgs, adc) =>
                        {
                            studentReg = currentAccount.CreateParentOrStudentRegistrant(
                                registrantOwner: null,
                                canBeAccessedBy: currentUser.AsList(),
                                registrantType: RegistrantType.Individual,
                                isMaster: true,
                                isParentOrGuardian: false,
                                allowTextMessages: false,
                                schoolLocationId: null,
                                profileStatus: RegistrantProfileStatus.NotSet,
                                loggedInUser: currentUser,
                                msgs: amsgs,
                                dc: adc);

                            if (parentRegistrant.IsNotEmpty())
                                RegistrationCommon.MapParent1AnswersToStudent(parentRegistrant, studentReg, participationObject.Form.GetAllQuestions(msgs), currentUser, amsgs, adc);

                            if (parent2Registrant.IsNotEmpty())
                                RegistrationCommon.MapParent2AnswersToStudent(parent2Registrant, studentReg, participationObject.Form.GetAllQuestions(msgs), currentUser, amsgs, adc);
                        },
                        smsgs =>
                        {
                            msgs.AddRange(smsgs);
                            SessionVariables.CurrentRegistrant = studentReg;
                        },
                        fmsgs => msgs.AddRange(fmsgs),
                        emsgs => throw new Exception(emsgs.ToString())
                    );

                var addnewStudentUrl = $"/ReactApp/registration/new-child/{participationObject.OwnerAccount.UniqueName}/{participationObject.UniqueName}/{studentReg.Uid}";

                var Response = new
                {
                    AddNewStudentUrl = addnewStudentUrl,

                };

                return Json(
                    new ApiResponse(msgs, Response),
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
    }
    
    
}