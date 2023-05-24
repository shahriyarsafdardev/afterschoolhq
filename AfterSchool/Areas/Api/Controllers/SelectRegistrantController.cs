using AfterSchool.Areas.Api.Models;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
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
        //From ids


        //From session
        public ActionResult GetRegistrantsForParent()
        {
            var msgs = new SystemMessages();

            try
            {

                var currentAccount = SessionVariables.CurrentAccount;
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");
                var currentUser = SessionVariables.CurrentUser;
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized Account");
                var masterRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant();
                if (masterRegistrant == null)
                    throw new Exception("No master registrant found");

                var participationObject = RegistrationCommon.GetParticipationObjectFromCart(); //Might use from SessionVariables

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

                var resParticipationObject = new SRParticipationObjectDTO
                {
                    ParticipationRegistrationMode = poRegistrationMode,
                    SeatsRemaining = poSeatsRemaining,
                    AnySubscriptionPlans = poSubscriptionPlans,
                    FirstSubscriptionPlan = poSubscriptionPlan,
                    ParticipationObjectIsSession = poParticipationObject,
                    ParticipationActivityGroupType = poActivityGroupType
                };

                //Get things for Students
                foreach (var student in students)
                {
                    var isGradeRestricted = participationObject.RestrictByGrade() && !participationObject.IsStudentGradeInRange(student);
                    var isAgeRestricted = !participationObject.RestrictByGrade() && !participationObject.IsStudentAgeInRange(student);
                    var isGenderRestricted = participationObject.IsGenderEnforced
                                           && !participationObject.AcceptsGenderType(student.GenderId);
                    var canStillRegister = false;
                    if (participationObject.CanRegisterPerClass)
                    {
                        canStillRegister = !student.IsReservedForAllDates(participationObject, selectedDates, null);
                    }
                    var hasSubmittedForActivity = student.HasSubmittedFor(participationObject, null);
                    var hasWithDrawnFromActivity = student.HasWithdrawn(participationObject);
                    var hasCancelledSubscription = student.HasCancelledSubscription(participationObject);
                    var hasConsentFor = student.HasConsentFor(participationObject);
                    var isOnWaitingListFor = student.OnWaitingListFor(participationObject);
                    var isReservedAndPaidFor = student.IsReservedAndPaidFor(participationObject, null);
                    var isAppliedFor = student.IsAppliedFor(participationObject, null);
                    var masterRegistrantHasPlan = student.MasterRegistrantHasPlan(participationObject);
                    var makePaymentAcc = student.GetRegistrationWithParticipationObject(participationObject)?.Uid;
                    var isProfileCompleted = student.IsProfileCompleted;
                    var ineligibleText = ParticipationObjectUI.PrintIneligibilityMessageString(participationObject,
                    isGenderRestricted, isAgeRestricted, isGradeRestricted);
                    var registrantGradeText = student.GetAnswer(QuestionDataType.RegistrantGrade)?.Text;
                    var grade = UILabel.GetGradeDescription(registrantGradeText);

                    var newStudent = new SelectRegistrantsDto
                    {
                        ID = student.ID,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        IsParentOrGuardian = student.IsParentOrGuardian,
                        IsAdult = student.IsAdult,
                        Uid = student.Uid,
                        IsGradeRestricted = isGradeRestricted,
                        IsAgeRestricted = isAgeRestricted,
                        IsGenderRestricted = isGenderRestricted,
                        CanStillRegister = canStillRegister,
                        HasSubmittedForActivity = hasSubmittedForActivity,
                        HasWithDrawnFromActivity = hasWithDrawnFromActivity,
                        HasCancelledSubscription = hasCancelledSubscription,
                        HasConsentFor = hasConsentFor,
                        IsOnWaitingListFor = isOnWaitingListFor,
                        IsReservedAndPaidFor = isReservedAndPaidFor,
                        IsAppliedFor = isAppliedFor,
                        MasterRegistrantHasPlan = masterRegistrantHasPlan,
                        MakePaymentAcc = makePaymentAcc,
                        IsProfileCompleted = isProfileCompleted,
                        IneligibleText = ineligibleText,
                        Grade = grade

                    };
                    registrantsDtos.Add(newStudent);
                }
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
                    Students = registrantsDtos,
                    ParticipationObject = resParticipationObject,
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
    
    //public ActionResult GetById(int currentAccId, int activityId)
    //{
    //    var msgs = new SystemMessages();

    //    if (currentAccId.IsEmpty())
    //        throw new Exception("Web user id required.");

    //    if (activityId.IsEmpty())
    //        throw new Exception("Activity id is required");

    //    try
    //    {
    //        var parent = WebUser.Load(currentAccId);
    //        var activity = Activity.Load(activityId);
    //        if (parent == null)
    //            throw new Exception("No parent was found");

    //        if (activity == null)
    //            throw new Exception("No activity was found");

    //        var participationObject = new ParticipationObject(activity);

    //        var students = parent.GetStudentRegistrants(msgs);

    //        var registrantsDtos = new List<SelectRegistrantsDto>();

    //        var selectedDates = RegistrationCommon.GetSelectedDatesFromCart();

    //        //Get things for ParticipationObject
    //        var poRegistrationMode = participationObject.RegistrationMode;
    //        var poSeatsRemaining = participationObject.SeatsRemaining;
    //        var poSubscriptionPlans = participationObject.SubscriptionPlans.Any();
    //        var poSubscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault();
    //        var poParticipationObject = participationObject.IsSession();
    //        var poActivityGroupType = participationObject.ActivityGroupType;

    //        var resParticipationObject = new SRParticipationObjectDTO
    //        {
    //            ParticipationRegistrationMode = poRegistrationMode,
    //            SeatsRemaining = poSeatsRemaining,
    //            AnySubscriptionPlans = poSubscriptionPlans,
    //            FirstSubscriptionPlan = poSubscriptionPlan,
    //            ParticipationObjectIsSession = poParticipationObject,
    //            ParticipationActivityGroupType = poActivityGroupType
    //        };

    //        //Get things for Students
    //        foreach (var student in students)
    //        {
    //            var isGradeRestricted = participationObject.RestrictByGrade() && !participationObject.IsStudentGradeInRange(student);
    //            var isAgeRestricted = !participationObject.RestrictByGrade() && !participationObject.IsStudentAgeInRange(student);
    //            var isGenderRestricted = participationObject.IsGenderEnforced
    //                                   && !participationObject.AcceptsGenderType(student.GenderId);
    //            var canStillRegister = false;
    //            if (participationObject.CanRegisterPerClass)
    //            {
    //                canStillRegister = !student.IsReservedForAllDates(participationObject, selectedDates, null);
    //            }
    //            var hasSubmittedForActivity = student.HasSubmittedFor(participationObject, null);
    //            var hasWithDrawnFromActivity = student.HasWithdrawn(participationObject);
    //            var hasCancelledSubscription = student.HasCancelledSubscription(participationObject);
    //            var hasConsentFor = student.HasConsentFor(participationObject);
    //            var isOnWaitingListFor = student.OnWaitingListFor(participationObject);
    //            var isReservedAndPaidFor = student.IsReservedAndPaidFor(participationObject, null);
    //            var isAppliedFor = student.IsAppliedFor(participationObject, null);
    //            var masterRegistrantHasPlan = student.MasterRegistrantHasPlan(participationObject);
    //            var makePaymentAcc = student.GetRegistrationWithParticipationObject(participationObject).Uid;
    //            var isProfileCompleted = student.IsProfileCompleted;
    //            var ineligibleText = ParticipationObjectUI.PrintIneligibilityMessageString(participationObject,
    //            isGenderRestricted, isAgeRestricted, isGradeRestricted);
    //            var registrantGradeText = student.GetAnswer(QuestionDataType.RegistrantGrade).Text;
    //            var grade = UILabel.GetGradeDescription(registrantGradeText);

    //            var newStudent = new SelectRegistrantsDto
    //            {
    //                ID = (int) student.ID,
    //                FirstName = student.FirstName,
    //                LastName = student.LastName,
    //                IsParentOrGuardian = student.IsParentOrGuardian,
    //                IsAdult = student.IsAdult,
    //                Uid = student.Uid,
    //                IsGradeRestricted = isGradeRestricted,
    //                IsAgeRestricted = isAgeRestricted,
    //                IsGenderRestricted = isGenderRestricted,
    //                CanStillRegister = canStillRegister,
    //                HasSubmittedForActivity = hasSubmittedForActivity,
    //                HasWithDrawnFromActivity = hasWithDrawnFromActivity,
    //                HasCancelledSubscription = hasCancelledSubscription,
    //                HasConsentFor = hasConsentFor,
    //                IsOnWaitingListFor = isOnWaitingListFor,
    //                IsReservedAndPaidFor = isReservedAndPaidFor,
    //                IsAppliedFor = isAppliedFor,
    //                MasterRegistrantHasPlan = masterRegistrantHasPlan,
    //                MakePaymentAcc = makePaymentAcc,
    //                IsProfileCompleted = isProfileCompleted,
    //                IneligibleText = ineligibleText,
    //                Grade = grade

    //            };
    //            registrantsDtos.Add(newStudent);
    //        }
    //        var Response = new
    //        {
    //            Students = registrantsDtos,
    //            ParticipationObject = resParticipationObject,

    //        };

    //        return Json(
    //            new ApiResponse(msgs, Response),
    //            JsonRequestBehavior.AllowGet);

    //    }
    //    catch (Exception ex)
    //    {
    //        msgs.Add(ex.Message, MessageType.ApiError);

    //        return Json(
    //            new ApiResponse(msgs, null),
    //            JsonRequestBehavior.AllowGet);

    //    }


    //}
}