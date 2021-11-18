using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json.Converters;
using Rollbar.DTOs;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Management;
using System.Web.Mvc;
using AfterSchool.Helpers;
using System;

namespace AfterSchool.Controllers
{
    public class AccountController : BaseController
    {
        #region Interested

        private List<FormQuestionViewModel> InterestedViewBag(ParticipationObject participationObject, SystemMessages msgs)
        {
            var currentUser = SessionVariables.CurrentUser;
            var form = GetInterestForm(msgs);
            var intererestGroup = form.GetQuestionGroups(msgs)
                                    .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.RegistrantInterestInformation);
            var questions = intererestGroup.GetAllQuestions(msgs);

            var nonIncludedQuestionsExistingUser = new List<QuestionDataType>()
            {
                QuestionDataType.RegistrantFirstName,
                QuestionDataType.RegistrantLastName,
                QuestionDataType.RegistrantEmail
            };

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantChild1Grade:
                        question.IsRequired = true;
                        break;
                    case QuestionDataType.RegistrantChild2Grade:
                    case QuestionDataType.RegistrantChild3Grade:
                    case QuestionDataType.RegistrantChild4Grade:
                    case QuestionDataType.RegistrantChild5Grade:
                    case QuestionDataType.RegistrantMobilePhone:
                        question.IsRequired = false;
                        break;
                }
            });

            ViewBag.ParticipationObject = participationObject;

            var qvms = questions.Select(question => new FormQuestionViewModel(question, Request.Params)).ToList();

            if (!currentUser.IsEmpty())
            {
                qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value = currentUser.FirstName;
                qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value = currentUser.LastName;
                qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value = currentUser.Email;

                qvms.ForEach(qvm =>
                {
                    if (qvm.Question.QuestionDataTypeId.In(nonIncludedQuestionsExistingUser))
                    {
                        qvm.Hide = true;
                        qvm.Question.IsRequired = false;
                    }
                });
            }

            return qvms;
        }

        [HttpGet]
        public ActionResult Interested(string regaccount)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var registrationStartPageAccount = SessionVariables.RegistrationStartPageAccount;

            // handles admin accounts that have or need parent accounts
            var redirectResult = HandleGetStartedAccountRedirect(true, msgs);
            if (redirectResult.IsNotEmpty())
                return redirectResult;

            var participationObject = GetParticipationObjectFromCart();
            var qvms = InterestedViewBag(participationObject, msgs);
            Account registrationAccount = null;

            if (regaccount.IsGuid())
                registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            // no activity is selected
            if (participationObject.IsEmpty())
            {
                if (registrationStartPageAccount.IsNotEmpty() || registrationAccount.IsNotEmpty())
                    return Redirect(registrationStartPageAccount?.HomePageUrl ?? registrationAccount?.HomePageUrl);

                return HttpNotFound();
            }

            // pre-load phone number
            if (currentUser.IsNotEmpty())
                qvms.Find(item => item.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value = currentUser.Phone;

            ViewBag.RegistrationAccount = registrationAccount;
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult Interested(string regaccount, string agree)
        {
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var participationObject = GetParticipationObjectFromCart();
            var qvms = InterestedViewBag(participationObject, msgs);

            WebUser user = null;
            Account account = null;
            Registrant registrant = null;
            Registration registration = null;
            Registrant submittedRecord = null;
            RegistrationPeriod period = null;

            qvms.ForEach(qvm =>
            {
                if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        user = CreateOrUpdateInterestedUser(
                            currentUser,
                            firstName.Value,
                            lastName.Value,
                            email.Value,
                            phone.Value,
                            amsgs,
                            adc);
                        
                        if (amsgs.HasNoErrors())
                            account = CreateOrUpdateAccount(user, amsgs, adc);

                        if (amsgs.HasNoErrors())
                        {
                            registrant = CreateOrUpdateRegistrant(account, user, amsgs, adc);

                            if (amsgs.HasNoErrors())
                                AddOrUpdateStandardAnswers(qvms, registrant, user, amsgs, adc);
                        }

                        if (amsgs.HasNoErrors())
                        {
                            (submittedRecord, registration) = registrant.GetSubmittedRecord(participationObject, amsgs, adc);
                            period = participationObject.GetCurrentRegistrationPeriod(amsgs, adc);

                            SubmitRecord(ref registration, ref submittedRecord, registrant, participationObject, user, period, null, amsgs, adc);
                        }

                        if (msgs.HasNoErrors())
                            AccountService.ProcessInterestRegistration(
                                checkoutObject: new CheckoutObject()
                                {
                                    BillingAccount = participationObject.OwnerAccount,
                                    PayingAccount = account,
                                    NumberOfSeats = 1,
                                    ParticipationObject = participationObject,
                                    Registration = registration,
                                    MasterRegistrant = registrant,
                                },
                                sendConfirmationEmail: true,
                                loggedInUser: user,
                                msgs: amsgs,
                                dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account);

                        // Show Succes Message
                        // LOG: Someone is Interested in Activity ActivityName for Account AccountName
                        msgs.Add(string.Format("Your Interest in {0} has been recorded", participationObject.Name), MessageType.Success);

                        SessionVariables.WebSession.LogActivity(
                            account: account,
                            actor: currentUser,
                            actionType: WebSessionActionType.Interested,
                            objectType: participationObject.GetWebSessionObjectType(),
                            objectId: participationObject.ObjectID.Value,
                            objectText: participationObject.Name,
                            indirectObjectType: ObjectType.Account,
                            indirectObjectId: account.ID.Value,
                            indirectObjectText: account.Name
                       );
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("thankyou", new { id = registration.Uid });

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

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

                if (qvm.Question.QuestionDataTypeId.ToString().ToLower().Contains("phone"))
                {
                    if (qvm.Value.IsNotEmpty() && !qvm.Value.IsPhone())
                        msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);

                }
            });

            if (msgs.HasNoErrors())
            {
                var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                Registrant submittedContact = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        submittedContact = registrationAccount.AddContact(
                                            firstName.Value, 
                                            lastName.Value, 
                                            email.Value, 
                                            null, 
                                            true,
                                            null, 
                                            amsgs, 
                                            adc);

                        if (amsgs.HasNoErrors())
                            AddOrUpdateStandardAnswers(qvms, submittedContact, null, amsgs, adc);

                        if (amsgs.HasNoErrors())
                            AddCustomQuestions(
                                qvms,
                                submittedContact,
                                null,
                                null,
                                amsgs,
                                adc);                        
                    },
                    onSuccess: (SystemMessages smsgs) => 
                    {
                        msgs.AddRange(smsgs);
                        AfterSchoolLibrary.Helpers.EmailHelper.SendContactFormConfirmation(registrationAccount, submittedContact, msgs);
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("contactthankyou", new { id = registrationAccount.Uid });

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

        private List<FormQuestionViewModel> GetStartedParentViewBag(ParticipationObject participationObject, SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);
            var nonIncludedQuestions = new List<QuestionDataType>()
            {
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantGrade,
                QuestionDataType.RegistrantGender,
                QuestionDataType.RegistrantDateOfBirth,
                QuestionDataType.RegistrantEthnicity,
                QuestionDataType.RegistrantTShirtSize
            };

            var nonIncludedQuestionsExistingUser = new List<QuestionDataType>()
            {
                QuestionDataType.RegistrantFirstName,
                QuestionDataType.RegistrantLastName,
                QuestionDataType.RegistrantEmail
            };

            questions = questions.Where(question => question.QuestionDataTypeId.NotIn(nonIncludedQuestions)).ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.ParticipationObject = participationObject;

            var qvms = questions.Select(question => new FormQuestionViewModel(question, Request.Params)).ToList();

            if (!SessionVariables.CurrentUser.IsEmpty())
            {
                qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value = SessionVariables.CurrentUser.FirstName;
                qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value = SessionVariables.CurrentUser.LastName;
                qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value = SessionVariables.CurrentUser.Email;

                qvms.ForEach(qvm =>
                {
                    if (qvm.Question.QuestionDataTypeId.In(nonIncludedQuestionsExistingUser))
                    {
                        qvm.Hide = true;
                        qvm.Question.IsRequired = false;
                    }
                });
            }

            return qvms;
        }

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
                        registrantAccount = currentUser.AddNewRegistrantAccount(UserType.Owner, true, true, currentUser, msgs);
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

                // if you have a completed parent profile
                if (masterRegistrant.IsNotEmpty() && masterRegistrant.ProfileStatusId == RegistrantProfileStatus.Completed && !isInterestForm)
                    return RedirectToAction("select");
            }

            return null;
        }

        [HttpGet]
        public ActionResult GetStarted(string regaccount)
        {
            var msgs = new SystemMessages();

            // handles admin accounts that have or need parent accounts
            var redirectResult = HandleGetStartedAccountRedirect(false, msgs);
            if (redirectResult.IsNotEmpty())
                return redirectResult;

            var participationObject = GetParticipationObjectFromCart();
            var qvms = GetStartedParentViewBag(participationObject, msgs);
            Account registrationAccount = null;

            if (regaccount.IsGuid())
                registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            // no activity is selected
            if (participationObject.IsEmpty())
            {
                if (SessionVariables.RegistrationStartPageAccount.IsNotEmpty() || registrationAccount.IsNotEmpty())
                    return Redirect(SessionVariables.RegistrationStartPageAccount?.HomePageUrl ?? registrationAccount?.HomePageUrl);

                return HttpNotFound();
            }

            // pre-load phone number
            if (SessionVariables.CurrentUser.IsNotEmpty())
                qvms.Find(item => item.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value = SessionVariables.CurrentUser.Phone;

            ViewBag.RegistrationAccount = registrationAccount;
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult GetStarted(string regaccount, string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();
            var participationObject = GetParticipationObjectFromCart();
            var qvms = GetStartedParentViewBag(participationObject, msgs);
            var currentUser = SessionVariables.CurrentUser;

            qvms.ForEach(qvm =>
            {
                if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
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

                if (AfterSchoolLibrary.Helpers.Utilities.IsProduction)  
                    msgs.AddRange(RecaptchaHelper.Validate(Request["g-recaptcha-response"]));
            }
            if (AfterSchoolLibrary.Helpers.Utilities.IsProduction && AfterSchoolLibrary.Helpers.Utilities.IsStaging)
            {
                if (!agree.IsBoolean())
                    msgs.Add("You must agree to the terms of service", MessageType.UserError);
            }
            

            if (msgs.HasNoErrors())
            {
                var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

                WebUser user = null;
                Account account = null;
                Registrant registrant = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        user = CreateOrUpdateUser(
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
                            account = CreateOrUpdateAccount(user, amsgs, adc);

                            // LOG: Someone Created Account AccountName
                            SessionVariables.WebSession.LogActivity(
                                account: account,
                                actor: currentUser,
                                actionType: WebSessionActionType.Created,
                                objectType: ObjectType.Account,
                                objectId: account.ID.Value,
                                objectText: account.Name
                           );

                            // LOG: Someone Created User UserName
                            SessionVariables.WebSession.LogActivity(
                                account: account,
                                actor: currentUser,
                                actionType: WebSessionActionType.SignedUp,
                                objectType: ObjectType.WebUser,
                                objectId: user.ID.Value,
                                objectText: user.FullName
                           );
                        }

                        if (amsgs.HasNoErrors())
                            registrant = CreateOrUpdateRegistrant(account, user, amsgs, adc);

                        if (amsgs.HasNoErrors())
                            AddOrUpdateStandardAnswers(qvms, registrant, user, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account);
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addhomeaddress");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddHomeAddress

        private List<FormQuestion> AddHomeAddressViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var homeAddressQuestionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress);
            var questions = homeAddressQuestionGroup.GetAllQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantAddress2:
                        question.IsRequired = false;
                        break;
                    default:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.ParticipationObject = GetParticipationObjectFromCart();

            return questions;
        }

        [HttpGet]
        public ActionResult AddHomeAddress()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var questions = AddHomeAddressViewBag(msgs);

            ViewBag.Messages = msgs;

            return View(questions.Select(question => question.ToFormQuestionViewModel()).ToList());
        }

        [HttpPost]
        public ActionResult AddHomeAddress(string id)
        {
            var msgs = new SystemMessages();
            var questions = AddHomeAddressViewBag(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();

            questions.ForEach(question =>
            {
                if (question.IsRequired && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddOrUpdateAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addparent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddParent

        private List<FormQuestion> AddParentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGrade &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGender &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantDateOfBirth &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEthnicity &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantTShirtSize
                )
                .ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    // case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.ParticipationObject = GetParticipationObjectFromCart();

            return questions;
        }

        [HttpGet]
        public ActionResult AddParent()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var parentOrGuardian = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var questions = AddParentViewBag(msgs);
            var values = new NameValueCollection();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantHomePhone:
                        values.Add(question.ToFormQuestionViewModel().Id, parentOrGuardian.GetAnswer(question.QuestionDataTypeId)?.Text);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();

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
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var canBeAccessedBy = SessionVariables.CurrentUser.AsList();

                        // create student account if requestioned by parent
                        if (createuseraccount.IsNumber())
                        {
                            var firstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            var lastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
                            var email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value;
                            var phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value;

                            otherParentUser = SessionVariables.CurrentAccount.InviteUser(
                                 firstName: firstName,
                                 lastName: lastName,
                                 email: email,
                                 phone: phone.Trim(),
                                 message: null,
                                 userType: UserType.Student,
                                 loggedInUser: SessionVariables.CurrentUser,
                                 msgs: amsgs,
                                 dc: adc);

                            canBeAccessedBy.Add(otherParentUser);
                        }

                        // create parent registrant record
                        otherParent = SessionVariables.CurrentAccount.CreateRegistrant(
                            registrantOwner: otherParentUser,
                            canBeAccessedBy: canBeAccessedBy,
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: true,
                            allowTextMessages: false,
                            schoolLocationId: null,
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        qvms.ForEach(qvm =>
                        {
                            otherParent.AddOrUpdateAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addemergencycontacts");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddEmergencyContacts

        private List<FormQuestion> AddEmergencyContactsViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var emergencyContactsQuestionGroups = form.GetQuestionGroups(msgs)
                .Where(group =>
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 ||
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2)
                .ToList();
            var questions = new List<FormQuestion>();

            emergencyContactsQuestionGroups.ForEach(group => questions.AddRange(group.GetAllQuestions(msgs)));
            questions.Where(question => question.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1)
                .ToList()
                .ForEach(question => { question.IsRequired = true; });

            ViewBag.ParticipationObject = GetParticipationObjectFromCart();

            return questions;
        }

        [HttpGet]
        public ActionResult AddEmergencyContacts()
        {
            var results = CheckRedirect();
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var questions = AddEmergencyContactsViewBag(msgs);

            ViewBag.Messages = msgs;

            return View(questions.Select(question => question.ToFormQuestionViewModel()).ToList());
        }

        [HttpPost]
        public ActionResult AddEmergencyContacts(string id)
        {
            var msgs = new SystemMessages();
            var questions = AddEmergencyContactsViewBag(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var phones = new List<string>();

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.EmergencyContact1Phone:
                    case QuestionDataType.EmergencyContact2Phone:
                        if (answer.IsNotEmpty() && !answer.Trim().IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.FormQuestionGroupQuestionGroupDataTypeId.ToString().SplitByUpperCase()} {question.Text} must be a valid phone number", MessageType.UserError);

                        if (answer.IsNotEmpty())
                            phones.Add(answer);

                        break;

                }
            });

            if (phones.Count == 2 && phones[0] == phones[1])
                msgs.Add("Emergency phone numbers should not be the same", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // mark parent profile as completed since they've went through all the steps
                        parentRegistrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddOrUpdateAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addstudent");

            ViewBag.Messages = msgs;

            return View(qvms);
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
                    question.QuestionDataTypeId != QuestionDataType.RegistrantWorkPhone
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
            ViewBag.ParticipationObject = GetParticipationObjectFromCart();
            ViewBag.ShowStepNumber = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs).Count == 0;

            return questions;
        }

        [HttpGet]
        public ActionResult AddStudent()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var adminAccountForm = GetAdministratorAccountForm(msgs);
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
                        values.Add(question.ToFormQuestionViewModel().Id, parentOrGuardian.GetAnswer(question.QuestionDataTypeId)?.Text);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult AddStudent(string id, string samehomeaddress, string createuseraccount, string locationname = null, string streetnumber = null, string streetname = null, string address2 = null, string neighborhood = null, 
            string city = null, string county = null, string state = null, string postalcode = null, string country = null, string placeid = null, decimal? latitude = null, decimal? longitude = null, bool schoolnotfound = false)
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var adminAccountForm = GetAdministratorAccountForm(msgs);
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
                        if (answer.IsNotEmpty() && !answer.Trim().IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            if (placeid.IsEmpty() && !schoolnotfound)
                msgs.Add($"Location is required", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();
            var email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value;

            if (createuseraccount.IsNumber() && !email.IsEmail())
                msgs.Add("Email is required and must be valid in order to allow your child to login", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Registrant student = null;
                WebUser studentUser = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var canBeAccessedBy = currentUser.AsList();
                        Location school = null;
                        
                        if (placeid.IsNotEmpty())
                            school = LocationService.CreateOrUpdateFromGoogle(locationname, streetnumber, streetname, address2, neighborhood, city, county, state, postalcode, country, placeid, latitude, longitude, SessionVariables.CurrentUser, amsgs, adc);


                        // create student account if requested by parent
                        if (createuseraccount.IsNumber())
                        {
                            var firstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            var lastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
                            // var phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value;

                            studentUser = currentAccount.CreateAndInviteUser(
                                 firstName: firstName,
                                 lastName: lastName,
                                 email: email,
                                 phone: null,
                                 message: $"Your parent/guardian ({currentUser.DisplayName}) has invited you to sign up for {AfterSchoolLibrary.Helpers.Utilities.AppName()} to register for activities",
                                 userType: UserType.Student,
                                 loggedInUser: currentUser,
                                 msgs: amsgs,
                                 dc: adc);

                            canBeAccessedBy.Add(studentUser);
                        }

                        // create student registrant record
                        student = SessionVariables.CurrentAccount.CreateRegistrant(
                            registrantOwner: studentUser,
                            canBeAccessedBy: canBeAccessedBy,
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: false,
                            allowTextMessages: false,
                            schoolLocationId: school.IsNotEmpty() ? school.ID : new int?(),
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: currentUser,
                            msgs: amsgs,
                            dc: adc);

                        // LOG: Someone Created Registrant RegistrantName for Account AccountName
                        if (student.IsNotEmpty() && amsgs.HasNoErrors())
                        {
                            SessionVariables.WebSession.LogActivity(
                                account: currentAccount,
                                actor: currentUser,
                                actionType: WebSessionActionType.Created,
                                objectType: ObjectType.Registrant,
                                objectId: student.ID.Value,
                                objectText: student.FullName,
                                indirectObjectType: ObjectType.Account,
                                indirectObjectId: currentAccount.ID.Value,
                                indirectObjectText: currentAccount.Name
                           );
                        }
                            
                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            student.AddOrUpdateAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: amsgs,
                                dc: adc);
                        });

                        // copy parent answers to student for home address, emergency contacts
                        parentRegistrant.GetAnswers(amsgs, adc)
                            .Where(answer =>
                                answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress ||
                                answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 ||
                                answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2
                            )
                            .ToList()
                            .ForEach(answer =>
                            {
                                student.AddOrUpdateAnswer(
                                    answerText: answer.Text.IfEmpty("").Trim(),
                                    question: answer.ToFormQuestion(msgs: amsgs, dc: adc),
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });

                        // copy parent info over to student parent info
                        var allquestions = adminAccountForm.GetAllQuestions(amsgs, adc);

                        var parent1Map = new Dictionary<QuestionDataType, QuestionDataType>()
                        {
                            { QuestionDataType.RegistrantFirstName,  QuestionDataType.Parent1FirstName },
                            { QuestionDataType.RegistrantLastName,  QuestionDataType.Parent1LastName },
                            { QuestionDataType.RegistrantEmail,  QuestionDataType.Parent1Email },
                            { QuestionDataType.RegistrantHomePhone,  QuestionDataType.Parent1HomePhone },
                            { QuestionDataType.RegistrantMobilePhone,  QuestionDataType.Parent1MobilePhone },
                            { QuestionDataType.RegistrantWorkPhone,  QuestionDataType.Parent1WorkPhone },
                        };

                        foreach (var key in parent1Map.Keys)
                        {
                            student.MapAndAddAnswer(
                               allquestions,
                               parentRegistrant,
                               key,
                               parent1Map[key],
                               SessionVariables.CurrentUser,
                               amsgs,
                               adc);
                        }

                        var parent1AddressMap = new Dictionary<QuestionDataType, QuestionDataType>()
                        {
                            { QuestionDataType.RegistrantAddress1,  QuestionDataType.Parent1Address1 },
                            { QuestionDataType.RegistrantAddress2,  QuestionDataType.Parent1Address2 },
                            { QuestionDataType.RegistrantCity,  QuestionDataType.Parent1City },
                            { QuestionDataType.RegistrantState,  QuestionDataType.Parent1State },
                            { QuestionDataType.RegistrantPostalCode,  QuestionDataType.Parent1PostalCode },
                        };

                        foreach (var key in parent1AddressMap.Keys)
                        {
                            student.MapAndAddAnswer(
                               allquestions,
                               parentRegistrant,
                               key,
                               parent1AddressMap[key],
                               SessionVariables.CurrentUser,
                               amsgs,
                               adc);
                        }

                        if (parentRegistrant2.IsNotEmpty())
                        {
                            // get parent 2 info
                            var parent2Map = new Dictionary<QuestionDataType, QuestionDataType>()
                            {
                                { QuestionDataType.RegistrantFirstName,  QuestionDataType.Parent2FirstName },
                                { QuestionDataType.RegistrantLastName,  QuestionDataType.Parent2LastName },
                                { QuestionDataType.RegistrantEmail,  QuestionDataType.Parent2Email },
                                { QuestionDataType.RegistrantHomePhone,  QuestionDataType.Parent2HomePhone },
                                { QuestionDataType.RegistrantMobilePhone,  QuestionDataType.Parent2MobilePhone },
                                { QuestionDataType.RegistrantWorkPhone,  QuestionDataType.Parent2WorkPhone },
                            };

                            foreach (var key in parent2Map.Keys)
                            {
                                student.MapAndAddAnswer(
                                   allquestions,
                                   parentRegistrant2,
                                   key,
                                   parent2Map[key],
                                   SessionVariables.CurrentUser,
                                   amsgs,
                                   adc);
                            }
                        }

                        // TODO: maybe should get sibling and copy health info
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);

                        SessionVariables.CurrentRegistrant = student;
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addhealth");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddHealth

        private List<FormQuestion> AddHealthViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
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

            ViewBag.ParticipationObject = GetParticipationObjectFromCart();
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
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // mark student profile as completed since they've went through all the steps
                        registrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            registrant.AddOrUpdateAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUser: currentUser,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

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
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();

            var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant();

            if (masterRegistrant.IsEmpty())
                return RedirectToAction("getstarted");

            if (masterRegistrant.ProfileStatusId == RegistrantProfileStatus.Started)
                return RedirectToAction("getstarted");

            if (!masterRegistrant.IsParentOrGuardian)
            {
                SessionVariables.CurrentRegistrant = masterRegistrant;
                return RedirectToAction("registerstudent");
            }

            var students = SessionVariables.CurrentUser.GetChildrenRegistrants(msgs);

            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = GetParticipationObjectFromCart();
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
            var allQuestions = participationObject.Form
                .GetActiveQuestions(msgs)
                .ToList();
            var requiredQuestions = allQuestions
                .Where(question => question.IsRequired &&
                    question.FormQuestionGroupRegistrantTypeId == RegistrantType.Individual &&
                    question.QuestionDataTypeId != QuestionDataType.NotSet)
                .ToList();

            var answers = SessionVariables.CurrentRegistrant.GetAnswers(msgs);

            var questions = requiredQuestions.Where(question => 
                    answers.FirstOrDefault(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).IsEmpty() ||
                    answers.FirstOrDefault(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).Text.IsBlank()
                )
                .Distinct()
                .ToList();

            questions.AddRange(
                    allQuestions
                        .Where(question => question.QuestionDataTypeId == QuestionDataType.NotSet)
                        .Where(question => answers.Find(answer => answer.FormQuestionId == question.ID).IsEmpty())
                );
            
            ViewBag.Registrant = SessionVariables.CurrentRegistrant;

            return questions;
        }

        private ActionResult RegisterActionGet()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = GetParticipationObjectFromCart();
            var selectedDates = GetSelectedDatesFromCart();
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            var parentOrGuardian = currentRegistrant.GetParentOrGuardian(msgs);
            var (submittedRecord, registration) = currentRegistrant.GetSubmittedRecord(participationObject, msgs);

            if (!parentOrGuardian.IsProfileCompleted && parentOrGuardian.GetOwner(msgs).ID == SessionVariables.CurrentUser.ID)
                return RedirectToAction(
                    "editpersonal",
                    "family",
                    new { Area = "user", id = parentOrGuardian.Uid, reg = registration?.Uid, student = currentRegistrant?.Uid, returnUrl = Url.Action("select", "account") });

            if (!currentRegistrant.IsProfileCompleted)
                return RedirectToAction(
                    "editstudentpersonal",
                    "family",
                    new { Area = "user", id = currentRegistrant.Uid, reg = registration?.Uid, student = currentRegistrant?.Uid, returnUrl = Url.Action("select", "account") });

            // check grade eligibility (extra check)
            if (!participationObject.IsStudentGradeInRange(currentRegistrant, msgs))
                return RedirectToAction("select");

            var needsToRegister = submittedRecord.NeedsToRegister(registration, participationObject, selectedDates, msgs);

            // current registrant is already registered
            if (!needsToRegister && !participationObject.IsSession())
                return RedirectToAction("select");

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegisterViewBag(participationObject, msgs);

            // if there are no additional questions to answer, 
            // if it doesn't require parental consent, 
            // the registration has already been started (by the child)
            // and it's not a session (parents can edit registrations for sessions)
            var gotoPayment = !questions.Any() && !participationObject.RequiresParentalConsent && registration.IsNotEmpty() && !participationObject.IsSession();

            if (gotoPayment)
            {
                if (participationObject.ShouldParentPay())
                    return RedirectToAction("payment", new { id = registration.Uid });
                
                return RedirectToAction("finalizeregistration", new { id = registration.Uid });
            }

            ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();
            ViewBag.Messages = msgs;

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);

            return View(participationObject);
        }

        [HttpGet]
        public ActionResult Register()
        {
            return HandleIsLoggedIn() ?? RegisterActionGet();
        }

        private ActionResult RegisterActionPost(string signature)
        {
            var msgs = new SystemMessages();
            var participationObject = GetParticipationObjectFromCart();

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegisterViewBag(participationObject, msgs);
            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            questions.ForEach(question =>
            {
                if (question.IsRequired)
                {
                    var populated = true;

                    if (question.QuestionTypeId == QuestionType.FileUpload && Request.Files[question.ToFormQuestionViewModel().Name].IsEmpty())
                        populated = false;
                    else if (question.QuestionTypeId != QuestionType.FileUpload && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                        populated = false;

                    if (!populated)
                        msgs.Add($"{question.Text} is required", MessageType.UserError);
                }

            });

            if (signature.IsEmpty())
                msgs.Add("E-signature is required", MessageType.UserError);

            var registration = CreateRegistration(participationObject, qvms, signature, msgs);

            if (msgs.HasNoErrors())
            {
                if (!participationObject.ShouldParentPay())
                    return RedirectToAction("finalizeregistration", new { id = registration.Uid });
                
                return RedirectToAction("payment", new { id = registration.Uid });
            }

            ViewBag.QuestionViewModels = qvms;
            ViewBag.Messages = msgs;

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);

            return View(participationObject);
        }

        [HttpPost]
        public ActionResult Register(string signature)
        {
            return HandleIsLoggedIn() ?? RegisterActionPost(signature);
        }

        private ActionResult FinalizeRegistrationGet(string registrationUid)
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = GetParticipationObjectFromCart();

            // if you can't pay later
            if (!participationObject.AllowToPayLater && participationObject.ShouldParentPay())
                return RedirectToAction("payment", new { id = registrationUid });

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

            selectedDates?.ForEach(date => {
                if (currentRegistrant.IsReservedForDate(participationObject, date, null))
                    datesToRemove.Add(date);
            });

            datesToRemove.ForEach(date => selectedDates.Remove(date));

            if (msgs.HasNoErrors())
                AccountService.ProcessRegistrationWithoutPayment(
                    checkoutObject: new CheckoutObject()
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
                    sendConfirmationEmail: true,
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("thankyou", new { id = registration.Uid });

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

            if (registrantActivity.IsNotEmpty() && registrantActivity.AmountCharged.HasValue && participationObject.Cost > registrantActivity.AmountCharged)
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
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty())
                return results;

            var msgs = new SystemMessages();
            var participationObject = GetParticipationObjectFromCart();
            var selectedDates = GetSelectedDatesFromCart();
            var currentRegistrant = SessionVariables.CurrentRegistrant;

            PaymentViewBag(id, participationObject, msgs);

            if (participationObject.CanRegisterPerClass)
            {
                var registeredForAllClasses = currentRegistrant.IsReservedForAllDates(participationObject, selectedDates, null, msgs);

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

        private ActionResult PaymentActionPost(string id, string stripetoken, string plaidtoken, string plaidaccountid, string ccSourceId, string bankSourceId, string savepaymentinfo, string paymentoption, string couponCode)
        {
            var msgs = new SystemMessages(); 
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var participationObject = GetParticipationObjectFromCart();
            var selectedDates = GetSelectedDatesFromCart();
            var coupon = new Coupon();
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            CheckoutObject checkoutObject = null;

            if (couponCode.IsNotEmpty())
                coupon = SessionVariables.CurrentAccount.ValidateCoupon(participationObject, couponCode, msgs);

            if (ccSourceId.IsEmpty() && stripetoken.IsEmpty() && plaidtoken.IsEmpty() && bankSourceId.IsEmpty() && paymentoption.IsNotEmpty() && paymentoption.ToInteger() >= 0)
                msgs.Add("Please enter your payment information", MessageType.UserError);

            var registration = PaymentViewBag(id, participationObject, msgs);
            var hasWithdrawn = currentRegistrant.HasWithdrawn(participationObject, msgs);

            if (registration.IsAlreadyPaidFor(msgs) && !hasWithdrawn)
                return RedirectToAction("thankyou", new { id = registration.Uid });

            if (msgs.HasNoErrors())
            {
                SubscriptionPlan paymentPlan = null;

                var billingAccount = participationObject.PaymentAccount;
                var payingAccount = SessionVariables.CurrentAccount;
                var convenienceFeePayer = billingAccount.IsConvenienceFeeTakenOutOfCost ? billingAccount : payingAccount;
                var creditCardFeePayer = billingAccount.IsCreditCardFeeTakenOutOfCost ? billingAccount : payingAccount;

                if (participationObject.PostPayPlans.Any())
                    convenienceFeePayer = participationObject.PostPayPlans.First().IsConvenienceFeeTakenOutOfCost ? billingAccount : payingAccount;

                // get original cost
                var submittedRecord = registration.GetRegistrant(msgs);
                var registrantActivity = submittedRecord.GetRegistrantActivity(participationObject, msgs);
                if (registrantActivity.IsNotEmpty() && registrantActivity.AmountCharged.HasValue && participationObject.Cost > registrantActivity.AmountCharged)
                    participationObject.Cost = registrantActivity.AmountCharged;

                BankAccountPurchaseObject bankAccountPurchaseObject = null;
                CreditCardPurchaseObject creditCardPurchaseObject = null;

                if (plaidtoken.IsNotEmpty() || bankSourceId.IsNotEmpty())
                    bankAccountPurchaseObject = new BankAccountPurchaseObject()
                    {
                        PlaidPublicToken = plaidtoken,
                        PlaidAccountId = plaidaccountid,
                        StripeSourceId = bankSourceId                       
                    };
                else if (ccSourceId.IsNotEmpty() || stripetoken.IsNotEmpty())
                    creditCardPurchaseObject = new CreditCardPurchaseObject()
                    {
                        StripeToken = stripetoken,
                        StripeSourceId = ccSourceId
                    };

                if (paymentoption.IsNotEmpty() && paymentoption.IsNumber())
                    paymentPlan = SubscriptionPlan.Load(paymentoption.ToInteger(), msgs);

                var paymentType = PaymentType.NotSet;
                if (bankAccountPurchaseObject.IsNotEmpty())
                    paymentType = PaymentType.ACH;
                else if (creditCardPurchaseObject.IsNotEmpty())
                    paymentType = PaymentType.CreditCard;

                var datesToRemove = new List<DateTime>();

                selectedDates?.ForEach(date => { 
                    if (currentRegistrant.IsReservedForDate(participationObject, date, null))
                        datesToRemove.Add(date);
                });

                datesToRemove.ForEach(date => selectedDates.Remove(date));

                checkoutObject = AccountService.ProcessRegistrationWithPayment(
                    checkoutObject: new CheckoutObject()
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
                        SelectedDates =  selectedDates,
                    },
                    loggedInUser: currentUser,
                    msgs: msgs);

                if (msgs.HasNoErrors())
                {                    
                    var activity = participationObject.GetActivity();
                    var phrase = "";

                    if (participationObject.SubscriptionPlans.IsNotEmpty() && participationObject.SubscriptionPlans.Count > 0)
                    {
                        var plan = participationObject.SubscriptionPlans.FirstOrDefault();
                        var amount = paymentType == PaymentType.ACH ? plan?.GetTotalCostWithConvenienceFee(billingAccount).ToCurrency() : plan?.GetTotalCostWithFees(billingAccount).ToCurrency();
                        phrase = $@"{amount}/{plan.BillingPeriodTypeId} via {(paymentType == PaymentType.ACH ? "Bank Account" : "Credit Card")}";
                    } 
                    else
                    {
                        phrase = $@"{participationObject.GetCheckoutCostWithConvenienceFee().ToCurrency()} via {(paymentType == PaymentType.ACH ? "Bank Account" : "Credit Card")}";
                    }

                    // LOG: Someone Paid amount/billing period via paymentType for participationObjectType participationObjectName
                    // or
                    // LOG: Someone Paid amount via PaymentType for participationObjectType participationObjectName
                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: currentUser,
                        actionType: WebSessionActionType.Paid,
                        objectType: ObjectType.Transaction,
                        objectId: checkoutObject.Transaction?.ID.Value,
                        objectText: phrase,
                        indirectObjectType: participationObject.GetWebSessionObjectType(),
                        indirectObjectId: participationObject.ObjectID.Value,
                        indirectObjectText: activity.Name
                    );

                    return RedirectToAction("thankyou", new { id = registration.Uid });
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Payment(string id, string ccsource, string banksource, string stripetoken, string plaidtoken, string plaidaccountid, string savepaymentinfo, string paymentoption, string coupon)
        {
            return HandleIsLoggedIn() ?? PaymentActionPost(id, stripetoken, plaidtoken, plaidaccountid, ccsource, banksource, savepaymentinfo, paymentoption, coupon);
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
                var currentRegistrant = registration.GetRegistrant(msgs).GetMasterRegistrant(msgs);
                var parentRegistrant = currentRegistrant.GetParentOrGuardian(msgs);
                var parentAccount = parentRegistrant.GetAccount(msgs);

                if (parentAccount.ID != SessionVariables.CurrentAccount.ID)
                    return Logout(Request.Url.ToString());

                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);
                SessionVariables.Cart.AddCartItem(participationObject, null, msgs);
                SessionVariables.CurrentRegistrant = currentRegistrant;
                SessionVariables.RegistrationStartPageAccount = participationObject.OwnerAccount;

                return RedirectToAction("payment", new { id = registration.Uid });
            }

            return RedirectToAction("thankyou", new { id = registration.Uid });
        }

        [HttpGet]
        public ActionResult MakePayment(string id)
        {
            return HandleIsLoggedIn() ?? MakePaymentActionGet(id);
        }

        [HttpPost]
        public ActionResult Pay(string id, string ccsource, string banksource, string stripetoken, string plaidtoken, string plaidaccountid, string savepaymentinfo, string paymentoption, string amount, string paymentdescription)
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
                var description = paymentdescription.IsNotBlank() ? paymentdescription : $"{billingAccount.Name} Payment";

                BankAccountPurchaseObject bankAccountPurchaseObject = null;
                CreditCardPurchaseObject creditCardPurchaseObject = null;
                CheckoutObject checkoutObject = null;

                if (plaidtoken.IsNotEmpty() || banksource.IsNotEmpty())
                {
                    bankAccountPurchaseObject = new BankAccountPurchaseObject()
                    {
                        PlaidPublicToken = plaidtoken,
                        PlaidAccountId = plaidaccountid,
                        StripeSourceId = banksource
                    };
                }
                else if (ccsource.IsNotEmpty() || stripetoken.IsNotEmpty())
                {
                    creditCardPurchaseObject = new CreditCardPurchaseObject()
                    {
                        StripeToken = stripetoken,
                        StripeSourceId = ccsource
                    };

                    ccFee = calcObj.GetCreditCardFee();
                    overrideAmount += ccFee;
                }

                var masterRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant(msgs);
                checkoutObject = AccountService.ProcessPayment(
                    checkoutObject: new CheckoutObject()
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
                    loggedInUser: currentUser,
                    msgs: msgs);

                if (msgs.HasNoErrors())
                {
                    var phrase = $@"{overrideAmount.ToCurrency()} via {(bankAccountPurchaseObject.IsNotEmpty() ? "Bank Account" : "Credit Card")}";

                    // LOG: Someone Paid amount via PaymentType to participationObjectType participationObjectName
                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: currentUser,
                        actionType: WebSessionActionType.Paid,
                        objectType: ObjectType.Transaction,
                        objectId: checkoutObject.Transaction.ID.Value,
                        objectText: phrase,
                        indirectObjectType: ObjectType.Account,
                        indirectObjectId: billingAccount.ID.Value,
                        indirectObjectText: billingAccount.Name
                    );

                    return RedirectToAction("paymentthankyou", new { id = billingAccount.Uid });
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
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGender &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantDateOfBirth &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEthnicity &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantTShirtSize &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantHomePhone &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantWorkPhone
                )
                .ToList();

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

            ViewBag.ParticipationObject = GetParticipationObjectFromCart();

            return questions;
        }

        [HttpGet]
        public ActionResult GetStartedStudent(string regaccount)
        {
            // disable and redirect to get started parent (since there is no way for students to register directly
            return RedirectToAction("getstarted", new { regaccount });

            if (SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            var msgs = new SystemMessages();
            var questions = GetStartedStudentViewBag(msgs);

            ViewBag.Messages = msgs;

            if (regaccount.IsGuid())
                ViewBag.RegistrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult GetStartedStudent(string regaccount, string password1, string password2, string agree, string textnotifications)
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
                var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);
                var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

                WebUser user = null;
                Account account = null;
                Registrant registrant = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create user
                        user = WebUserService.CreateNewWithOptionalPhone(
                            firstName: firstName.Value,
                            lastName: lastName.Value,
                            email: email.Value.Trim(),
                            phone: phone.Value.Trim(),
                            password: password1,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            allowTextMessages: textnotifications.IfBlank("0").ToBoolean(),
                            loggedInUser: null,
                            msgs: amsgs,
                            dc: adc);

                        if (amsgs.HasNoErrors())
                            // create account
                            account = user.AddNewRegistrantAccount(
                                userType: UserType.Student,
                                isActive: true,
                                isAfterSchool: SessionVariables.IsAfterSchool.Value,
                                loggedInUser: user,
                                msgs: amsgs,
                                dc: adc);


                        if (amsgs.HasNoErrors())
                        {
                            var location = participationObject?.Location;
                            if (location.IsNotEmpty() && location.GetHostAccount(amsgs, adc)?.AccountTypeId != AccountType.School)
                                location = null;

                            // create student registrant record
                            registrant = account.CreateRegistrant(
                                registrantOwner: user,
                                canBeAccessedBy: user.AsList(),
                                registrantType: RegistrantType.Individual,
                                isMaster: true,
                                isParentOrGuardian: false,
                                allowTextMessages: false,
                                schoolLocationId: location?.ID, // probably should have a school location id
                                profileStatus: RegistrantProfileStatus.Started,
                                loggedInUser: user,
                                msgs: amsgs,
                                dc: adc);

                            if (amsgs.HasNoErrors())
                                // add answers to student registrant record
                                qvms.ForEach(qvm =>
                                {
                                    registrant.AddOrUpdateAnswer(
                                        answerText: qvm.Value.IfEmpty("").Trim(),
                                        question: qvm.Question,
                                        loggedInUser: user,
                                        msgs: amsgs,
                                        dc: adc);
                                });
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        SessionVariables.CurrentUser = user;
                        SessionVariables.CurrentAccount = account;
                        SessionVariables.AccountUserType = user.GetUserType(account);
                        SessionVariables.CurrentRegistrant = registrant;
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("inviteparent");
            ViewBag.Messages = msgs;
            return View(qvms);
        }

        #endregion

        #region InviteParent

        private List<FormQuestion> InviteParentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGrade &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGender &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantDateOfBirth &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEthnicity &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantTShirtSize &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantHomePhone &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantWorkPhone
                )
                .ToList();

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

            ViewBag.ParticipationObject = GetParticipationObjectFromCart();

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

            var participationObject = GetParticipationObjectFromCart();
            var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
            var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
            var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
            var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

            if (email.Value.IsEmpty())
                msgs.Add("Parent email is required", MessageType.UserError);
            else if (email.Value.Trim().IsSameAs(SessionVariables.CurrentUser.Email.Trim()))
                msgs.Add("Parent email cannot be the same as your student email address when creating a login account", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var location = participationObject?.Location;
                        var hostAccount = location?.GetHostAccount(amsgs, adc);
                        var locationString = "";

                        if (hostAccount.IsNotEmpty() && hostAccount.AccountTypeId != AccountType.School)
                            locationString = $" at {hostAccount.Name}";

                        // invite parent: 
                        var parentUser = SessionVariables.CurrentAccount.InviteUser(
                            firstName: firstName.Value,
                            lastName: lastName.Value,
                            email: email.Value.Trim(),
                            phone: phone.Value.Trim(),
                            message: $"{SessionVariables.CurrentUser.FirstName} has signed up for an account on {AfterSchoolLibrary.Helpers.Utilities.AppName()}{locationString}. Please create your parent {AfterSchoolLibrary.Helpers.Utilities.AppName()} account to know and approve of activities your child requests to participate in.",
                            userType: UserType.Owner,
                            canUseExistingUserAccount: true,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: msgs);

                        // Create parent registrant
                        var parentRegistrant = SessionVariables.CurrentAccount.CreateRegistrant(
                            registrantOwner: parentUser,
                            canBeAccessedBy: parentUser.AsList(),
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: true,
                            allowTextMessages: textnotifications.IfBlank("").ToBoolean(),
                            schoolLocationId: null,
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: msgs,
                            dc: adc);

                        SessionVariables.CurrentRegistrant.AddUserWithAccess(
                            user: parentUser,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: msgs,
                            dc: adc);

                    // add answers to student registrant record
                    qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddOrUpdateAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("RegisterStudent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region RegisterStudent

        [HttpGet]
        public ActionResult RegisterStudent()
        {
            var msgs = new SystemMessages();

            // check if parent has been invited 
            // if not redirect to parent invite page
            if (SessionVariables.CurrentRegistrant.GetParentOrGuardian(msgs).IsEmpty())
                return RedirectToAction("InviteParent");

            var participationObject = GetParticipationObjectFromCart();
            var (submittedRecord, registration) = SessionVariables.CurrentRegistrant.GetSubmittedRecord(participationObject, msgs);

            ViewBag.AlreadyRegistered = submittedRecord.IsNotEmpty();
            ViewBag.Messages = msgs;

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);

            return View(participationObject);
        }

        [HttpPost]
        public ActionResult RegisterStudent(string agree)
        {
            var msgs = new SystemMessages();
            var participationObject = GetParticipationObjectFromCart();
            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form

            if (agree.IsEmpty())
                msgs.Add("You must agree to participate", MessageType.UserError);

            Registration registration = null;

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // account gets copy / snapshot of master registrant record
                        // required in order to make sure registration data doesn't change as registrant changes information in master record.
                        var submittedRecord = SessionVariables.CurrentRegistrant.SubmitRecordToAccount(
                            ownerAccount: participationObject.OwnerAccount,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        // Create Registration
                        registration = submittedRecord.AddRegistration(
                                        ownerAccountId: participationObject.OwnerAccount.ID.Value,
                                        activityGroupId: participationObject.GetAnyRelatedActivityGroupId(),
                                        accountActivityGroupId: participationObject.GetAccountActivityGroupId(),
                                        periodId: participationObject.GetCurrentRegistrationPeriod(amsgs, adc)?.ID,
                                        completionStatus: RegistrationCompletionStatus.RequiresParentSignature,
                                        loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                        msgs: amsgs,
                                        dc: adc);

                        if (amsgs.HasNoErrors())
                            AddSessionActivitiesToRegistration(
                                participationObject,
                                registration,
                                submittedRecord,
                                participationObject.RegistrationPeriod,
                                amsgs,
                                adc);

                        if (msgs.HasNoErrors())
                            AccountService.ProcessStudentRegistration(
                                checkoutObject: new CheckoutObject()
                                {
                                    BillingAccount = participationObject.OwnerAccount,
                                    PayingAccount = SessionVariables.CurrentAccount,
                                    NumberOfSeats = 1,
                                    ParticipationObject = participationObject,
                                    Registration = registration,
                                    MasterRegistrant = SessionVariables.CurrentRegistrant
                                },
                                sendConfirmationEmail: true,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: msgs, dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("ThankYou", new { id = registration.Uid });

            ViewBag.Messages = msgs;

            LoadSessionActivitiesViewBag(participationObject, registration, msgs);

            return View(participationObject);
        }

        #endregion

        #region Helpers
        private Registration CreateRegistration(ParticipationObject participationObject, List<FormQuestionViewModel> qvms, string signature, SystemMessages msgs = null)
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
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        period = participationObject.GetCurrentRegistrationPeriod(amsgs, adc);

                        if (qvms.IsNotEmpty())
                            AddOrUpdateStandardAnswers(
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
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                // LOG: Someone Registered RegistrantName for ParticipationObjectType ParticipationObjectName
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Registered,
                    objectType: ObjectType.Registrant,
                    objectId: currentRegistrant.ID.Value,
                    objectText: currentRegistrant.FullName,
                    indirectObjectType: participationObject.GetWebSessionObjectType(),
                    indirectObjectId: participationObject.ObjectID.Value,
                    indirectObjectText: participationObject.Name
                );

            return registration;
        }

        private ActionResult CheckRedirect(bool checkForEmptyCart = false)
        {            
            if (SessionVariables.RegistrationStartPageAccount.IsNotEmpty())
            {
                var redirectUrl = SessionVariables.RegistrationStartPageAccount.HomePageUrl;

                if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                    return Redirect(redirectUrl);

                if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                    return Redirect(redirectUrl);

                if (checkForEmptyCart && SessionVariables.Cart.IsEmpty())
                    return Redirect(redirectUrl);
            }

            if (SessionVariables.CurrentUser.IsEmpty())
                return Redirect(AfterSchoolLibrary.Helpers.Utilities.LoginUrl());

            if (SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("select", "login");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return HandleRedirectAfterLogin(null);

            if (checkForEmptyCart && SessionVariables.Cart.IsEmpty())
                return HandleRedirectAfterLogin(null);

            return null;
        }

        private WebUser CreateOrUpdateUser(WebUser currentUser, string firstName, string lastName, string email, string phone, string password, bool allowTextMessages, SystemMessages msgs, DatabaseConnection dc = null)
        {
            WebUser user = null;

            if (currentUser.IsEmpty())
                // create user
                user = WebUserService.CreateNew(
                    firstName: firstName,
                    lastName: lastName,
                    email: email.Trim(),
                    phone: phone.Trim(),
                    password: password,
                    isActive: true,
                    isRegistered: true,
                    isVerified: false,
                    allowTextMessages: allowTextMessages,
                    loggedInUser: null,
                    msgs: msgs,
                    dc: dc);
            else
            {
                user = SessionVariables.CurrentUser;
                user.Phone = phone.Trim();
                user.AllowTextMessages = allowTextMessages;
                user.LastModifiedBy = currentUser.ID;
                user.LastModifiedDate = System.DateTime.UtcNow;
                msgs.AddRange(user.Save(dc));
            }

            return user;
        }

        private WebUser CreateOrUpdateInterestedUser(WebUser currentUser, string firstName, string lastName, string email, string phone, SystemMessages msgs, DatabaseConnection dc = null)
        {
            WebUser user;

            if (currentUser.IsEmpty())
            {
                user = WebUserService.StartUserAccount(firstName, lastName, email, msgs, dc);
            }
            else
            {
                user = currentUser;
                user.Phone = phone.Trim();
                user.LastModifiedBy = currentUser.ID;
                user.LastModifiedDate = System.DateTime.UtcNow;
                msgs.AddRange(user.Save(dc));
            }

            return user;
        }

        private Account CreateOrUpdateAccount(WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var account = user.GetRegistrantAccount(msgs, dc);

            // create account
            if (account.IsEmpty())
                account = user.AddNewRegistrantAccount(
                    userType: UserType.Owner,
                    isActive: true,
                    isAfterSchool: true,
                    loggedInUser: user,
                    msgs: msgs,
                    dc: dc);

            return account;
        }

        private Registrant CreateOrUpdateRegistrant(Account account, WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrant = account.GetFirstParentOrGuardianRegistrant(msgs, dc);

            if (registrant.IsEmpty())
                // create parent registrant record
                registrant = account.CreateRegistrant(
                    registrantOwner: user,
                    canBeAccessedBy: user.AsList(),
                    registrantType: RegistrantType.Individual,
                    isMaster: true,
                    isParentOrGuardian: true,
                    allowTextMessages: false,
                    schoolLocationId: null,
                    profileStatus: RegistrantProfileStatus.Started,
                    loggedInUser: user,
                    msgs: msgs,
                    dc: dc);

            return registrant;
        }
        public string GetUserHostAddress()
        {
            if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"].IsNotEmpty())
                return Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0];

            return Request.ServerVariables["REMOTE_ADDR"];
        }

        private void SubmitRecord(ref Registration registration, ref Registrant submittedRecord, Registrant currentRegistrant, ParticipationObject participationObject, WebUser currentUser, RegistrationPeriod period, string signature, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // account gets copy / snapshot of master registrant record
            // required in order to make sure registration data doesn't change as registrant changes information in master record.
            if (submittedRecord.IsEmpty())
            {
                submittedRecord = currentRegistrant.SubmitRecordToAccount(
                                    ownerAccount: participationObject.OwnerAccount,
                                    loggedInUser: currentUser,
                                    msgs: msgs,
                                    dc: dc);

                // create or update registration
                registration = submittedRecord.AddRegistrationWithESignature(
                                ownerAccountId: participationObject.OwnerAccount.ID.Value,
                                activityGroupId: participationObject.GetAnyRelatedActivityGroupId(),
                                accountActivityGroupId: participationObject.GetAccountActivityGroupId(),
                                periodId: period?.ID,
                                esignature: signature,      
                                userAgent: Request.UserAgent.ToString().MaxChars(400),
                                ipAddress: GetUserHostAddress(),
                                loggedInUserId: currentUser.ID.Value,
                                msgs: msgs,
                                dc: dc);
            }
            else
                registration.AddESignature(
                    esignature: signature,
                    userAgent: Request.UserAgent.ToString().MaxChars(400),
                    ipAddress: GetUserHostAddress(),
                    loggedInUser: currentUser,
                    msgs: msgs,
                    dc: dc);

            //copy all answers from the main account to the registrant & registration :)
            currentRegistrant.CopyAnswersTo(
                copyToRegistrant: submittedRecord,
                registration: registration,
                loggedInUser: currentUser,
                msgs: msgs,
                dc: dc);
        }

        private void AddOrUpdateStandardAnswers(List<FormQuestionViewModel> qvms, Registrant currentRegistrant, WebUser currentUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            qvms.Where(qvm => qvm.Question.QuestionDataTypeId != QuestionDataType.NotSet)
                .ToList()
                .ForEach(qvm =>
                {
                    currentRegistrant.AddOrUpdateAnswer(
                        answerText: qvm.Value,
                        question: qvm.Question,
                        loggedInUser: currentUser,
                        msgs: msgs,
                        dc: dc);
                });
        }

        private void AddCustomQuestions(List<FormQuestionViewModel> qvms, Registrant submittedRecord, Registration registration, WebUser currentUser, SystemMessages msgs, DatabaseConnection dc = null)
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
                                file: Request.Files[qvm.Name].InputStream,
                                filename: Request.Files[qvm.Name].FileName,
                                question: qvm.Question,
                                registrationId: registration.ID,
                                loggedInUser: currentUser,
                                msgs: msgs,
                                dc: dc);
                        else
                            msgs.Add("Upload a file less than or equal to 5 megabytes", MessageType.UserError);
                    }
                }
                else
                    submittedRecord.AddOrUpdateAnswer(
                        answerText: qvm.Value,
                        question: qvm.Question,
                        registrationId: registration?.ID,
                        loggedInUser: currentUser,
                        msgs: msgs,
                        dc: dc);
            });
        }

        private (ParticipationObject, RegistrationAssignmentType) GetParticipationObjectFromRegistration(Registration registration, SystemMessages msgs)
        {
            var registrantActivities = registration.GetRegistrantActivities(msgs);
            var assignmentType = RegistrationAssignmentType.NotSet;
            RegistrantActivity registrantActivity = null;

            if (registrantActivities.Count == 1)
                registrantActivity = registrantActivities.First();
            else
                registrantActivity = registrantActivities
                                    .First(x => x.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup);

            assignmentType = registrantActivity.AssignmentTypeId;

            return (registrantActivity.GetParticipationObject(msgs), assignmentType);
        }

        private ParticipationObject GetParticipationObjectFromCart()
        {
            var participationObjects = SessionVariables.Cart?.GetCartItems().Select(cart => cart.GetParticipationObject()).ToList();
            ParticipationObject participationObject = null;

            participationObjects?.ForEach(item => {
                if (item.IsActivityGroup())
                    participationObject = item;
            });

            return participationObject ?? participationObjects?.FirstOrDefault();
        }

        private List<DateTime> GetSelectedDatesFromCart()
        {
            return SessionVariables.Cart?.GetCartItems().FirstOrDefault()?.SelectedDates?.Split(',').Select(x => x.ToDateTime()).ToList();
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

        private Form GetAdministratorAccountForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetDefaultForm(msgs);
        }

        private Form GetInterestForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetOrCreateInterestForm(SessionVariables.CurrentUser, msgs);
        }

        private void AddSessionActivitiesToRegistration(ParticipationObject participationObject, Registration registration, Registrant submittedRecord, RegistrationPeriod period, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // if there is a Session, then a person may set their preferences for activities in that session, 
            // also only if the session is set to Apply
            if (participationObject.IsSession())
            {
                // only get activities that are in the cart
                var participationObjects = GetAllParticipationObjectsForCheckout(registration, msgs, dc);                    

                if (participationObject.RegistrationMode == RegistrationMode.Apply)
                {
                    for (int i = 1; i <= participationObjects.Count; i++)
                    {
                        var postedValue = Request[$"activity-preference-{i}"];
                        if (postedValue.IsGuid())
                        {
                            var po = participationObjects.First(x => x.Uid == postedValue.ToGuid());

                            registration.ProcessRegistrationAssignment(
                                participationObject: po,
                                masterRegistrant: SessionVariables.CurrentRegistrant,
                                registrant: submittedRecord,
                                seatsRequested: 1,
                                preferenceNumber: i,
                                activityGroupId: participationObject.GetActivityGroupId(),
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
                        msgs.Add($"Please enter all of your {UILabel.ActivityLabel(ActivityType.Activity).ToLower()} preferences", MessageType.UserError);
                }
                else
                    participationObjects.ForEach(po => {
                        registration.ProcessRegistrationAssignment(
                                participationObject: po,
                                masterRegistrant: SessionVariables.CurrentRegistrant,
                                registrant: submittedRecord,
                                seatsRequested: 1,
                                activityGroupId: participationObject.GetActivityGroupId(),
                                amountCharged: po.Cost,
                                periodId: period?.ID,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: msgs,
                                dc: dc);});
            }
        }

        private List<ParticipationObject> GetAllParticipationObjectsForCheckout(Registration registration, SystemMessages msgs, DatabaseConnection dc = null)
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

        private List<ParticipationObject> GetCurrentlyRegisteredForParticipationObjects(Registration registration, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ignoredTypes = new List<RegistrationAssignmentType>()
            {
                RegistrationAssignmentType.Declined,
                RegistrationAssignmentType.Withdrawn,
                RegistrationAssignmentType.Invited,
                RegistrationAssignmentType.NotSet
            };

            return registration.GetRegistrantActivities(msgs, dc)
                        .Where(x => x.AssignmentTypeId.NotIn(ignoredTypes))
                        .Where(x => x.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                        .Select(x => x.GetParticipationObject(msgs, dc))
                        .ToList();
        }

        private void LoadSessionActivitiesViewBag(ParticipationObject participationObject, Registration registration, SystemMessages msgs)
        {
            if (participationObject.IsSession() && participationObject.RegistrationMode == RegistrationMode.Apply)
                ViewBag.SessionActivities = GetAllParticipationObjectsForCheckout(registration, msgs);
        }

        #endregion
    }
}