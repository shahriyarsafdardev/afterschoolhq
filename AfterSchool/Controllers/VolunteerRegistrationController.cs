using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AfterSchool.Assemblers;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Hangfire;
using LibraryUtilities = AfterSchoolLibrary.Helpers.Utilities;
using Utilities = BasicLibrary.Utilities;
using LibraryModels = AfterSchoolLibrary.Models;

namespace AfterSchool.Controllers
{
    public class VolunteerRegistrationController : BaseController
    {
        private readonly IBackgroundCheckWorkflow backgroundCheckWorkflow;
        private readonly IFeatureFlagRepository featureFlagRepository;

        public VolunteerRegistrationController(IFeatureFlagRepository featureFlagRepository, IBackgroundCheckWorkflow backgroundCheckWorkflow)
        {
            this.featureFlagRepository = featureFlagRepository;
            this.backgroundCheckWorkflow = backgroundCheckWorkflow;
        }

        [HttpGet]
        public ActionResult GetStarted(string regaccount)
        {
            var msgs = new SystemMessages();

            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

            if (currentUser.IsNotEmpty() && currentAccount.IsNotEmpty())
            {
                var registrantAccount = currentAccount;

                if (currentAccount.AccountTypeId != AccountType.Volunteer)
                {
                    //check if the user has a parent account
                    registrantAccount = currentUser.GetVolunteerAccount(msgs);

                    // if no parent account, then create one real quick
                    if (registrantAccount.IsEmpty())
                        registrantAccount = currentUser.AddNewVolunteerAccount(UserType.Owner, true, currentUser,participationObject, msgs);
                }

                var masterRegistrant = currentUser.GetMasterVolunteerRegistrant(msgs);

                if (masterRegistrant.IsNotEmpty())
                    registrantAccount = masterRegistrant.GetAccount(msgs);

                if (registrantAccount.ID != currentAccount.ID)
                    LoadSessionInfo(
                        currentUser,
                        registrantAccount,
                        false,
                        msgs);

                var hasCompletedVolunteerProfile = masterRegistrant.IsNotEmpty() && masterRegistrant.ProfileStatusId == RegistrantProfileStatus.Completed &&
                                                   masterRegistrant.PersonTypeId == RegistrantPersonType.Volunteer;

                if (hasCompletedVolunteerProfile)
                    return RedirectToAction("Register");
            }

            Account registrationAccount = null;

            if (regaccount.IsGuid())
                registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            var noActivityIsSelected = participationObject.IsEmpty() || !participationObject.ObjectID.HasValue;
            if (noActivityIsSelected)
            {
                if (SessionVariables.RegistrationStartPageAccount.IsNotEmpty() || registrationAccount.IsNotEmpty())
                    return Redirect(SessionVariables.RegistrationStartPageAccount?.HomePageUrl ?? registrationAccount?.HomePageUrl);

                return HttpNotFound();
            }

            var questions = RegistrationCommon.GetAllDefaultVolunteerPersonalInformationQuestions(msgs);

            var getStartedAssembler = new GetStartedAssembler(featureFlagRepository);

            // NOTE: Should this be updated for volunteers?
            var result = getStartedAssembler.AssembleGetStartedResult(msgs, participationObject, registrationAccount,
                currentUser, questions, false, Request.Params);
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            return View(result);
        }

        [HttpPost]
        public ActionResult GetStarted(string regaccount, string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var currentUser = SessionVariables.CurrentUser;

            var allQuestionsForQuestionGroup = RegistrationCommon.GetAllDefaultVolunteerPersonalInformationQuestions(msgs);
            var getStartedAssembler = new GetStartedAssembler(featureFlagRepository);
            var qvms = getStartedAssembler.FilterAndTransformQuestionsForVolunteerRegistration(currentUser, allQuestionsForQuestionGroup, Request.Params);

            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            qvms.ForEach(qvm =>
            {
                if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.Trim().IsRegExMatch(Utilities.PhoneRegEx))
                            msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                    case QuestionDataType.RegistrantGender:
                        if (participationObject.IsGenderEnforced)
                            if (!participationObject.AcceptsGenderType(FormService.GetGenderFromAnswer(qvm.Value)))
                                msgs.Add($"The selected opportunity is for {ParticipationObjectUI.PrintGenderRestrictions(participationObject).ToLower()}",
                                    MessageType.UserError);

                        break;
                }
            });

