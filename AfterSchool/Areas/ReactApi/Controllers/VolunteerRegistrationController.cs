using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Areas.ReactApi.Models;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.ReactApi.Controllers
{
    public class VolunteerRegistrationController : BaseController
    {
        [HttpGet]
        public ActionResult GetForm(string accountName, string activityName, string registrantUid)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                var account = Account.LoadByUniqueName(accountName, msgs);
                var activity = Activity.LoadByUniqueNameByAccountId(activityName, account.ID.Value, msgs);
                var currentRegistrant = Registrant.LoadByUid(registrantUid.ToGuid(), msgs);
                
                Registration registration = null;
                RegistrantActivity registrantActivity = null;

                var participationObject = activity.GetParticipationObject(msgs);
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
                            DisplayOrder = questionGroup.DisplayOrder,
                            QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                        }
                    );
                }
                
                foreach (var questionAnswer in FormQuestionAnswer.LoadByRegistrantId(currentRegistrant.ID.Value))
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

                if (currentRegistrant.ProfileStatusId == RegistrantProfileStatus.Completed)
                    registration = currentRegistrant.GetRegistrations(msgs)?.LastOrDefault();

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    activityDto = new
                    {
                        Name = participationObject.Name,
                        Organization = participationObject.OwnerAccount.Name,
                        RegistrantType = currentRegistrant.PersonTypeId.ToString(),
                        FindAnotherActivityUrl = participationObject.OwnerAccount.ShortHomePageUrl,
                        ActivityUrl = participationObject.GetShortHomePageUrl(),
                        RegisterForNewChildUrl = participationObject.GetRegistrationPageUrl(),
                        IsGradeRestricted = participationObject.RestrictByGrade(),
                        SchoolLocation = currentRegistrant.GetSchoolLocation(msgs),
                        IsAgeRestricted = (participationObject.AgeStart != null ? true : false),
                        RegistrationIsCompleted = currentRegistrant.ProfileStatusId == RegistrantProfileStatus.Completed ? true : false,
                        CanAccess = currentUser.HasAccessToRegistrant(currentRegistrant, msgs),
                        RegistrationUid = registration?.Uid,
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
        
        public ActionResult AddUpdateAnswers(QuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();

            try
            {
                var currentUser = SessionVariables.CurrentUser;

                var account = Account.LoadByUniqueName(json.accountName, msgs);
                var activity = Activity.LoadByUniqueNameByAccountId(json.activityName, account.ID.Value, msgs);
                var currentRegistrant = Registrant.LoadByUid(json.registrantUid.ToGuid(), msgs);

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");

                var participationObject = activity.GetParticipationObject(msgs);
                var questions = participationObject.Form.GetAllQuestions(msgs);
                var collection = HttpUtility.ParseQueryString(json.answers);

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
                
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (json.groupid.ToInteger() == (int) QuestionGroupDataType.PersonalInformation)
                        {
                            currentRegistrant.UpdateProfileStatus(RegistrantProfileStatus.Started, currentUser, amsgs, adc);
                        }
                        
                        qvms.ForEach(qvm =>
                        {
                            if (qvm.Question.QuestionDataTypeId != QuestionDataType.NotSet)
                            {
                                currentRegistrant.AddOrUpdateAnswer(
                                    qvm.Value.IfEmpty("").Trim(),
                                    qvm.Question,
                                    currentUser,
                                    amsgs,
                                    adc);
                            }
                        });
                        
                        RegistrationCommon.AddCustomQuestions(
                            qvms,
                            currentRegistrant,
                            null,
                            currentUser,
                            Request,
                            amsgs, adc);

                        if (json.groupid.ToInteger() == -1)
                        {
                            currentRegistrant.SetProfileAsCompleted(currentUser, amsgs, adc);
                        }
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
        
        public ActionResult Register(QuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                var account = Account.LoadByUniqueName(json.accountName, msgs);
                var activity = Activity.LoadByUniqueNameByAccountId(json.activityName, account.ID.Value, msgs);
                var currentRegistrant = Registrant.LoadByUid(json.registrantUid.ToGuid(), msgs);

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");

                if (json.esignature.IsEmpty())
                    msgs.Add("E-signature is required", MessageType.UserError);                

                var participationObject = activity.GetParticipationObject(msgs);
                var questions = participationObject.Form.GetAllQuestions(msgs);
                NameValueCollection collection = HttpUtility.ParseQueryString(json.answers);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                //var registration = RegistrationCommon.CreateRegistration(participationObject, qvms, json.esignature, Request, msgs);
                
                SessionVariables.CurrentRegistrant = currentRegistrant;
                var (_, registration) = currentRegistrant.GetSubmittedRecord(participationObject, msgs);

                if (msgs.HasNoErrors())
                    ProcessRegistration(currentRegistrant, participationObject, registration, msgs);
                
                if (msgs.HasNoErrors())
                    currentRegistrant.SetProfileAsCompleted(currentUser, msgs);
                
                return Json(
                    new ApiResponse(msgs,registration.Uid),
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
        
        private static void ProcessRegistration(Registrant currentRegistrant, ParticipationObject participationObject, Registration registration,
            SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var selectedDates = GatherAvailableSelectedDates(currentRegistrant, participationObject);
            AccountService.ProcessRegistrationWithoutPayment(
                new CheckoutObject
                {
                    BillingAccount = participationObject.OwnerAccount,
                    PayingAccount = currentAccount,
                    NumberOfSeats = 1,
                    ParticipationObject = participationObject,
                    Registration = registration,
                    MasterRegistrant = currentRegistrant,
                    SubscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault(),
                    SelectedDates = selectedDates,
                },
                true, SessionVariables.CurrentUser, msgs);
        }
        
        private static List<DateTime> GatherAvailableSelectedDates(Registrant currentRegistrant, ParticipationObject participationObject)
        {
            return RegistrationCommon.GetSelectedDatesFromCart()?.Where(date =>
                !currentRegistrant.IsReservedForDate(participationObject, date, null)
            ).ToList();
        }
    }
}