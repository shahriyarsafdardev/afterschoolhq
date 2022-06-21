using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public AccountController(IFeatureFlagRepository featureFlagRepository)
        {
            this.featureFlagRepository = featureFlagRepository;
        }


        #region Contact

        private List<FormQuestionViewModel> ContactViewBag(Account account, SystemMessages msgs)
        {
            var contactForm = account.GetContactForm(msgs);
            var questions = contactForm.GetActiveQuestions(msgs);
            var qvms = questions.Select(question => new FormQuestionViewModel(question, Request.Params)).ToList();

            ViewBag.AccountProfile = account.GetAccountProfile(msgs);
            ViewBag.ContactForm = contactForm;
            ViewBag.Qvms = qvms;
            ViewBag.RegistrationAccount = account;

            return qvms;
        }

        [HttpGet]
        public ActionResult Contact(string regaccount)
        {
            if (!regaccount.IsGuid())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            if (registrationAccount.IsEmpty())
                return HttpNotFound();

            if (!registrationAccount.ContactFormId.HasValue)
                return Redirect(registrationAccount.HomePageUrl);

            var qvms = ContactViewBag(registrationAccount, msgs);

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult Contact(string regaccount, string agree)
        {
            if (!regaccount.IsGuid())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            if (registrationAccount.IsEmpty())
                return HttpNotFound();

            if (!registrationAccount.ContactFormId.HasValue)
                return Redirect(registrationAccount.HomePageUrl);

            var qvms = ContactViewBag(registrationAccount, msgs);

            qvms.ForEach(qvm =>
            {
                if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                if (qvm.Question.QuestionDataTypeId.ToString().ToLower().Contains("phone") && qvm.Value.IsNotEmpty() && !qvm.Value.IsPhone())
                    msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
            });

            if (msgs.HasNoErrors())
            {
                var firstName = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                Registrant submittedContact = null;

                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        submittedContact = registrationAccount.AddContact(firstName.Value, lastName.Value, email.Value, null, true, null, amsgs, adc);

                        if (amsgs.HasNoErrors()) RegistrationCommon.AddOrUpdateStandardAnswers(qvms, submittedContact, null, amsgs, adc);

                        if (amsgs.HasNoErrors()) AddCustomQuestions(qvms, submittedContact, null, null, amsgs, adc);
                    },
                    smsgs =>
                    {
                        msgs.AddRange(smsgs);
                        EmailHelper.SendContactFormConfirmation(registrationAccount, submittedContact, msgs);
                    },
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("contactthankyou", new {id = registrationAccount.Uid});

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        public ActionResult ContactThankYou(string id)
        {
            if (!id.IsGuid())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var registrationAccount = Account.LoadByUid(id.ToGuid(), msgs);

            if (registrationAccount.IsEmpty())
                return HttpNotFound();

            if (!registrationAccount.ContactFormId.HasValue)
                return Redirect(registrationAccount.HomePageUrl);

            ViewBag.Account = registrationAccount;
            ViewBag.AccountProfile = registrationAccount?.GetAccountProfile(msgs);
            ViewBag.Messages = msgs;

            return View();
        }

        #endregion

        #region GetStartedParent

        private ActionResult HandleGetStartedAccountRedirect(bool isInterestForm, SystemMessages msgs)
        {
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            if (currentUser.IsNotEmpty() && currentAccount.IsNotEmpty())
            {
                var registrantAccount = currentAccount;

                if (currentAccount.AccountTypeId != AccountType.Registrant)
                {
                    //check if the user has a parent account
                    registrantAccount = currentUser.GetRegistrantAccount(msgs);

                    // if no parent account, then create one real quick
                    if (registrantAccount.IsEmpty())
                        registrantAccount =
                            currentUser.AddNewRegistrantAccount(UserType.Owner, true, true, currentUser, msgs);
                }

                var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant(msgs);

                if (masterRegistrant.IsNotEmpty())
                    registrantAccount = masterRegistrant.GetAccount(msgs);

                if (registrantAccount.ID != currentAccount.ID)
                    LoadSessionInfo(
                        currentUser,
                        registrantAccount,
                        false,
                        msgs);

                var hasCompletedParentProfile = masterRegistrant.IsNotEmpty() && masterRegistrant.ProfileStatusId == RegistrantProfileStatus.Completed &&
                                                !isInterestForm && masterRegistrant.IsParentOrGuardian;
                if (hasCompletedParentProfile)
                    return RedirectToAction("select");
            }

            return null;
        }

        private static bool DetermineIfStudentIsAllowedToRegisterSelf(ParticipationObject participationObject,
            SystemMessages msgs)
        {
            if (participationObject.ObjectType != ParticipationObjectType.Activity) return false;

            var activity = Activity.Load(participationObject.ObjectID.Value, msgs);
            return activity?.IsStudentAllowedToRegister ?? false;
        }

        [HttpGet]
        public ActionResult GetStarted(string regaccount)
        {
            var msgs = new SystemMessages();

            // handles admin accounts that have or need parent accounts
            var redirectResult = HandleGetStartedAccountRedirect(false, msgs);
            if (redirectResult.IsNotEmpty())
                return redirectResult;

            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

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

            var questions = RegistrationCommon.GetAllDefaultPersonalInformationQuestions(msgs);

            var isAllowedToRegisterSelf = DetermineIfStudentIsAllowedToRegisterSelf(participationObject, msgs);
            var getStartedAssembler = new GetStartedAssembler(featureFlagRepository);
            var result = getStartedAssembler.AssembleGetStartedResult(msgs, participationObject, registrationAccount,
                SessionVariables.CurrentUser, questions, isAllowedToRegisterSelf, Request.Params);
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            return View(result);
        }

        public class GetStartedResult
        {
            public bool CleverSsoEnabled { get; set; }
            public List<FormQuestionViewModel> FormQuestionViewModels { get; set; }
            public SystemMessages Messages { get; set; }
            public ParticipationObject ParticipationObject { get; set; }
            public Account RegistrationAccount { get; set; }
            public bool StudentRegistrationEnabled { get; set; }
        }

        [HttpPost]
        public ActionResult GetStarted(string regaccount, string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var currentUser = SessionVariables.CurrentUser;

            var allQuestionsForQuestionGroup = RegistrationCommon.GetAllDefaultPersonalInformationQuestions(msgs);
            var getStartedAssembler = new GetStartedAssembler(featureFlagRepository);
            var qvms = getStartedAssembler.FilterAndTransformQuestionsForParentRegistration(currentUser, allQuestionsForQuestionGroup, Request.Params);

            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            // Register as an existing student
            if (currentUser.IsNotEmpty())
            {
                var studentRegistrant = currentUser.GetMasterRegistrant(msgs);
                if (msgs.HasNoErrors() && !studentRegistrant.IsParentOrGuardian)
                {
                    SessionVariables.CurrentRegistrant = studentRegistrant;

                    return RedirectToAction("RegisterStudent");
                }
            }


            qvms.ForEach(qvm =>
            {
                if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.Trim().IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
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
                if (Utilities.IsProduction)
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
                            account = RegistrationCommon.CreateOrUpdateAccount(user, amsgs, adc);

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

                        if (amsgs.HasNoErrors()) registrant = RegistrationCommon.CreateOrUpdateRegistrant(account, user, amsgs, adc);

                        if (amsgs.HasNoErrors()) RegistrationCommon.AddOrUpdateStandardAnswers(qvms, registrant, user, amsgs, adc);
                    },
                    smsgs =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account);
                    },
                    failureMessages => msgs.AddRange(failureMessages),
                    exceptionMessages => Utilities.SystemErrorLog(exceptionMessages)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addhomeaddress");

            Account registrationAccount = null;

            if (regaccount.IsGuid())
                registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);
            var questions = RegistrationCommon.GetAllDefaultPersonalInformationQuestions(msgs);
            var isAllowedToRegisterSelf = DetermineIfStudentIsAllowedToRegisterSelf(participationObject, msgs);
            var result = getStartedAssembler.AssembleGetStartedResult(msgs, participationObject, registrationAccount,
                SessionVariables.CurrentUser, questions, isAllowedToRegisterSelf, Request.Params);
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            return View(result);
        }

        #endregion

        #region AddHomeAddress

        private List<FormQuestion> GatherAddHomeAddressQuestions(SystemMessages msgs, ParticipationObject participationObject)
        {
            var form = FormService.GetAdministratorAccountForm(msgs);
            var homeAddressQuestionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress);
            var questions = homeAddressQuestionGroup.GetAllQuestions(msgs);

            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questionsWithPersonalInfo = personalContactGroup.GetAllQuestions(msgs);
            var allQuestionsWithPersonalInfo = questionsWithPersonalInfo.Where(question =>
                    question.QuestionDataTypeId.In(new List<QuestionDataType>
                    {
                        QuestionDataType.RegistrantHomePhone,
                        QuestionDataType.RegistrantWorkPhone,
                        QuestionDataType.RegistrantParentRelationship,
                        QuestionDataType.RegistrantEmployer,
                    }))
                .ToList();

            questions.AddRange(allQuestionsWithPersonalInfo);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantAddress2:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantEmployer:
                        question.IsRequired = false;
                        break;
                    default:
                        question.IsRequired = true;
                        break;
                }
            });

            return questions;
        }

        [HttpGet]
        public ActionResult AddHomeAddress()
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var questions = GatherAddHomeAddressQuestions(msgs, participationObject);
            var formQuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();

            var result = GatherDataForAndAssembleAddHomeAddressResult(formQuestionViewModels, participationObject, msgs);

            return View(result);
        }

        private AddHomeAddressResult GatherDataForAndAssembleAddHomeAddressResult(List<FormQuestionViewModel> formQuestionViewModels,
            ParticipationObject participationObject, SystemMessages msgs)
        {
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;

            return new AddHomeAddressAssembler().AssembleAddHomeAddressResult(formQuestionViewModels, participationObject, registrationStartPageAccount,
                msgs);
        }

        [HttpPost]
        public ActionResult AddHomeAddress(string id)
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var questions = GatherAddHomeAddressQuestions(msgs, participationObject);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();

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
                            parentRegistrant.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);
                        });
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                return RedirectToAction("addparent");

            var result = GatherDataForAndAssembleAddHomeAddressResult(qvms, participationObject, msgs);

            return View(result);
        }

        #endregion

        #region AddParent

        private List<FormQuestion> AddParentViewBag(SystemMessages msgs)
        {
            var form = FormService.GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                question.QuestionDataTypeId.NotIn(FormConstants.QuestionsToNotIncludeInAdditionalParentPanel())).ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    // case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                    case QuestionDataType.RegistrantParentRelationship:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.ParticipationObject = RegistrationCommon.GetParticipationObjectFromCart();

            return questions;
        }

        [HttpGet]
        public ActionResult AddParent()
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var questions = AddParentViewBag(msgs);
            var qvms = questions.Select(question => question.ToFormQuestionViewModel()).ToList();

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult AddParent(string id, string createuseraccount)
        {
            var msgs = new SystemMessages();
            var questions = AddParentViewBag(msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.Trim().IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Registrant otherParent = null;
                WebUser otherParentUser = null;

                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        var canBeAccessedBy = SessionVariables.CurrentUser.AsList();

                        // create student account if requestioned by parent
                        if (createuseraccount.IsNumber())
                        {
                            var firstName = qvms.Find(qvm =>
                                qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            var lastName = qvms.Find(qvm =>
                                qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
                            var email = qvms.Find(qvm =>
                                qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value;
                            var phone = qvms.Find(qvm =>
                                qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value;

                            otherParentUser = SessionVariables.CurrentAccount.InviteUser(
                                firstName,
                                lastName,
                                email,
                                phone.Trim(),
                                null,
                                UserType.Student,
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);

                            canBeAccessedBy.Add(otherParentUser);
                        }

                        // create parent registrant record
                        otherParent = SessionVariables.CurrentAccount.CreateRegistrant(
                            otherParentUser,
                            canBeAccessedBy,
                            RegistrantType.Individual,
                            true,
                            true,
                            false,
                            null,
                            RegistrantProfileStatus.Started,
                            SessionVariables.CurrentUser,
                            amsgs,
                            adc);

                        qvms.ForEach(qvm =>
                        {
                            otherParent.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);
                        });
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addemergencycontacts");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddEmergencyContacts

        [HttpGet]
        public ActionResult AddEmergencyContacts()
        {
            var results = CheckRedirect();
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs);
            var formQuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();

            var result = GatherDataForAndAssembleAddEmergencyContactResult(formQuestionViewModels, msgs);
            return View(result);
        }

        private static AddEmergencyContactResult GatherDataForAndAssembleAddEmergencyContactResult(List<FormQuestionViewModel> formQuestionViewModels,
            SystemMessages msgs)
        {
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;

            var result = new AddEmergencyContactAssembler().AssembleAddEmergencyContactResult(formQuestionViewModels, participationObject, registrationStartPageAccount, msgs);
            return result;
        }

        [HttpPost]
        public ActionResult AddEmergencyContacts(string id)
        {
            var msgs = new SystemMessages();
            var questions = FormQuestionService.GetEmergencyContactQuestions(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();

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
                        parentRegistrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);
                        });
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                return RedirectToAction("addstudent");

            var result = GatherDataForAndAssembleAddEmergencyContactResult(qvms, msgs);
            return View(result);
        }

        #endregion

        #region AddStudent

        private List<FormQuestion> AddStudentViewBag(Form administratorAccountForm, SystemMessages msgs)
        {
            var studentQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = studentQuestionGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantWorkPhone &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEmployer &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantHomePhone &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantParentRelationship
                )
                .ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantDateOfBirth:
                    case QuestionDataType.RegistrantGender:
                        question.IsRequired = true;
                        break;
                    case QuestionDataType.RegistrantTShirtSize:
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantEmail:
                        question.IsRequired = false;
                        break;
                }
            });

            var schoolLocation = new Location();

            if (SessionVariables.RegistrationStartPageAccount.AccountTypeId == AccountType.School)
                schoolLocation = SessionVariables.RegistrationStartPageAccount.GetDefaultLocation(msgs);

            ViewBag.School = schoolLocation;
            ViewBag.ParticipationObject = RegistrationCommon.GetParticipationObjectFromCart();
            ViewBag.ShowStepNumber = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs).Count == 0;

            return questions;
        }

        [HttpGet]
        public ActionResult AddStudent()
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var adminAccountForm = FormService.GetAdministratorAccountForm(msgs);
            var parentOrGuardian = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var questions = AddStudentViewBag(adminAccountForm, msgs);
            var values = new NameValueCollection();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                    case QuestionDataType.RegistrantHomePhone:
                        values.Add(question.ToFormQuestionViewModel().Id,
                            parentOrGuardian.GetAnswer(question.QuestionDataTypeId)?.Text);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult AddStudent(string id, string samehomeaddress, string createuseraccount,
            string locationname = null, string streetnumber = null, string streetname = null, string address2 = null,
            string neighborhood = null,
            string city = null, string county = null, string state = null, string postalcode = null,
            string country = null, string placeid = null, decimal? latitude = null, decimal? longitude = null,
            bool schoolnotfound = false)
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var adminAccountForm = FormService.GetAdministratorAccountForm(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant(msgs);
            var parentRegistrant2 = SessionVariables.CurrentAccount.GetSecondParentOrGuardianRegistrant(msgs);
            var questions = AddStudentViewBag(adminAccountForm, msgs);
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        FormService.PhoneValidation(answer, question, msgs);
                        break;
                }
            });

            if (placeid.IsEmpty() && !schoolnotfound) msgs.Add("Location is required", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();
            var email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value;

            if (createuseraccount.IsNumber() && !email.IsEmail())
                msgs.Add("Email is required and must be valid in order to allow your child to login",
                    MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Registrant student = null;
                WebUser studentUser = null;

                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        var canBeAccessedBy = currentUser.AsList();
                        Location school = null;

                        if (placeid.IsNotEmpty())
                            school = LocationService.CreateOrUpdateFromGoogle(locationname, streetnumber, streetname,
                                address2, neighborhood, city, county, state, postalcode, country, placeid, latitude,
                                longitude, SessionVariables.CurrentUser, amsgs, adc);

                        // create student account if requested by parent
                        if (createuseraccount.IsNumber())
                        {
                            var firstName = qvms.Find(qvm =>
                                qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            var lastName = qvms.Find(qvm =>
                                qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;

                            studentUser = currentAccount.CreateAndInviteUser(
                                firstName,
                                lastName,
                                email,
                                null,
                                $"Your parent/guardian ({currentUser.DisplayName}) has invited you to sign up for {Utilities.AppName()} to register for activities",
                                UserType.Student,
                                currentUser,
                                amsgs,
                                adc);

                            canBeAccessedBy.Add(studentUser);
                        }

                        // create student registrant record
                        student = SessionVariables.CurrentAccount.CreateRegistrant(
                            studentUser,
                            canBeAccessedBy,
                            RegistrantType.Individual,
                            true,
                            false,
                            false,
                            school.IsNotEmpty() ? school.ID : new int?(),
                            RegistrantProfileStatus.Started,
                            currentUser,
                            amsgs,
                            adc);

                        // LOG: Someone Created Registrant RegistrantName for Account AccountName
                        if (student.IsNotEmpty() && amsgs.HasNoErrors())
                            SessionVariables.WebSession.LogActivity(
                                currentAccount,
                                currentUser,
                                WebSessionActionType.Created,
                                ObjectType.Registrant,
                                objectId: student.ID.Value,
                                objectText: student.FullName,
                                indirectObjectType: ObjectType.Account,
                                indirectObjectId: currentAccount.ID.Value,
                                indirectObjectText: currentAccount.Name
                            );

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            student.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);
                        });

                        // copy parent answers to student for home address, emergency contacts
                        parentRegistrant.GetAnswers(amsgs, adc)
                            .Where(answer =>
                                answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress ||
                                answer.FormQuestionGroupQuestionGroupDataTypeId.In(FormQuestionService
                                    .GetEmergencyContactQuestionGroups())
                            )
                            .ToList()
                            .ForEach(answer =>
                            {
                                student.AddOrUpdateAnswer(
                                    answer.Text.IfEmpty("").Trim(),
                                    answer.ToFormQuestion(msgs: amsgs, dc: adc),
                                    SessionVariables.CurrentUser,
                                    amsgs,
                                    adc);
                            });

                        // copy parent info over to student parent info
                        var allquestions = adminAccountForm.GetAllQuestions(amsgs, adc);
                        var parent1Map = new Dictionary<QuestionDataType, QuestionDataType>
                        {
                            {QuestionDataType.RegistrantFirstName, QuestionDataType.Parent1FirstName},
                            {QuestionDataType.RegistrantMiddleName, QuestionDataType.Parent1MiddleName},
                            {QuestionDataType.RegistrantLastName, QuestionDataType.Parent1LastName},
                            {QuestionDataType.RegistrantEmail, QuestionDataType.Parent1Email},
                            {QuestionDataType.RegistrantHomePhone, QuestionDataType.Parent1HomePhone},
                            {QuestionDataType.RegistrantMobilePhone, QuestionDataType.Parent1MobilePhone},
                            {QuestionDataType.RegistrantWorkPhone, QuestionDataType.Parent1WorkPhone},
                            {QuestionDataType.RegistrantEmployer, QuestionDataType.Parent1Employer},
                            {
                                QuestionDataType.RegistrantParentRelationship,
                                QuestionDataType.Parent1RelationshipToRegistrant
                            },
                        };

                        foreach (var key in parent1Map.Keys)
                            student.MapAndAddAnswer(
                                allquestions,
                                parentRegistrant,
                                key,
                                parent1Map[key],
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);

                        var parent1AddressMap = new Dictionary<QuestionDataType, QuestionDataType>
                        {
                            {QuestionDataType.RegistrantAddress1, QuestionDataType.Parent1Address1},
                            {QuestionDataType.RegistrantAddress2, QuestionDataType.Parent1Address2},
                            {QuestionDataType.RegistrantCity, QuestionDataType.Parent1City},
                            {QuestionDataType.RegistrantState, QuestionDataType.Parent1State},
                            {QuestionDataType.RegistrantPostalCode, QuestionDataType.Parent1PostalCode},
                        };

                        foreach (var key in parent1AddressMap.Keys)
                            student.MapAndAddAnswer(
                                allquestions,
                                parentRegistrant,
                                key,
                                parent1AddressMap[key],
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);

                        if (parentRegistrant2.IsNotEmpty())
                        {
                            // get parent 2 info
                            var parent2Map = new Dictionary<QuestionDataType, QuestionDataType>
                            {
                                {QuestionDataType.RegistrantFirstName, QuestionDataType.Parent2FirstName},
                                {QuestionDataType.RegistrantMiddleName, QuestionDataType.Parent2MiddleName},
                                {QuestionDataType.RegistrantLastName, QuestionDataType.Parent2LastName},
                                {QuestionDataType.RegistrantEmail, QuestionDataType.Parent2Email},
                                {QuestionDataType.RegistrantHomePhone, QuestionDataType.Parent2HomePhone},
                                {QuestionDataType.RegistrantMobilePhone, QuestionDataType.Parent2MobilePhone},
                                {QuestionDataType.RegistrantWorkPhone, QuestionDataType.Parent2WorkPhone},
                                {QuestionDataType.RegistrantEmployer, QuestionDataType.Parent2Employer},
                                {
                                    QuestionDataType.RegistrantParentRelationship,
                                    QuestionDataType.Parent2RelationshipToRegistrant
                                },
                            };

                            foreach (var key in parent2Map.Keys)
                                student.MapAndAddAnswer(
                                    allquestions,
                                    parentRegistrant2,
                                    key,
                                    parent2Map[key],
                                    SessionVariables.CurrentUser,
                                    amsgs,
                                    adc);
                        }

                        // TODO: maybe should get sibling and copy health info
                    },
                    smsgs =>
                    {
                        msgs.AddRange(smsgs);

                        SessionVariables.CurrentRegistrant = student;
                    },
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addhealth");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region StudentRegistration

        private List<FormQuestion> StudentRegistrationViewBag(SystemMessages msgs)
        {
            var form = FormService.GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);
            questions = questions.Where(question =>
                    question.QuestionDataTypeId.NotIn(FormConstants.QuestionsToNotIncludeInStudentRegistrationPanel()))
                .ToList();
            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantDateOfBirth:
                    case QuestionDataType.RegistrantGender:
                        question.IsRequired = true;
                        break;
                }
            });
            return questions;
        }

        private List<FormQuestion> StudentRegistrationParentViewBag(SystemMessages msgs)
        {
            var form = FormService.GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);
            questions = questions.Where(question =>
                    question.QuestionDataTypeId.NotIn(
                        FormConstants.QuestionsToNotIncludeInStudentRegistrationParentPanel()))
                .ToList();
            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        question.IsRequired = true;
                        question.Text = "Parent's " + question.Text;
                        break;
                }
            });
            ViewBag.ParentQuestions =
                questions.Select(parentQuestion => new FormQuestionViewModel(parentQuestion)).ToList();
            return questions;
        }

        [HttpGet]
        public ActionResult StudentRegistration(string regaccount)
        {
            var msgs = new SystemMessages();

            // handles admin accounts that have or need parent accounts
            var redirectResult = HandleGetStartedAccountRedirect(false, msgs);
            if (redirectResult.IsNotEmpty())
                return redirectResult;

            if (SessionVariables.CurrentUser.IsNotEmpty() &&
                SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            Account registrationAccount = null;
            if (regaccount.IsGuid())
                registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            // no activity is selected
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            if (participationObject.IsEmpty())
            {
                if (SessionVariables.RegistrationStartPageAccount.IsNotEmpty() || registrationAccount.IsNotEmpty())
                    return Redirect(SessionVariables.RegistrationStartPageAccount?.HomePageUrl ??
                                    registrationAccount?.HomePageUrl);

                return HttpNotFound();
            }

            var questions = StudentRegistrationViewBag(msgs);
            ViewBag.ParentQuestions = StudentRegistrationParentViewBag(msgs)
                .Select(question => new FormQuestionViewModel(question)).ToList();
            ViewBag.RegistrationAccount = registrationAccount;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");
            ViewBag.Messages = msgs;

            var qvm = questions.Select(question => new FormQuestionViewModel(question)).ToList();
            return View(qvm);
        }

        [HttpPost]
        public ActionResult StudentRegistration(string regaccount, string password1, string password2, string agree,
            string textnotifications, string isStudent)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var questions = StudentRegistrationViewBag(msgs);
            var parentQuestions = StudentRegistrationParentViewBag(msgs);

            if (currentUser.IsEmpty())
                ValidateStudentRegistrationPasswords(password1, password2, msgs);

            ValidateStudentRegistrationQuestions(questions, participationObject, msgs);
            ValidateStudentConfirmation(agree, isStudent, msgs);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();
            if (msgs.HasNoErrors())
            {
                var firstName =
                    qvms?.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName)?.Value
                        ?.Split(',')?[0];
                var lastName =
                    qvms?.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName)?.Value
                        .Split(',')?[0];
                var email = qvms?.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail)
                    ?.Value.Split(',')?[0];
                var phone = qvms
                    ?.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone)?.Value;
                var dob = qvms?.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantDateOfBirth)
                    ?.Value;
                var parentFirstName =
                    qvms?.Last(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName)?.Value
                        .Split(',')?[1];
                var parentLastName =
                    qvms?.Last(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName)?.Value
                        .Split(',')?[1];
                var parentEmail = qvms?.Last(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail)
                    ?.Value.Split(',')?[1];
                var location = participationObject?.Location;
                if (location.IsNotEmpty() && location.GetHostAccount(msgs)?.AccountTypeId != AccountType.School)
                    location = null;

                ValidateEmailsForStudentRegistration(email, parentEmail, msgs);
                if (msgs.HasNoErrors())
                {
                    var questionAnswerModel = GetQuestionAnswerModel(qvms);
                    var parentWebUser = WebUser.LoadByEmail(parentEmail, msgs);
                    var parentAccount = parentWebUser?.GetAccounts().FirstOrDefault();
                    var studentDetails = AccountService.ProcessStudentRegistration(
                        firstName,
                        lastName,
                        email,
                        phone,
                        password1,
                        location?.ID.Value, // probably should have a school location id
                        textnotifications.IfBlank("0").ToBoolean(),
                        SessionVariables.IsAfterSchool.Value,
                        questionAnswerModel,
                        parentAccount,
                        msgs);

                    if (msgs.HasNoErrors())
                    {
                        var studentAccount = studentDetails.Item1;
                        var studentUser = studentDetails.Item2;
                        var studentRegistrant = studentDetails.Item3;
                        var questionAnswerParentModel = GetQuestionAnswerModel(qvms, true);
                        if (studentAccount.IsNotEmpty() && studentUser.IsNotEmpty() && studentRegistrant.IsNotEmpty() &&
                            parentAccount.IsEmpty())
                        {
                            var parentDetails = studentAccount.ProcessParentRegistration(
                                parentFirstName,
                                parentLastName,
                                parentEmail,
                                participationObject?.GetBrowseUrl(),
                                textnotifications.IfBlank("").ToBoolean(),
                                questionAnswerParentModel,
                                studentUser,
                                msgs);

                            if (msgs.HasNoErrors())
                            {
                                var parentUser = parentDetails.Item1;

                                studentRegistrant.AddUserWithAccess(
                                    parentUser,
                                    studentUser,
                                    msgs);
                            }

                            SessionVariables.CurrentUser = studentUser;
                            SessionVariables.CurrentAccount = studentAccount;
                            SessionVariables.AccountUserType = studentUser.GetUserType(studentAccount, msgs);
                            SessionVariables.CurrentRegistrant = studentRegistrant;
                        }
                        else
                        {
                            // in case of existing parent, we need to set session for parent account data
                            if (msgs.HasNoErrors()) studentRegistrant.AddUserWithAccess(parentWebUser, studentUser, msgs);

                            if (msgs.HasNoErrors())
                                msgs.AddRange(EmailHelper.SendParentStudentAccountEmail(parentWebUser.FirstName,
                                    studentUser.FirstName, Utilities.LoginUrl(), parentWebUser.Email,
                                    participationObject?.GetBrowseUrl()));

                            SessionVariables.CurrentUser = studentUser;
                            SessionVariables.CurrentAccount = parentAccount;
                            SessionVariables.AccountUserType = parentWebUser.GetUserType(parentAccount, msgs);
                            SessionVariables.CurrentRegistrant = studentRegistrant;
                            SessionVariables.ParentAccount = parentAccount;
                        }

                        SessionVariables.OriginalLoggedInUser = studentUser;
                    }
                }
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("RegisterStudent");

            if (regaccount.IsGuid())
                ViewBag.RegistrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");
            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.ParentQuestions = GetParentQuestionAnswerViewModel(qvms, ViewBag.ParentQuestions);
            qvms = GetQuestionAnswerViewModel(qvms);

            return View(qvms);
        }

        private void ValidateStudentRegistrationQuestions(List<FormQuestion> questions,
            ParticipationObject participationObject, SystemMessages msgs)
        {
            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                if (question.QuestionDataTypeId == QuestionDataType.RegistrantDateOfBirth && answer.IsNotEmpty() &&
                    !participationObject.IsAgeInRange(answer.ToDateTime()))
                    msgs.Add($"Age must be between {participationObject.AgeStart} and {participationObject.AgeEnd}.",
                        MessageType.UserError);

                if (question.QuestionDataTypeId == QuestionDataType.RegistrantGrade && answer.IsNotEmpty() &&
                    !participationObject.IsGradeInRange(answer))
                    msgs.Add(
                        $"Grade must be between {participationObject.GetMinGradeLevel(msgs)} and {participationObject.GetMaxGradeLevel(msgs)}.",
                        MessageType.UserError);

                if (question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone && answer.IsNotEmpty())
                {
                    if (!answer.Trim().IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                        msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);

                    if (WebUser.ExistsByPhone(answer, msgs))
                        msgs.Add("Phone is already in use, do you already have an account?", MessageType.UserError);
                }
            });
        }

        private void ValidateStudentRegistrationPasswords(string password, string confirmPassword, SystemMessages msgs)
        {
            if (password.IsEmpty())
                msgs.Add("Password is required", MessageType.UserError);
            if (password.Length < 8)
                msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
            if (confirmPassword.IsEmpty())
                msgs.Add("Confirm Password is required", MessageType.UserError);
            if (password != confirmPassword)
                msgs.Add("Confirm Password must match with main Password", MessageType.UserError);
        }

        private void ValidateStudentConfirmation(string agree, string isStudent, SystemMessages msgs)
        {
            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);
            if (!isStudent.IsBoolean())
                msgs.Add("You must be student in order to register", MessageType.UserError);
        }

        private void ValidateEmailsForStudentRegistration(string email, string parentEmail, SystemMessages msgs)
        {
            if (WebUser.ExistsByEmail(email, msgs))
                msgs.Add("Student email already exists.", MessageType.UserError);
            if (email.IsSameAs(parentEmail))
                msgs.Add("Parent email cannot be the same as your student email address when creating a login account",
                    MessageType.UserError);
        }

        private List<KeyValuePair<FormQuestion, string>> GetQuestionAnswerModel(List<FormQuestionViewModel> qvms,
            bool isParent = false)
        {
            //prepare a model of question and answers that we can pass to service layer
            var model = new List<KeyValuePair<FormQuestion, string>>();
            foreach (var qvm in qvms)
                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        if (!isParent)
                            model.Add(new KeyValuePair<FormQuestion, string>(qvm.Question,
                                qvm.Value?.Split(',')?[0].Trim()));
                        else
                            model.Add(new KeyValuePair<FormQuestion, string>(qvm.Question,
                                qvm.Value?.Split(',')?[1].Trim()));
                        break;
                    default:
                        if (!isParent)
                            model.Add(
                                new KeyValuePair<FormQuestion, string>(qvm.Question, qvm.Value.IfEmpty("").Trim()));
                        break;
                }

            return model;
        }

        private List<FormQuestionViewModel> GetQuestionAnswerViewModel(List<FormQuestionViewModel> qvms,
            bool isParent = false)
        {
            foreach (var qvm in qvms)
                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        qvm.Value = !isParent ? qvm.Value?.Split(',')?[0].Trim() : qvm.Value?.Split(',')?[1].Trim();
                        break;
                }

            return qvms;
        }

        private List<FormQuestionViewModel> GetParentQuestionAnswerViewModel(List<FormQuestionViewModel> qvms,
            List<FormQuestionViewModel> parentQuestions)
        {
            var parentFirstName =
                qvms?.Last(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName)?.Value
                    .Split(',')?[1];
            var parentLastName =
                qvms?.Last(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName)?.Value
                    .Split(',')?[1];
            var parentEmail = qvms?.Last(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail)
                ?.Value.Split(',')?[1];
            for (var i = 0; i < parentQuestions.Count; i++)
                switch (parentQuestions[i].Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                        parentQuestions[i].Value = parentFirstName;
                        break;
                    case QuestionDataType.RegistrantLastName:
                        parentQuestions[i].Value = parentLastName;
                        break;
                    case QuestionDataType.RegistrantEmail:
                        parentQuestions[i].Value = parentEmail;
                        break;
                }

            return parentQuestions;
        }

        #endregion

        #region AddHealth

        private List<FormQuestion> AddHealthViewBag(SystemMessages msgs)
        {
            var form = FormService.GetAdministratorAccountForm(msgs);
            var healthQuestionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HealthInformation);
            var questions = healthQuestionGroup.GetAllQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            if (RegistrationCommon.GetParticipationObjectFromCart().IsEmpty())
                msgs.Add("Activity not found", MessageType.Warning);
            else
                ViewBag.ParticipationObject = RegistrationCommon.GetParticipationObjectFromCart();

            ViewBag.ShowStepNumber = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs).Count < 2;

            return questions;
        }

        [HttpGet]
        public ActionResult AddHealth()
        {
            if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
            var questions = AddHealthViewBag(msgs);

            ViewBag.Student = SessionVariables.CurrentRegistrant;
            ViewBag.Messages = msgs;

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult AddHealth(string id)
        {
            if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
            var questions = AddHealthViewBag(msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantDoctorPhone:
                        if (answer.IsNotEmpty() && !answer.Trim().IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();
            var registrant = SessionVariables.CurrentRegistrant;
            var currentUser = SessionVariables.CurrentUser;

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        // mark student profile as completed since they've went through all the steps
                        registrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            registrant.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                currentUser,
                                amsgs,
                                adc);
                        });
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                return RedirectToAction("register");

            ViewBag.Student = SessionVariables.CurrentRegistrant;
            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region SelectStudent

        [HttpGet]
        public ActionResult Select()
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();

            // Case 3 - New Student Register with Parent Exists
            if (SessionVariables.AccountUserType == UserType.Student)
                return RedirectToAction("Home", "User");

            var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant();

            if (masterRegistrant.IsEmpty())
                return RedirectToAction("getstarted");

            if (masterRegistrant.ProfileStatusId == RegistrantProfileStatus.Started)
                return RedirectToAction("getstarted");

            if (!masterRegistrant.IsParentOrGuardian)
            {
                // Case 2 - new Student Register with New Parent Email
                if (masterRegistrant.ProfileStatusId == RegistrantProfileStatus.Completed)
                    return RedirectToAction("Home", "User");

                // Case 1 - Parent Registration flow
                SessionVariables.CurrentRegistrant = masterRegistrant;
                return RedirectToAction("registerstudent");
            }

            var students = SessionVariables.CurrentUser.GetChildrenRegistrants(msgs);

            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = RegistrationCommon.GetParticipationObjectFromCart();
            ViewBag.SelectedDates = GetSelectedDatesFromCart();

            return View(students);
        }

        private ActionResult SetStudentAction(string id)
        {
            if (id.IsGuid())
            {
                SessionVariables.CurrentRegistrant = Registrant.LoadByUid(id.ToGuid());
                return RedirectToAction("register", "account");
            }

            return RedirectToAction("select", "account");
        }

        [HttpGet]
        public ActionResult SetStudent(string id)
        {
            return HandleIsLoggedIn() ?? SetStudentAction(id);
        }

        #endregion

        #region Register

        private List<FormQuestion> RegisterViewBag(ParticipationObject participationObject, SystemMessages msgs)
        {
            var allQuestions = participationObject.Form.GetActiveQuestions(msgs).ToList();

            var requiredQuestions = allQuestions
                .Where(question => question.IsRequired &&
                                   question.FormQuestionGroupRegistrantTypeId == RegistrantType.Individual &&
                                   question.QuestionDataTypeId != QuestionDataType.NotSet)
                .ToList();

            var answers = SessionVariables.CurrentRegistrant.GetAnswers(msgs);

            var questions = requiredQuestions.Where(question =>
                    answers.FirstOrDefault(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId)
                        .IsEmpty() ||
                    answers.FirstOrDefault(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).Text
                        .IsBlank()
                )
                .Distinct()
                .ToList();

            questions.AddRange(allQuestions
                .Where(question => !question.IsDeleted && question.QuestionDataTypeId == QuestionDataType.NotSet)
                .Where(question => answers.Find(answer => answer.FormQuestionId == question.ID).IsEmpty())
                .OrderBy(question => question.DisplayOrder)
            );

            ViewBag.Registrant = SessionVariables.CurrentRegistrant;
            ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();

            return questions;
        }

        private ActionResult RegisterActionGet()
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var selectedDates = GetSelectedDatesFromCart();
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            var parentOrGuardian = currentRegistrant.GetParentOrGuardian(msgs);
            var (submittedRecord, registration) = currentRegistrant.GetSubmittedRecord(participationObject, msgs);

            if (!parentOrGuardian.IsProfileCompleted &&
                parentOrGuardian.GetOwner(msgs).ID == SessionVariables.CurrentUser.ID)
                return RedirectToAction("editpersonal", "family",
                    new
                    {
                        Area = "user", id = parentOrGuardian.Uid, reg = registration?.Uid,
                        student = currentRegistrant?.Uid, returnUrl = Url.Action("select", "account"),
                    });

            if (!currentRegistrant.IsProfileCompleted)
                return RedirectToAction("editstudentpersonal", "family",
                    new
                    {
                        Area = "user", id = currentRegistrant.Uid, reg = registration?.Uid,
                        student = currentRegistrant?.Uid, returnUrl = Url.Action("select", "account"),
                    });

            // check grade eligibility (extra check)
            if (!participationObject.IsStudentGradeInRange(currentRegistrant, msgs))
                return RedirectToAction("select");

            // current registrant is already registered
            var needsToRegister =
                submittedRecord.NeedsToRegister(registration, participationObject, selectedDates, msgs);
            if (!needsToRegister && !participationObject.IsSession())
                return RedirectToAction("select");

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegisterViewBag(participationObject, msgs);

            // if there are no additional questions to answer, 
            // if it doesn't require parental consent, 
            // the registration has already been started (by the child)
            // and it's not a session (parents can edit registrations for sessions)
            var gotoPayment = !questions.Any() && !participationObject.RequiresParentalConsent &&
                              registration.IsNotEmpty() && !participationObject.IsSession();

            if (gotoPayment)
            {
                if (participationObject.ShouldParentPay())
                    return RedirectToAction("payment", new {id = registration.Uid});

                return RedirectToAction("finalizeregistration", new {id = registration.Uid});
            }

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);
            ViewBag.Messages = msgs;

            return View(participationObject);
        }

        [HttpGet]
        public ActionResult Register()
        {
            return HandleIsLoggedIn() ?? RegisterActionGet();
        }

        private ActionResult RegisterStudentByParent(string signature)
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegisterStudentViewBag(participationObject, msgs);
            ValidateRegisterStudentQuestions(questions, msgs);

            if (signature.IsEmpty())
                msgs.Add("E-signature is required", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();
            var registration = CreateRegistration(participationObject, qvms, signature, msgs);

            if (msgs.HasNoErrors())
            {
                if (!participationObject.ShouldParentPay())
                    return RedirectToAction("finalizeregistration", new {id = registration.Uid});

                return RedirectToAction("payment", new {id = registration.Uid});
            }

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);
            ViewBag.Messages = msgs;

            return View(participationObject);
        }

        [HttpPost]
        public ActionResult Register(string signature)
        {
            return HandleIsLoggedIn() ?? RegisterStudentByParent(signature);
        }

        private ActionResult FinalizeRegistrationGet(string registrationUid)
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

            // if you can't pay later
            if (!participationObject.AllowToPayLater && participationObject.ShouldParentPay())
                return RedirectToAction("payment", new {id = registrationUid});

            // need to actually register first :)
            if (!registrationUid.IsGuid())
                return RedirectToAction("register");

            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);

            // current registrant has already submitted, then select a different student
            if (registration.CompletionStatusId == RegistrationCompletionStatus.Completed)
                return RedirectToAction("select");

            var currentRegistrant = SessionVariables.CurrentRegistrant;
            var currentAccount = SessionVariables.CurrentAccount;
            var selectedDates = GetSelectedDatesFromCart();
            var datesToRemove = new List<DateTime>();

            selectedDates?.ForEach(date =>
            {
                if (currentRegistrant.IsReservedForDate(participationObject, date, null))
                    datesToRemove.Add(date);
            });

            datesToRemove.ForEach(date => selectedDates.Remove(date));

            if (msgs.HasNoErrors())
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
                    true,
                    SessionVariables.CurrentUser,
                    msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("thankyou", new {id = registration.Uid});

            return View();
        }

        [HttpGet]
        public ActionResult FinalizeRegistration(string id)
        {
            return HandleIsLoggedIn() ?? FinalizeRegistrationGet(id);
        }

        [HttpGet]
        public ActionResult ParentalConsent(string id)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            SessionVariables.CurrentRegistrant = registration.GetMasterRegistrant(msgs);
            SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);
            SessionVariables.Cart.AddCartItem(registration.GetFirstParticipationObject(msgs), null, msgs);
            SessionVariables.RegistrationStartPageAccount = registration.GetAccount(msgs);

            return RedirectToAction("Register");
        }

        #endregion

        #region Payment

        private Registration PaymentViewBag(string id, ParticipationObject participationObject, SystemMessages msgs)
        {
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);
            var registrant = registration.GetRegistrant(msgs);
            var registrantActivity = registrant.GetRegistrantActivity(participationObject, msgs);

            if (registrantActivity.IsNotEmpty() && registrantActivity.AmountCharged.HasValue &&
                participationObject.Cost > registrantActivity.AmountCharged)
                participationObject.Cost = registrantActivity.AmountCharged;

            ViewBag.Registration = registration;
            ViewBag.Sources = SessionVariables.CurrentAccount.GetStripeSources(msgs);
            ViewBag.BankAccounts = SessionVariables.CurrentAccount.GetStripeBankAccounts(msgs);
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.Messages = msgs;
            ViewBag.AccountCanAcceptPayments = participationObject.PaymentAccount.CanProcessPayments(msgs);
            ViewBag.SelectedDates = GetSelectedDatesFromCart();

            return registration;
        }

        private ActionResult PaymentActionGet(string id)
        {
            var results = CheckRedirect(true);
            if (results.IsNotEmpty()) return Redirect(results);

            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var selectedDates = GetSelectedDatesFromCart();
            var currentRegistrant = SessionVariables.CurrentRegistrant;

            PaymentViewBag(id, participationObject, msgs);

            if (participationObject.CanRegisterPerClass)
            {
                var registeredForAllClasses =
                    currentRegistrant.IsReservedForAllDates(participationObject, selectedDates, null, msgs);

                if (registeredForAllClasses)
                    return RedirectToAction("select");
            }
            else
            {
                var masterRegistrantHasPlan = currentRegistrant.MasterRegistrantHasPlan(participationObject, msgs);
                var isReservedAndPaidFor = currentRegistrant.IsReservedAndPaidFor(participationObject, true, msgs);

                // current registrant is already registered or already using a payment plan
                if (isReservedAndPaidFor || masterRegistrantHasPlan)
                    return RedirectToAction("select");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Payment(string id)
        {
            return HandleIsLoggedIn() ?? PaymentActionGet(id);
        }

        private ActionResult PaymentActionPost(string id, string stripetoken, string plaidtoken, string plaidaccountid,
            string ccSourceId, string bankSourceId, string savepaymentinfo, string paymentoption, string couponCode)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var selectedDates = GetSelectedDatesFromCart();
            var coupon = new Coupon();
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            CheckoutObject checkoutObject = null;

            if (couponCode.IsNotEmpty())
                coupon = SessionVariables.CurrentAccount.ValidateCoupon(participationObject, couponCode, msgs);

            if (ccSourceId.IsEmpty() && stripetoken.IsEmpty() && plaidtoken.IsEmpty() && bankSourceId.IsEmpty() &&
                paymentoption.IsNotEmpty() && paymentoption.ToInteger() >= 0)
                msgs.Add("Please enter your payment information", MessageType.UserError);

            var registration = PaymentViewBag(id, participationObject, msgs);
            var hasWithdrawn = currentRegistrant.HasWithdrawn(participationObject, msgs);

            if (registration.IsAlreadyPaidFor(msgs) && !hasWithdrawn)
                return RedirectToAction("thankyou", new {id = registration.Uid});

            if (msgs.HasNoErrors())
            {
                SubscriptionPlan paymentPlan = null;

                var billingAccount = participationObject.PaymentAccount;
                var payingAccount = SessionVariables.CurrentAccount;
                var convenienceFeePayer =
                    billingAccount.IsConvenienceFeeTakenOutOfCost ? billingAccount : payingAccount;
                var creditCardFeePayer = billingAccount.IsCreditCardFeeTakenOutOfCost ? billingAccount : payingAccount;

                if (participationObject.PostPayPlans.Any())
                    convenienceFeePayer = participationObject.PostPayPlans.First().IsConvenienceFeeTakenOutOfCost
                        ? billingAccount
                        : payingAccount;

                // get original cost
                var submittedRecord = registration.GetRegistrant(msgs);
                var registrantActivity = submittedRecord.GetRegistrantActivity(participationObject, msgs);
                if (registrantActivity.IsNotEmpty() && registrantActivity.AmountCharged.HasValue &&
                    participationObject.Cost > registrantActivity.AmountCharged)
                    participationObject.Cost = registrantActivity.AmountCharged;

                BankAccountPurchaseObject bankAccountPurchaseObject = null;
                CreditCardPurchaseObject creditCardPurchaseObject = null;

                if (plaidtoken.IsNotEmpty() || bankSourceId.IsNotEmpty())
                    bankAccountPurchaseObject = new BankAccountPurchaseObject
                    {
                        PlaidPublicToken = plaidtoken,
                        PlaidAccountId = plaidaccountid,
                        StripeSourceId = bankSourceId,
                    };
                else if (ccSourceId.IsNotEmpty() || stripetoken.IsNotEmpty())
                    creditCardPurchaseObject = new CreditCardPurchaseObject
                    {
                        StripeToken = stripetoken,
                        StripeSourceId = ccSourceId,
                    };

                if (paymentoption.IsNotEmpty() && paymentoption.IsNumber())
                    paymentPlan = SubscriptionPlan.Load(paymentoption.ToInteger(), msgs);

                var paymentType = PaymentType.NotSet;
                if (bankAccountPurchaseObject.IsNotEmpty())
                    paymentType = PaymentType.ACH;
                else if (creditCardPurchaseObject.IsNotEmpty())
                    paymentType = PaymentType.CreditCard;

                var datesToRemove = new List<DateTime>();

                selectedDates?.ForEach(date =>
                {
                    if (currentRegistrant.IsReservedForDate(participationObject, date, null))
                        datesToRemove.Add(date);
                });

                datesToRemove.ForEach(date => selectedDates.Remove(date));

                checkoutObject = AccountService.ProcessRegistrationWithPayment(
                    new CheckoutObject
                    {
                        Coupon = coupon,
                        NumberOfSeats = 1,
                        ParticipationObject = participationObject,
                        PaymentPlan = paymentPlan,
                        SubscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault(),
                        PostPayPlan = participationObject.PostPayPlans.FirstOrDefault(),
                        Products = new List<Product>(),
                        PayingAccount = payingAccount,
                        BillingAccount = billingAccount,
                        Registration = registration,
                        CreditCardPurchaseObject = creditCardPurchaseObject,
                        BankAccountPurchaseObject = bankAccountPurchaseObject,
                        SavePaymentInfo = savepaymentinfo.IfEmpty("0").ToBoolean(),
                        MasterRegistrant = currentRegistrant,
                        PaymentType = paymentType,
                        PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                        ConvenienceFeePayer = convenienceFeePayer,
                        CreditCardFeePayer = creditCardFeePayer,
                        SelectedDates = selectedDates,
                    },
                    currentUser,
                    msgs);

                if (msgs.HasNoErrors())
                {
                    var activity = participationObject.GetActivity();
                    var phrase = "";

                    if (participationObject.SubscriptionPlans.IsNotEmpty() &&
                        participationObject.SubscriptionPlans.Count > 0)
                    {
                        var plan = participationObject.SubscriptionPlans.FirstOrDefault();
                        var amount = paymentType == PaymentType.ACH
                            ? plan?.GetTotalCostWithConvenienceFee(billingAccount).ToCurrency()
                            : plan?.GetTotalCostWithFees(billingAccount).ToCurrency();
                        phrase =
                            $@"{amount}/{plan.BillingPeriodTypeId} via {(paymentType == PaymentType.ACH ? "Bank Account" : "Credit Card")}";
                    }
                    else
                    {
                        phrase =
                            $@"{participationObject.GetCheckoutCostWithConvenienceFee().ToCurrency()} via {(paymentType == PaymentType.ACH ? "Bank Account" : "Credit Card")}";
                    }

                    // LOG: Someone Paid amount/billing period via paymentType for participationObjectType participationObjectName
                    // or
                    // LOG: Someone Paid amount via PaymentType for participationObjectType participationObjectName
                    SessionVariables.WebSession.LogActivity(
                        currentAccount,
                        currentUser,
                        WebSessionActionType.Paid,
                        ObjectType.Transaction,
                        objectId: checkoutObject.Transaction?.ID.Value,
                        objectText: phrase,
                        indirectObjectType: participationObject.GetWebSessionObjectType(),
                        indirectObjectId: participationObject.ObjectID.Value,
                        indirectObjectText: activity.Name
                    );

                    return RedirectToAction("thankyou", new {id = registration.Uid});
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Payment(string id, string ccsource, string banksource, string stripetoken,
            string plaidtoken, string plaidaccountid, string savepaymentinfo, string paymentoption, string coupon)
        {
            return HandleIsLoggedIn() ?? PaymentActionPost(id, stripetoken, plaidtoken, plaidaccountid, ccsource,
                banksource, savepaymentinfo, paymentoption, coupon);
        }

        private ActionResult MakePaymentActionGet(string id)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            if (registration.IsEmpty())
                return View("");

            // check if still owes money, load up variables
            if (!registration.IsPaid(msgs))
            {
                var participationObject = registration.GetFirstParticipationObject(msgs);

                if (participationObject.IsEmpty())
                    return HttpNotFound();

                var currentRegistrant = registration.GetRegistrant(msgs).GetMasterRegistrant(msgs);
                var parentRegistrant = currentRegistrant.GetParentOrGuardian(msgs);
                var parentAccount = parentRegistrant.GetAccount(msgs);

                if (parentAccount.ID != SessionVariables.CurrentAccount.ID)
                    return Logout(Request.Url.ToString());

                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);
                SessionVariables.Cart.AddCartItem(participationObject, null, msgs);
                SessionVariables.CurrentRegistrant = currentRegistrant;
                SessionVariables.RegistrationStartPageAccount = participationObject.OwnerAccount;

                return RedirectToAction("payment", new {id = registration.Uid});
            }

            return RedirectToAction("thankyou", new {id = registration.Uid});
        }

        [HttpGet]
        public ActionResult MakePayment(string id)
        {
            return HandleIsLoggedIn() ?? MakePaymentActionGet(id);
        }

        [HttpPost]
        public ActionResult Pay(string id, string ccsource, string banksource, string stripetoken, string plaidtoken,
            string plaidaccountid, string savepaymentinfo, string paymentoption, string amount,
            string paymentdescription)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (ccsource.IsEmpty() && stripetoken.IsEmpty() && plaidtoken.IsEmpty() && banksource.IsEmpty())
                msgs.Add("Please enter your payment information", MessageType.UserError);

            var billingAccount = Account.LoadByUid(id.ToGuid(), msgs);

            if (msgs.HasNoErrors())
            {
                var payingAccount = SessionVariables.CurrentAccount;
                var convenienceFeePayer = billingAccount;
                var creditCardFeePayer = payingAccount;
                var cost = amount.ToDecimal();
                var calcObj = new FeeCalculationObject(cost, billingAccount, msgs);
                var convenienceFee = calcObj.GetConvenienceFee();
                var ccFee = 0M;
                var overrideAmount = cost;
                var description = paymentdescription.IsNotBlank()
                    ? paymentdescription
                    : $"{billingAccount.Name} Payment";

                BankAccountPurchaseObject bankAccountPurchaseObject = null;
                CreditCardPurchaseObject creditCardPurchaseObject = null;
                CheckoutObject checkoutObject = null;

                if (plaidtoken.IsNotEmpty() || banksource.IsNotEmpty())
                {
                    bankAccountPurchaseObject = new BankAccountPurchaseObject
                    {
                        PlaidPublicToken = plaidtoken,
                        PlaidAccountId = plaidaccountid,
                        StripeSourceId = banksource,
                    };
                }
                else if (ccsource.IsNotEmpty() || stripetoken.IsNotEmpty())
                {
                    creditCardPurchaseObject = new CreditCardPurchaseObject
                    {
                        StripeToken = stripetoken,
                        StripeSourceId = ccsource,
                    };

                    ccFee = calcObj.GetCreditCardFee();
                    overrideAmount += ccFee;
                }

                var masterRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant(msgs);
                checkoutObject = AccountService.ProcessPayment(
                    new CheckoutObject
                    {
                        OverrideAmount = overrideAmount,
                        ConvenienceFeeOverrideAmount = convenienceFee,
                        PaymentTypeFeeOverrideAmount = ccFee,
                        PayingAccount = payingAccount,
                        BillingAccount = billingAccount,
                        CreditCardPurchaseObject = creditCardPurchaseObject,
                        BankAccountPurchaseObject = bankAccountPurchaseObject,
                        SavePaymentInfo = savepaymentinfo.IfEmpty("0").ToBoolean(),
                        PaymentType = bankAccountPurchaseObject.IsNotEmpty() ? PaymentType.ACH : PaymentType.CreditCard,
                        PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                        ConvenienceFeePayer = convenienceFeePayer,
                        CreditCardFeePayer = creditCardFeePayer,
                        MasterRegistrant = masterRegistrant,
                        Description = description,
                    },
                    currentUser,
                    msgs: msgs);

                if (msgs.HasNoErrors())
                {
                    var phrase =
                        $@"{overrideAmount.ToCurrency()} via {(bankAccountPurchaseObject.IsNotEmpty() ? "Bank Account" : "Credit Card")}";

                    // LOG: Someone Paid amount via PaymentType to participationObjectType participationObjectName
                    SessionVariables.WebSession.LogActivity(
                        currentAccount,
                        currentUser,
                        WebSessionActionType.Paid,
                        ObjectType.Transaction,
                        objectId: checkoutObject.Transaction.ID.Value,
                        objectText: phrase,
                        indirectObjectType: ObjectType.Account,
                        indirectObjectId: billingAccount.ID.Value,
                        indirectObjectText: billingAccount.Name
                    );

                    return RedirectToAction("paymentthankyou", new {id = billingAccount.Uid});
                }
            }

            return View();
        }

        #endregion

        #region ThankYou

        private void ThankYouViewBag(string registrationUid, SystemMessages msgs)
        {
            ResetRegistrationSession();

            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
            var registrant = registration.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var (participationObject, assignmentType) = GetParticipationObjectFromRegistration(registration, msgs);
            var account = SessionVariables.RegistrationStartPageAccount ?? participationObject.OwnerAccount;
            var currentUser = SessionVariables.CurrentUser;

            ViewBag.Registration = registration;
            ViewBag.Account = account;
            ViewBag.IsSelfRegistered = masterRegistrant.WebUserId == currentUser.ID;
            ViewBag.Registrant = masterRegistrant;
            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.AssignmentType = assignmentType;
            ViewBag.Profile = account.GetAccountProfile();
            ViewBag.CurrentUser = currentUser;
        }

        private ActionResult ThankYouActionGet(string registrationUid)
        {
            var msgs = new SystemMessages();

            ThankYouViewBag(registrationUid, msgs);

            return View();
        }

        [HttpGet]
        public ActionResult ThankYou(string id)
        {
            return HandleIsLoggedIn() ?? ThankYouActionGet(id);
        }

        private ActionResult PaymentThankYouActionGet(string accountUid)
        {
            var msgs = new SystemMessages();

            ViewBag.Messages = msgs;
            ViewBag.Account = Account.LoadByUid(accountUid.ToGuid(), msgs);

            return View();
        }

        [HttpGet]
        public ActionResult PaymentThankYou(string id)
        {
            return HandleIsLoggedIn() ?? PaymentThankYouActionGet(id);
        }

        #endregion

        #region GetStartedStudent

        private List<FormQuestion> GetStartedStudentViewBag(SystemMessages msgs)
        {
            var form = FormService.GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                question.QuestionDataTypeId.NotIn(
                    FormConstants.QuestionsToNotIncludeInStudentPanel(false))).ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantEmail:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.ParticipationObject = RegistrationCommon.GetParticipationObjectFromCart();

            return questions;
        }

        [HttpGet]
        public ActionResult GetStartedStudent(string regaccount)
        {
            if (SessionVariables.CurrentUser.IsNotEmpty() &&
                SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            var msgs = new SystemMessages();
            var questions = GetStartedStudentViewBag(msgs);

            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");
            ViewBag.Messages = msgs;

            if (regaccount.IsGuid())
                ViewBag.RegistrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult GetStartedStudent(string regaccount, string password1, string password2, string agree,
            string textnotifications)
        {
            var msgs = new SystemMessages();

            var questions = GetStartedStudentViewBag(msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            if (password1.IsEmpty())
                msgs.Add("Password 1 is required", MessageType.UserError);
            if (password1.Length < 8)
                msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
            if (password2.IsEmpty())
                msgs.Add("Password 2 is required", MessageType.UserError);
            if (password1 != password2)
                msgs.Add("Passwords must match", MessageType.UserError);
            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()
                    ?.GetParticipationObject(msgs);
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
                        // create user
                        user = WebUserService.CreateNewWithOptionalPhone(
                            firstName.Value,
                            lastName.Value,
                            email.Value.Trim(),
                            phone.Value.Trim(),
                            password1,
                            true,
                            true,
                            false,
                            textnotifications.IfBlank("0").ToBoolean(),
                            null,
                            amsgs,
                            adc);

                        if (amsgs.HasNoErrors())
                            // create account
                            account = user.AddNewRegistrantAccount(
                                UserType.Student,
                                true,
                                SessionVariables.IsAfterSchool.Value,
                                user,
                                amsgs,
                                adc);


                        if (amsgs.HasNoErrors())
                        {
                            var location = participationObject?.Location;
                            if (location.IsNotEmpty() &&
                                location.GetHostAccount(amsgs, adc)?.AccountTypeId != AccountType.School)
                                location = null;

                            // create student registrant record
                            registrant = account.CreateRegistrant(
                                user,
                                user.AsList(),
                                RegistrantType.Individual,
                                true,
                                false,
                                false,
                                location?.ID, // probably should have a school location id
                                RegistrantProfileStatus.Started,
                                user,
                                amsgs,
                                adc);

                            if (amsgs.HasNoErrors())
                                // add answers to student registrant record
                                qvms.ForEach(qvm =>
                                {
                                    registrant.AddOrUpdateAnswer(
                                        qvm.Value.IfEmpty("").Trim(),
                                        qvm.Question,
                                        user,
                                        amsgs,
                                        adc);
                                });
                        }
                    },
                    smsgs =>
                    {
                        msgs.AddRange(smsgs);
                        SessionVariables.CurrentUser = user;
                        SessionVariables.CurrentAccount = account;
                        SessionVariables.AccountUserType = user.GetUserType(account);
                        SessionVariables.CurrentRegistrant = registrant;
                    },
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("inviteparent");

            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");
            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region InviteParent

        private List<FormQuestion> InviteParentViewBag(SystemMessages msgs)
        {
            var form = FormService.GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                question.QuestionDataTypeId.NotIn(
                    FormConstants.QuestionsToNotIncludeInStudentPanel(true))).ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.ParticipationObject = RegistrationCommon.GetParticipationObjectFromCart();

            return questions;
        }

        [HttpGet]
        public ActionResult InviteParent()
        {
            var msgs = new SystemMessages();
            var questions = InviteParentViewBag(msgs);

            ViewBag.Messages = msgs;

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult InviteParent(string textnotifications)
        {
            var msgs = new SystemMessages();

            var questions = InviteParentViewBag(msgs);
            // Get parent info from page
            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
            var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
            var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
            var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

            if (email.Value.IsEmpty())
                msgs.Add("Parent email is required", MessageType.UserError);
            else if (email.Value.Trim().IsSameAs(SessionVariables.CurrentUser.Email.Trim()))
                msgs.Add("Parent email cannot be the same as your student email address when creating a login account",
                    MessageType.UserError);

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        var location = participationObject?.Location;
                        var hostAccount = location?.GetHostAccount(amsgs, adc);
                        var locationString = "";

                        if (hostAccount.IsNotEmpty() && hostAccount.AccountTypeId != AccountType.School)
                            locationString = $" at {hostAccount.Name}";

                        // invite parent: 
                        var parentUser = SessionVariables.CurrentAccount.InviteUser(
                            firstName.Value,
                            lastName.Value,
                            email.Value.Trim(),
                            phone.Value.Trim(),
                            $"{SessionVariables.CurrentUser.FirstName} has signed up for an account on {Utilities.AppName()}{locationString}. Please create your parent {Utilities.AppName()} account to know and approve of activities your child requests to participate in.",
                            UserType.Owner,
                            true,
                            SessionVariables.CurrentUser,
                            msgs);

                        // Create parent registrant
                        var parentRegistrant = SessionVariables.CurrentAccount.CreateRegistrant(
                            parentUser,
                            parentUser.AsList(),
                            RegistrantType.Individual,
                            true,
                            true,
                            textnotifications.IfBlank("").ToBoolean(),
                            null,
                            RegistrantProfileStatus.Started,
                            SessionVariables.CurrentUser,
                            msgs,
                            adc);

                        SessionVariables.CurrentRegistrant.AddUserWithAccess(
                            parentUser,
                            SessionVariables.CurrentUser,
                            msgs,
                            adc);

                        // add answers to student registrant record
                        qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                SessionVariables.CurrentUser,
                                amsgs,
                                adc);
                        });
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                return RedirectToAction("RegisterStudent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region RegisterStudent

        private List<FormQuestion> RegisterStudentViewBag(ParticipationObject participationObject, SystemMessages msgs)
        {
            var allQuestions = participationObject.Form.GetActiveQuestions(msgs).ToList();
            var answers = SessionVariables.CurrentRegistrant.GetAnswers(msgs);
            var questions = allQuestions
                .Where(question => !question.IsDeleted && question.QuestionDataTypeId == QuestionDataType.NotSet &&
                                   answers.Find(answer => answer.FormQuestionId == question.ID).IsEmpty())
                .OrderBy(question => question.DisplayOrder)
                .ToList();

            ViewBag.Registrant = SessionVariables.CurrentRegistrant;
            ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();
            return questions;
        }

        private void ValidateRegisterStudentQuestions(List<FormQuestion> questions, SystemMessages msgs)
        {
            questions.ForEach(question =>
            {
                if (question.IsRequired)
                {
                    var populated = true;

                    if (question.QuestionTypeId == QuestionType.FileUpload &&
                        Request.Files[question.ToFormQuestionViewModel().Name].IsEmpty())
                        populated = false;
                    else if (question.QuestionTypeId != QuestionType.FileUpload &&
                             Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                        populated = false;

                    if (!populated)
                        msgs.Add($"{question.Text} is required", MessageType.UserError);
                }
            });
        }

        [HttpGet]
        public ActionResult RegisterStudent()
        {
            var msgs = new SystemMessages();

            // check if parent has been invited, if not redirect to parent invite page
            if (SessionVariables.CurrentRegistrant.GetParentOrGuardian(msgs).IsEmpty())
                return RedirectToAction("StudentRegistration");

            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var (submittedRecord, registration) =
                SessionVariables.CurrentRegistrant.GetSubmittedRecord(participationObject, msgs);

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            RegisterStudentViewBag(participationObject, msgs);

            ViewBag.AlreadyRegistered = submittedRecord.IsNotEmpty();
            ViewBag.Messages = msgs;
            return View(participationObject);
        }

        [HttpPost]
        public ActionResult RegisterStudent(string agree)
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegisterStudentViewBag(participationObject, msgs);
            ValidateRegisterStudentQuestions(questions, msgs);

            if (agree.IsEmpty()) msgs.Add("Must agree that you intend to participate", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();
            var registration = CreateStudentRegistration(participationObject, qvms, msgs);

            if (msgs.HasNoErrors() && registration.IsNotEmpty())
                return RedirectToAction("RegistrationComplete", new {id = registration?.Uid});

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);

            ViewBag.Messages = msgs;
            ViewBag.AlreadyRegistered = false;

            return View(participationObject);
        }

        private Registration CreateStudentRegistration(ParticipationObject participationObject,
            List<FormQuestionViewModel> qvms, SystemMessages msgs = null)
        {
            Registration registration = null;
            RegistrationPeriod period = null;
            Registrant submittedRecord = null;
            var currentAccount = SessionVariables.CurrentAccount;
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            var currentUser = SessionVariables.CurrentUser;

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        period = participationObject.GetCurrentRegistrationPeriod(amsgs, adc);

                        if (qvms.IsNotEmpty())
                            RegistrationCommon.AddOrUpdateStandardAnswers(
                                qvms,
                                currentRegistrant,
                                currentUser,
                                amsgs,
                                adc);

                        SubmitStudentRecord(
                            ref registration,
                            ref submittedRecord,
                            currentRegistrant,
                            participationObject,
                            currentUser,
                            period,
                            amsgs,
                            adc);

                        if (qvms.IsNotEmpty())
                            AddCustomQuestions(
                                qvms,
                                submittedRecord,
                                registration,
                                currentUser,
                                amsgs,
                                adc);


                        if (amsgs.HasNoErrors() && participationObject.IsSession())
                            AddSessionActivitiesToRegistration(
                                participationObject,
                                registration,
                                submittedRecord,
                                period,
                                amsgs,
                                adc);
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
            {
                if (registration.ID != null && participationObject.ObjectID != null &&
                    registration.RegistrationPeriodId != null && participationObject.Cost != null)
                {
                    var registrationAssignmentType = participationObject.RequiresParentalConsent
                        ? RegistrationAssignmentType.ReservedPendingParentalConsent
                        : RegistrationAssignmentType.Reserved;
                    submittedRecord.AddOrUpdateParticipationObject(
                        registration.ID.Value,
                        participationObject.ObjectID.Value,
                        participationObject.ObjectType,
                        registrationAssignmentType, registration.RegistrationPeriodId.Value, null,
                        participationObject.Cost.Value, currentUser.ID.Value, msgs);
                }

                // LOG: Someone Registered RegistrantName for ParticipationObjectType ParticipationObjectName
                SessionVariables.WebSession.LogActivity(
                    currentAccount,
                    currentUser,
                    WebSessionActionType.Registered,
                    ObjectType.Registrant,
                    objectId: currentRegistrant.ID.Value,
                    objectText: currentRegistrant.FullName,
                    indirectObjectType: participationObject.GetWebSessionObjectType(),
                    indirectObjectId: participationObject.ObjectID.Value,
                    indirectObjectText: participationObject.Name);
            }

            return registration;
        }

        private void SubmitStudentRecord(ref Registration registration, ref Registrant submittedRecord,
            Registrant currentRegistrant, ParticipationObject participationObject, WebUser currentUser,
            RegistrationPeriod period, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // account gets copy / snapshot of master registrant record
            // required in order to make sure registration data doesn't change as registrant changes information in master record.
            submittedRecord = currentRegistrant.SubmitRecordToAccount(
                participationObject.OwnerAccount,
                currentUser,
                msgs,
                dc);

            // create or update registration
            registration = submittedRecord.AddRegistration(
                participationObject.OwnerAccount.ID.Value,
                participationObject.GetAnyRelatedActivityGroupId(),
                participationObject.GetAccountActivityGroupId(),
                period?.ID,
                RegistrationCompletionStatus.RequiresParentSignature,
                currentUser.ID.Value,
                msgs,
                dc);

            //copy all answers from the main account to the registrant & registration :)
            currentRegistrant.CopyAnswersTo(
                submittedRecord,
                registration,
                currentUser,
                msgs,
                dc);
        }

        #endregion

        #region RegistrationComplete

        private void RegistrationCompleteViewBag(string registrationUid, SystemMessages msgs)
        {
            ResetRegistrationSession();

            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
            var registrant = registration.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var (participationObject, assignmentType) = GetParticipationObjectFromRegistration(registration, msgs);

            ViewBag.Registrant = masterRegistrant;
            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.AssignmentType = assignmentType;
        }

        private ActionResult RegistrationCompleteActionGet(string registrationUid)
        {
            var msgs = new SystemMessages();

            RegistrationCompleteViewBag(registrationUid, msgs);

            return View();
        }

        [HttpGet]
        public ActionResult RegistrationComplete(string id)
        {
            return HandleIsLoggedIn() ?? RegistrationCompleteActionGet(id);
        }

        #endregion

        #region Helpers

        private Registration CreateRegistration(ParticipationObject participationObject,
            List<FormQuestionViewModel> qvms, string signature, SystemMessages msgs = null)
        {
            Registration registration = null;
            RegistrationPeriod period = null;
            Registrant submittedRecord = null;
            var currentAccount = SessionVariables.CurrentAccount;
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            var currentUser = SessionVariables.CurrentUser;

            (submittedRecord, registration) = currentRegistrant.GetSubmittedRecord(participationObject, msgs);

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        period = participationObject.GetCurrentRegistrationPeriod(amsgs, adc);

                        if (qvms.IsNotEmpty())
                            RegistrationCommon.AddOrUpdateStandardAnswers(
                                qvms,
                                currentRegistrant,
                                currentUser,
                                amsgs,
                                adc);

                        SubmitRecord(
                            ref registration,
                            ref submittedRecord,
                            currentRegistrant,
                            participationObject,
                            currentUser,
                            period,
                            signature,
                            amsgs,
                            adc);

                        if (qvms.IsNotEmpty())
                            AddCustomQuestions(
                                qvms,
                                submittedRecord,
                                registration,
                                currentUser,
                                amsgs,
                                adc);

                        if (amsgs.HasNoErrors() && participationObject.IsSession())
                            AddSessionActivitiesToRegistration(
                                participationObject,
                                registration,
                                submittedRecord,
                                period,
                                amsgs,
                                adc);
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                // LOG: Someone Registered RegistrantName for ParticipationObjectType ParticipationObjectName
                SessionVariables.WebSession.LogActivity(
                    currentAccount,
                    currentUser,
                    WebSessionActionType.Registered,
                    ObjectType.Registrant,
                    objectId: currentRegistrant.ID.Value,
                    objectText: currentRegistrant.FullName,
                    indirectObjectType: participationObject.GetWebSessionObjectType(),
                    indirectObjectId: participationObject.ObjectID.Value,
                    indirectObjectText: participationObject.Name
                );

            return registration;
        }

        public string CheckRedirect(bool checkForEmptyCart = false)
        {
            var currentAccountNotRegistrant = SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant;
            var isEmptyCart = checkForEmptyCart && SessionVariables.Cart.IsEmpty();
            if (SessionVariables.RegistrationStartPageAccount.IsNotEmpty())
            {
                var redirectUrl = SessionVariables.RegistrationStartPageAccount.HomePageUrl;

                var isEmptyUserOrAccount = SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty();
                if (isEmptyUserOrAccount || currentAccountNotRegistrant || isEmptyCart)
                    return redirectUrl;
            }

            if (SessionVariables.CurrentUser.IsEmpty())
                return Utilities.LoginUrl();

            if (SessionVariables.CurrentAccount.IsEmpty())
                return new UrlHelper(ControllerContext.RequestContext).Action("select", "login");

            if (currentAccountNotRegistrant)
                return DetermineUrlForLoginRedirect(null, null);

            if (isEmptyCart)
                return DetermineUrlForLoginRedirect(null, null);

            return null;
        }


        public string GetUserHostAddress()
        {
            if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"].IsNotEmpty())
                return Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0];

            return Request.ServerVariables["REMOTE_ADDR"];
        }

        private void SubmitRecord(ref Registration registration, ref Registrant submittedRecord,
            Registrant currentRegistrant, ParticipationObject participationObject, WebUser currentUser,
            RegistrationPeriod period, string signature, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // account gets copy / snapshot of master registrant record
            // required in order to make sure registration data doesn't change as registrant changes information in master record.
            if (submittedRecord.IsEmpty())
            {
                submittedRecord = currentRegistrant.SubmitRecordToAccount(
                    participationObject.OwnerAccount,
                    currentUser,
                    msgs,
                    dc);

                // create or update registration
                registration = submittedRecord.AddRegistrationWithESignature(
                    participationObject.OwnerAccount.ID.Value,
                    participationObject.GetAnyRelatedActivityGroupId(),
                    participationObject.GetAccountActivityGroupId(),
                    period?.ID,
                    signature,
                    Request.UserAgent.MaxChars(400),
                    GetUserHostAddress(),
                    currentUser.ID.Value,
                    msgs,
                    dc);
            }
            else
            {
                registration.AddESignature(
                    signature,
                    Request.UserAgent.MaxChars(400),
                    GetUserHostAddress(),
                    currentUser,
                    msgs,
                    dc);
            }

            //copy all answers from the main account to the registrant & registration :)
            currentRegistrant.CopyAnswersTo(
                submittedRecord,
                registration,
                currentUser,
                msgs,
                dc);
        }

        private void AddCustomQuestions(List<FormQuestionViewModel> qvms, Registrant submittedRecord,
            Registration registration, WebUser currentUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // add custom answers to questions directly to this registration
            qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.NotSet)
                .ToList()
                .ForEach(qvm =>
                {
                    if (qvm.Question.QuestionTypeId == QuestionType.FileUpload)
                    {
                        if (Request.Files[qvm.Name].IsNotEmpty() && Request.Files[qvm.Name].ContentLength > 0)
                        {
                            var currentFileSizeInBytes = Request.Files[qvm.Name].ContentLength;
                            //5 MB = 5242880 Bytes
                            //File uploaded here should be less than of 5MB
                            if (currentFileSizeInBytes <= 5242880)
                                submittedRecord.AddOrUpdateAnswer(
                                    Request.Files[qvm.Name].InputStream,
                                    Request.Files[qvm.Name].FileName,
                                    qvm.Question,
                                    registration.ID,
                                    currentUser,
                                    msgs,
                                    dc);
                            else
                                msgs.Add("Upload a file less than or equal to 5 megabytes", MessageType.UserError);
                        }
                    }
                    else
                    {
                        submittedRecord.AddOrUpdateAnswer(
                            qvm.Value,
                            qvm.Question,
                            registration?.ID,
                            currentUser,
                            msgs,
                            dc);
                    }
                });
        }

        private (ParticipationObject, RegistrationAssignmentType) GetParticipationObjectFromRegistration(
            Registration registration, SystemMessages msgs)
        {
            var registrantActivities = registration.GetRegistrantActivities(msgs);
            RegistrantActivity registrantActivity = null;

            if (registrantActivities.Count == 1)
                registrantActivity = registrantActivities.First();
            else
                registrantActivity = registrantActivities
                    .First(x => x.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup);

            var assignmentType = registrantActivity.AssignmentTypeId;

            return (registrantActivity.GetParticipationObject(msgs), assignmentType);
        }


        private List<DateTime> GetSelectedDatesFromCart()
        {
            return SessionVariables.Cart?.GetCartItems().FirstOrDefault()?.SelectedDates?.Split(',')
                .Select(x => x.ToDateTime()).ToList();
        }

        private void ResetRegistrationSession()
        {
            SessionVariables.CurrentRegistrant = null;
            SessionVariables.Cart = null;
        }

        private ActionResult HandleIsLoggedIn()
        {
            if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return Logout(Request.Url.ToString(), true);

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return Logout(Request.Url.ToString(), true);

            return null;
        }

        private void AddSessionActivitiesToRegistration(ParticipationObject participationObject,
            Registration registration, Registrant submittedRecord, RegistrationPeriod period, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            // if there is a Session, then a person may set their preferences for activities in that session, 
            // also only if the session is set to Apply
            if (participationObject.IsSession())
            {
                // only get activities that are in the cart
                var participationObjects = GetAllParticipationObjectsForCheckout(registration, msgs, dc);

                if (participationObject.RegistrationMode == RegistrationMode.Apply)
                {
                    for (var i = 1; i <= participationObjects.Count; i++)
                    {
                        var postedValue = Request[$"activity-preference-{i}"];
                        if (postedValue.IsGuid())
                        {
                            var po = participationObjects.First(x => x.Uid == postedValue.ToGuid());

                            registration.ProcessRegistrationAssignment(
                                po,
                                SessionVariables.CurrentRegistrant,
                                submittedRecord,
                                1,
                                i,
                                participationObject.GetActivityGroupId(),
                                amountCharged: po.Cost,
                                periodId: period?.ID,
                                registeredDates: null,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: msgs,
                                dc: dc);
                        }
                    }

                    // make sure the number of final registered programs is equal to the number of programs that is in the cart + what already exists
                    var registeredPOs = GetCurrentlyRegisteredForParticipationObjects(registration, msgs, dc);

                    if (registeredPOs.Count != participationObjects.Count)
                        msgs.Add(
                            $"Please enter all of your {UILabel.ActivityLabel().ToLower()} preferences",
                            MessageType.UserError);
                }
                else
                {
                    participationObjects.ForEach(po =>
                    {
                        registration.ProcessRegistrationAssignment(
                            po,
                            SessionVariables.CurrentRegistrant,
                            submittedRecord,
                            1,
                            participationObject.GetActivityGroupId(),
                            amountCharged: po.Cost,
                            periodId: period?.ID,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: msgs,
                            dc: dc);
                    });
                }
            }
        }

        private List<ParticipationObject> GetAllParticipationObjectsForCheckout(Registration registration,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var participationObjects = new List<ParticipationObject>();

            participationObjects.AddRange(SessionVariables.Cart.GetCartItems(msgs, dc)
                .Where(x => x.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                .Select(x => x.GetParticipationObject(msgs, dc))
                .Where(x => x.GetActivity().ActivityGroupId.HasValue));

            if (registration.IsNotEmpty())
                participationObjects.AddRange(GetCurrentlyRegisteredForParticipationObjects(registration, msgs, dc));

            return participationObjects.DistinctBy(x => x.ObjectID).OrderBy(x => x.Name).ToList();
        }

        private List<ParticipationObject> GetCurrentlyRegisteredForParticipationObjects(Registration registration,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ignoredTypes = new List<RegistrationAssignmentType>
            {
                RegistrationAssignmentType.Declined,
                RegistrationAssignmentType.Withdrawn,
                RegistrationAssignmentType.Invited,
                RegistrationAssignmentType.NotSet,
            };

            return registration.GetRegistrantActivities(msgs, dc)
                .Where(x => x.AssignmentTypeId.NotIn(ignoredTypes))
                .Where(x => x.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                .Select(x => x.GetParticipationObject(msgs, dc))
                .ToList();
        }

        private void LoadSessionActivitiesViewBag(ParticipationObject participationObject, Registration registration,
            SystemMessages msgs)
        {
            if (participationObject.IsSession() && participationObject.RegistrationMode == RegistrationMode.Apply)
                ViewBag.SessionActivities = GetAllParticipationObjectsForCheckout(registration, msgs);
        }

        #endregion
    }
}