            if (currentUser.IsEmpty())
            {
                if (password1.IsEmpty())
                    msgs.Add("Password 1 is required", MessageType.UserError);
                if (password1.Length < 8)
                    msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
                if (password2.IsEmpty())
                    msgs.Add("Password 2 is required", MessageType.UserError);
                if (password1 != password2)
                    msgs.Add("Passwords must match", MessageType.UserError);
                if (LibraryUtilities.IsProduction)
                    msgs.AddRange(RecaptchaHelper.Validate(Request["g-recaptcha-response"]));
            }

            if (msgs.HasNoErrors())
            {
                var firstName = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                var phone = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

                WebUser user = null;
                Account account = null;
                Registrant registrant = null;

                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        user = RegistrationCommon.CreateOrUpdateUser(
                            currentUser,
                            firstName.Value.Trim(),
                            lastName.Value.Trim(),
                            email.Value.Trim(),
                            phone.Value.Trim(),
                            password1,
                            textnotifications.IfBlank("0").ToBoolean(),
                            amsgs,
                            adc);


                        if (amsgs.HasNoErrors() && user.IsNotEmpty())
                        {
                            account = RegistrationCommon.GetOrCreateVolunteerAccount(user, participationObject ,amsgs, adc);
                            // LOG: Someone Created Account AccountName
                            SessionVariables.WebSession.LogActivity(
                                account,
                                currentUser,
                                WebSessionActionType.Created,
                                ObjectType.Account,
                                objectId: account.ID.Value,
                                objectText: account.Name
                            );

                            // LOG: Someone Created User UserName
                            SessionVariables.WebSession.LogActivity(
                                account,
                                currentUser,
                                WebSessionActionType.SignedUp,
                                ObjectType.WebUser,
                                objectId: user.ID.Value,
                                objectText: user.FullName
                            );
                        }

                        if (amsgs.HasNoErrors())
                            registrant = RegistrationCommon.GetOrCreateVolunteerRegistrant(account, user, amsgs, adc);

                        if (amsgs.HasNoErrors())
                            RegistrationCommon.AddOrUpdateStandardAnswers(qvms, registrant, user, amsgs, adc);
                    },
                    smsgs =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account);
                    },
                    failureMessages => msgs.AddRange(failureMessages),
                    LibraryUtilities.SystemErrorLog
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("AddHomeAddress");

            var registrationAccount = regaccount.IsGuid() ? Account.LoadByUid(regaccount.ToGuid(), msgs) : null;
            var questions = RegistrationCommon.GetAllDefaultVolunteerPersonalInformationQuestions(msgs);
            var result = getStartedAssembler.AssembleGetStartedResult(msgs, participationObject, registrationAccount,
                SessionVariables.CurrentUser, questions, false, Request.Params);
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            return View(result);
        }

        [HttpGet]
        public ActionResult Select()
        {
            return View();
        }

        public ActionResult CheckRedirect(bool checkForEmptyCart = false)
        {
            if (SessionVariables.RegistrationStartPageAccount.IsNotEmpty())
            {
                var redirectUrl = SessionVariables.RegistrationStartPageAccount.HomePageUrl;

                if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                    return Redirect(redirectUrl);

                if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Volunteer)
                    return Redirect(redirectUrl);

                if (checkForEmptyCart && SessionVariables.Cart.IsEmpty())
                    return Redirect(redirectUrl);
            }
            else
            {
                if (SessionVariables.CurrentUser.IsEmpty())
                    return Redirect(LibraryUtilities.LoginUrl());

                if (SessionVariables.CurrentAccount.IsEmpty())
                    return RedirectToAction("select", "login");

                if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Volunteer)
                    return HandleRedirectAfterLogin(null);

                if (checkForEmptyCart && SessionVariables.Cart.IsEmpty())
                    return HandleRedirectAfterLogin(null);
            }

            return null;
        }

        [HttpGet]
        public ActionResult ThankYou(string id)
        {
            if (!RegistrationCommon.IsValidLogin(AccountType.Volunteer))
                return Logout(Request.Url.ToString(), true);

            var msgs = new SystemMessages();

            RegistrationCommon.ResetRegistrationSession();

            var registration = Registration.LoadByUid(id.ToGuid(), msgs);
            var (participationObject, assignmentType) = RegistrationCommon.GetParticipationObjectFromRegistration(registration, msgs);
            var account = SessionVariables.RegistrationStartPageAccount ?? participationObject.OwnerAccount;
            var currentUser = SessionVariables.CurrentUser;
            var registrant = registration.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var isSelfRegistered = masterRegistrant.WebUserId == currentUser.ID;
            var result = new ThankYouResultAssembler().AssembleThankYouResult(participationObject, account, currentUser, registrant, isSelfRegistered,
                assignmentType, msgs);
            return View(result);
        }

        private List<FormQuestion> GatherAddHomeAddressQuestions(SystemMessages msgs, ParticipationObject participationObject)
        {
            var form = FormService.GetAdministratorAccountVolunteerForm(msgs);
            var questions = FormQuestion.LoadByFormIdByQuestionDataTypeIds(form.ID.Value, new[]
            {
                QuestionDataType.RegistrantAddress1,
                QuestionDataType.RegistrantAddress2,
                QuestionDataType.RegistrantCity,
                QuestionDataType.RegistrantState,
                QuestionDataType.RegistrantPostalCode,
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantWorkPhone,
                QuestionDataType.RegistrantEmployer,
            });

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantAddress1:
                    case QuestionDataType.RegistrantCity:
                    case QuestionDataType.RegistrantState:
                    case QuestionDataType.RegistrantPostalCode:
                        question.IsRequired = true;
                        break;
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            return questions.OrderBy(question => question.QuestionDataTypeId).ToList();
        }

        [HttpGet]
        public ActionResult AddHomeAddress()
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return results;

            var result = GatherDataForAndAssembleAddHomeAddressResult();

            return View(result);
        }

        private AddHomeAddressResult GatherDataForAndAssembleAddHomeAddressResult()
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;
            var questions = GatherAddHomeAddressQuestions(msgs, participationObject);
            var formQuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();

            var result = new AddHomeAddressAssembler().AssembleAddHomeAddressResult(formQuestionViewModels, participationObject, registrationStartPageAccount,
                msgs);
            return result;
        }

        [HttpPost]
        public ActionResult AddHomeAddress(string id)
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var questions = GatherAddHomeAddressQuestions(msgs, participationObject);
            var volunteerRegistrant = SessionVariables.CurrentAccount.GetVolunteerRegistrant();

            questions.ForEach(question =>
            {
                if (question.IsRequired && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                        FormService.PhoneValidation(Request[question.ToFormQuestionViewModel().Name], question, msgs);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            volunteerRegistrant.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);
                        });
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => LibraryUtilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                return RedirectToAction("addEmergencyContacts");

            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = participationObject;
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;
            var result = new AddHomeAddressAssembler().AssembleAddHomeAddressResult(qvms, participationObject, registrationStartPageAccount, msgs);

            return View(result);
        }

        [HttpGet]
        public ActionResult AddEmergencyContacts()
        {
            var results = CheckRedirect();
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var questions = FormQuestionService.GetVolunteerEmergencyContactQuestions(msgs);
            var formQuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();

            var result = GatherDataForAndAssembleAddEmergencyContactResult(formQuestionViewModels, msgs);
            return View(result);
        }

        private static AddEmergencyContactResult GatherDataForAndAssembleAddEmergencyContactResult(List<FormQuestionViewModel> formQuestionViewModels,
            SystemMessages msgs)
        {
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;

            var result = new AddEmergencyContactAssembler().AssembleAddEmergencyContactResult(formQuestionViewModels, participationObject,
                registrationStartPageAccount, msgs);
            return result;
        }

        [HttpPost]
        public ActionResult AddEmergencyContacts(string id)
        {
            var msgs = new SystemMessages();
            var questions = FormQuestionService.GetVolunteerEmergencyContactQuestions(msgs);
            var volunteerRegistrant = SessionVariables.CurrentAccount.GetVolunteerRegistrant();
            var currentUser = SessionVariables.CurrentUser;

            // validate emergency contact fields
            FormQuestionService.ValidateFormQuestionForEmergencyContacts(
                questions.Select(question => question.ToFormQuestionViewModelForService(Request.Params))
                    .ToList(), msgs);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        // mark parent profile as completed since they've went through all the steps
                        volunteerRegistrant.SetProfileAsCompleted(currentUser, amsgs, adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            volunteerRegistrant.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                currentUser,
                                amsgs,
                                adc);
                        });
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => LibraryUtilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                return RedirectToAction("Register");

            var result = GatherDataForAndAssembleAddEmergencyContactResult(qvms, msgs);
            return View(result);
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (!RegistrationCommon.IsValidLogin(AccountType.Volunteer))
                return Logout(Request.Url.ToString(), true);

            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var currentUser = SessionVariables.CurrentUser;
            var currentRegistrant = currentUser.GetMasterVolunteerRegistrant();
            SessionVariables.CurrentRegistrant = currentRegistrant;
            var (_, registration) = currentRegistrant.GetSubmittedRecord(participationObject, msgs);

            // NOTE: Should be removed for now, since there is no volunteer account portal to actually edit the profile
            // var parentOrGuardian = currentRegistrant.GetParentOrGuardian(msgs);
            // if (!parentOrGuardian.IsProfileCompleted && parentOrGuardian.GetOwner(msgs).ID == currentUser.ID)
            //     return RedirectToAction("editpersonal", "family", new
            //     {
            //         Area = "user", id = parentOrGuardian.Uid, reg = registration?.Uid,
            //         student = currentRegistrant?.Uid, returnUrl = Url.Action("select", "account"),
            //     });

            var questions = RegistrationCommon.GetQuestionsRequiredForActivityThatHaveNotYetBeenCollected(participationObject, currentRegistrant, msgs);

            var isReadyToFinalizeRegistration = !questions.Any() && registration.IsNotEmpty() && msgs.HasNoErrors();
            if (isReadyToFinalizeRegistration)
            {
                // NOTE: If already submitted, then there is nothing else to select, should be kicked totally out
                if (CurrentRegistrantHasAlreadySubmitted(registration.CompletionStatusId))
                    return new RedirectResult(SessionVariables.RegistrationStartPageAccount.HomePageUrl);

                if (msgs.HasNoErrors())
                    ProcessRegistration(currentRegistrant, participationObject, registration, msgs);

                if (msgs.HasNoErrors())
                    return RedirectToAction("ThankYou", new {id = registration.Uid});
            }

            return AssembleResultAndGoToRegisterView(participationObject, msgs, questions, currentRegistrant);
        }

        private ActionResult AssembleResultAndGoToRegisterView(ParticipationObject participationObject, SystemMessages msgs,
            List<FormQuestionViewModel> questions, Registrant currentRegistrant)
        {
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;
            var result = new VolunteerRegisterAssembler().AssembleRegisterResult(questions, participationObject,
                registrationStartPageAccount, currentRegistrant, msgs);
            return View("Register", result);
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

        private static bool CurrentRegistrantHasAlreadySubmitted(RegistrationCompletionStatus registrationCompletionStatusId)
        {
            return registrationCompletionStatusId == RegistrationCompletionStatus.Completed;
        }

        private static List<DateTime> GatherAvailableSelectedDates(Registrant currentRegistrant, ParticipationObject participationObject)
        {
            return RegistrationCommon.GetSelectedDatesFromCart()?.Where(date =>
                !currentRegistrant.IsReservedForDate(participationObject, date, null)
            ).ToList();
        }

        [HttpPost]
        public async Task<ActionResult> Register(string signature)
        {
            if (!RegistrationCommon.IsValidLogin(AccountType.Volunteer))
                return Logout(Request.Url.ToString(), true);

            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegistrationCommon.GetActiveCustomQuestionsThatHaveNotYetBeenAnswered(participationObject, msgs);
            RegistrationCommon.ValidateRegisterStudentQuestions(questions, msgs, Request);

            if (signature.IsEmpty())
                msgs.Add("E-signature is required", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();
            var registration = RegistrationCommon.CreateRegistration(participationObject, qvms, signature, Request, msgs);
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            var currentUser = SessionVariables.CurrentUser;

            if (msgs.HasNoErrors() && registration != null)
            {
                if (CurrentRegistrantHasAlreadySubmitted(registration.CompletionStatusId))
                    return RedirectToAction("Select");

                if (msgs.HasNoErrors())
                {
                    ProcessRegistration(currentRegistrant, participationObject, registration, msgs);
                }

                if (msgs.HasNoErrors())
                {
                    await InitateBackgroundCheck(currentUser, participationObject, msgs);
                }

                if (msgs.HasNoErrors())
                    return RedirectToAction("ThankYou", new {id = registration.Uid});
            }

            return AssembleResultAndGoToRegisterView(participationObject, msgs, qvms, currentRegistrant);
        }

        /// <summary>
        ///     Form to request a background check. To access this, you must have the "background-check" feature flag enabled and go through the registration process for a
        ///     volunteer opportunity. When you get to the Register action, manually change the URL to take you to the BackgroundCheck action.
        /// </summary>
        /*[HttpGet]
        public ActionResult BackgroundCheck()
        {
            if (!RegistrationCommon.IsValidLogin(AccountType.Volunteer) || !featureFlagRepository.IsEnabled("background-check"))
                return Logout(Request.Url!.ToString(), true);

            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var currentUser = SessionVariables.CurrentUser;
            var currentRegistrant = currentUser.GetMasterVolunteerRegistrant();
            SessionVariables.CurrentRegistrant = currentRegistrant;

            return AssembleResultAndGoToBackgroundCheckView(participationObject, msgs, currentUser.ID!.Value);
        }

        [HttpPost]
        public async Task<ActionResult> BackgroundCheck(string signature)
        {
            if (!RegistrationCommon.IsValidLogin(AccountType.Volunteer) || !featureFlagRepository.IsEnabled("background-check"))
                return Logout(Request.Url!.ToString(), true);

            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

            var currentUser = SessionVariables.CurrentUser;

            var _ = await CheckIfBackgroundCheckIsEnabledAndRequestUrl(msgs, currentUser, participationObject.OwnerAccount.ID!.Value);

            return AssembleResultAndGoToBackgroundCheckView(participationObject, msgs, currentUser.ID!.Value);
        }

        private async Task<string> CheckIfBackgroundCheckIsEnabledAndRequestUrl(SystemMessages msgs, WebUser webUser, int accountId)
        {
            if (msgs.HasErrors() || !featureFlagRepository.IsEnabled("background-check")) return null;
            var backgroundCheckResult = await backgroundCheckWorkflow.RequestBackgroundCheck(webUser, accountId, msgs);
            return backgroundCheckResult.BackgroundCheckUrl;
        }

        private ActionResult AssembleResultAndGoToBackgroundCheckView(ParticipationObject participationObject, SystemMessages msgs, int webUserId)
        {
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;
            var backgroundCheck = AfterSchoolLibrary.Models.BackgroundCheck.LoadByWebUserId(webUserId, msgs).FirstOrDefault();
            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.RegistrationStartPageAccount = registrationStartPageAccount;
            return View("BackgroundCheck", backgroundCheck);
        }*/

        private async Task<SystemMessages> InitateBackgroundCheck(WebUser webUser,ParticipationObject participationObject,SystemMessages msgs)
        {
            var backgroundCheck = LibraryModels.BackgroundCheck.LoadByWebUserId((int) webUser.ID, msgs).FirstOrDefault();
            var fiscalAccountId = AccountRelationship.LoadAccountByVisitorAccountId((int) participationObject.OwnerAccount.ID)?.HostAccountId;
            var accountId = fiscalAccountId.IsNotEmpty() ? fiscalAccountId : SessionVariables.CurrentAccount.ID;
            if (backgroundCheck.IsEmpty())
            {
                if (msgs.HasErrors() || !featureFlagRepository.IsEnabled("background-check")) return null;
                var backgroundCheckResult = await backgroundCheckWorkflow.RequestBackgroundCheck(webUser, (int)accountId, msgs);

                if (msgs.HasNoErrors())
                {
                    SendLinkToVolunteer(backgroundCheckResult.BackgroundCheckUrl, webUser, msgs);
                }
            }

            return msgs;
        }
        
        private void SendLinkToVolunteer(string link, WebUser webUser, SystemMessages msgs)
        {
            EmailHelper.InitiateBackgroundCheckMail(webUser, link, msgs);
        }
        
    }
}