using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using System.Linq;
using System.Collections.Specialized;
using AfterSchool.Models;
using System.Collections.Generic;
using AfterSchoolLibrary.Services;
using BasicLibrary.DataAccess;
using System;

namespace AfterSchool.Controllers
{
    public class AccountController : BaseController
    {

        #region GetStarted

        private List<FormQuestion> GetStartedViewBag(string regaccount, SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs).Where(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation).First();
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
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;
            

            return questions;
        }
        
        [HttpGet]
        public ActionResult GetStarted(string regaccount)
        {
            if (SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            var msgs = new SystemMessages();
            var questions = GetStartedViewBag(regaccount, msgs);

            ViewBag.Messages = msgs;

            if (regaccount.IsGuid())
                ViewBag.RegistrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult GetStarted(string regaccount, string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();
            var questions = GetStartedViewBag(regaccount, msgs);

            questions.ForEach(question => {
                if (question.IsRequired && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            if (password1.IsEmpty())
                msgs.Add("Password 1 is required", MessageType.UserError);
            if (password2.IsEmpty())
                msgs.Add("Password 2 is required", MessageType.UserError);
            if (password1 != password2)
                msgs.Add("Passwords must match", MessageType.UserError);
            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {

                var firstName = qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).First();
                var lastName = qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).First();
                var email = qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).First();
                var phone = qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).First();
                WebUser user = null;
                Account account = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create user
                        user = WebUserService.CreateNew(
                            firstName: firstName.Value,
                            lastName: lastName.Value,
                            email: email.Value,
                            phone: phone.Value,
                            password: password1,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            allowTextMessages: textnotifications.IfBlank("0").ToBoolean(),
                            loggedInUser: null,
                            msgs: amsgs,
                            dc: adc);

                        // create account
                        account = user.AddNewRegistrantAccount(
                            isActive: true,
                            isAfterSchool: SessionVariables.IsAfterSchool.Value,
                            loggedInUser: user,
                            msgs: amsgs,
                            dc: adc);

                        // create parent registrant record
                        var registrant = account.CreateRegistrant(
                            associatedUser: user,
                            registrantType: RegistrantType.Individual, 
                            isMaster: true, 
                            isParentOrGuardian: true,
                            allowTextMessages: textnotifications.IfBlank("0").ToBoolean(),
                            loggedInUser: user,
                            msgs: amsgs, 
                            dc: adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            registrant.AddAnswer(
                                answerText: qvm.Value, 
                                question: qvm.Question, 
                                loggedInUserId: user.ID.Value, 
                                msgs: amsgs, 
                                dc: adc);
                        });                        
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        SessionVariables.LoggedInUser = user;
                        SessionVariables.CurrentAccount = account;
                        SessionVariables.AccountUserType = user.GetUserType(account);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
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
                .Where(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress)
                .First();
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

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult AddHomeAddress()
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

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

            questions.ForEach(question => {
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
                            parentRegistrant.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
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
            var emergencyContactsQuestionGroup = form.GetQuestionGroups(msgs)
                .Where(group => 
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 || 
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2)
                .First();
            var questions = emergencyContactsQuestionGroup.GetAllQuestions(msgs);

            questions.ForEach(question => { question.IsRequired = true; });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult AddEmergencyContacts()
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

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

            questions.ForEach(question => {
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
                            parentRegistrant.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addstudent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddStudent

        private List<FormQuestion> AddStudentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var studentQuestionGroup = form.GetQuestionGroups(msgs)
                .Where(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation)
                .First();
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
                        case QuestionDataType.RegistrantTShirtSize:
                            question.IsRequired = true;
                            break;
                        case QuestionDataType.RegistrantHomePhone:
                        case QuestionDataType.RegistrantEmail:
                            question.IsRequired = false;
                            break;
                    }
                });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult AddStudent()
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
            var parentOrGuardian = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var questions = AddStudentViewBag(msgs);
            var values = new NameValueCollection();

            questions.ForEach(question => {
                switch(question.QuestionDataTypeId)
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
        public ActionResult AddStudent(string id, string samehomeaddress)
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant(msgs);
            var questions = AddStudentViewBag(msgs);

            questions.ForEach(question => {
                if (question.IsRequired && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Registrant student = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create student registrant record
                        student = SessionVariables.CurrentAccount.CreateRegistrant(
                            associatedUser: null,
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: false,
                            allowTextMessages: false,
                            loggedInUser: SessionVariables.LoggedInUser,
                            msgs: amsgs,
                            dc: adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            student.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
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
                                student.AddAnswer(
                                    answerText: answer.Text,
                                    question: answer.ToFormQuestion(msgs: amsgs, dc: adc),
                                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                                    msgs: amsgs,
                                    dc: adc
                                    );
                            });
                        
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        SessionVariables.CurrentRegistrant = student;
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addhealth");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region HealthView

        private List<FormQuestion> AddHealthViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var healthQuestionGroup = form.GetQuestionGroups(msgs)
                .Where(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HealthInformation)
                .First();
            var questions = healthQuestionGroup.GetAllQuestions(msgs);

            questions.ForEach(question => {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantDoctorName:
                    case QuestionDataType.RegistrantDoctorPhone:
                    case QuestionDataType.RegistrantAllergies:
                    case QuestionDataType.RegistrantMedicalConditions:
                    case QuestionDataType.RegistrantMedications:
                    case QuestionDataType.RegistrantImmunizations:
                    case QuestionDataType.RegistrantPreferredHospital:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult AddHealth()
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
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
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
            var questions = AddHealthViewBag(msgs);

            questions.ForEach(question => {
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
                            SessionVariables.CurrentRegistrant.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("select");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region SelectStudent

        [HttpGet]
        public ActionResult Select()
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            if (SessionVariables.CurrentRegistrant.IsNotEmpty())
                return RedirectToAction("register", "account");

            var msgs = new SystemMessages();
            var students = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs);
            
            ViewBag.Messages = msgs;

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

            var requiredQuestions = participationObject.Form
                .GetActiveQuestions(msgs)
                .Where(question => question.IsRequired && question.FormQuestionGroupRegistrantTypeId == RegistrantType.Individual)
                .ToList();

            var answers = SessionVariables.CurrentRegistrant.GetAnswers(msgs);

            var questions = requiredQuestions.Where(question =>
                    answers.Find(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).IsEmpty()
                )
                .Distinct()
                .ToList();

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        private ActionResult RegisterActionGet()
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();

            if (participationObject.IsEmpty())
                return RedirectToAction(""); // TODO: Should return to school homepage, they don't have an item in the cart

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form

            var questions = RegisterViewBag(participationObject, msgs);

            ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();
            ViewBag.Messages = msgs;

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
            var participationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();

            if (participationObject.IsEmpty())
                return RedirectToAction(""); // TODO: Should return to school homepage, they don't have an item in the cart

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegisterViewBag(participationObject, msgs);
            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            questions.ForEach(question => {
                if (question.IsRequired && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            if (signature.IsEmpty())
                msgs.Add("E-signature is required", MessageType.UserError);

            Registration registration = null;

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // account gets copy of master registrant record
                        var accountRegistrantCopy = SessionVariables.CurrentRegistrant.CopyToAccount(
                            ownerAccount: participationObject.OwnerAccount,
                            loggedInUser: SessionVariables.LoggedInUser,
                            msgs: amsgs,
                            dc: adc
                            );

                        // create registration
                        registration = accountRegistrantCopy.AddRegistrationWithESignature(
                            ownerAccountId: participationObject.OwnerAccount.ID.Value,
                            activityGroupId: participationObject.ObjectType == ParticipationObjectType.Activity ? ((Activity)participationObject.Object).ActivityGroupId : null,
                            esignature: signature,
                            loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("payment", new { id = registration.Uid });

            ViewBag.QuestionViewModels = qvms;
            ViewBag.Messages = msgs;

            return View(participationObject);
        }

        [HttpPost]
        public ActionResult Register(string signature)
        {
            return HandleIsLoggedIn() ?? RegisterActionPost(signature);
        }

        #endregion

        #region Payment

        private Registration PaymentViewBag(string id, ParticipationObject participationObject, SystemMessages msgs)
        {
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            ViewBag.Registration = registration;
            ViewBag.Cards = SessionVariables.CurrentAccount.GetStripeCards(msgs);
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.Messages = msgs;

            return registration;
        }

        private ActionResult PaymentActionGet(string id)
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);

            if (participationObject.IsEmpty())
                return RedirectToAction(""); // TODO: Should return to school homepage, they don't have an item in the cart

            PaymentViewBag(id, participationObject, msgs);

            return View();
        }

        [HttpGet]
        public ActionResult Payment(string id)
        {
            return HandleIsLoggedIn() ?? PaymentActionGet(id);
        }

        private ActionResult PaymentActionPost(string id, string stripetoken, string cardId, string savepaymentinfo)
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);

            if (participationObject.IsEmpty())
                return RedirectToAction(""); // TODO: Should return to school homepage, they don't have an item in the cart

            if (cardId.IsEmpty() && stripetoken.IsEmpty())
                msgs.Add("Please enter your payment information", MessageType.UserError);

            var registration = PaymentViewBag(id, participationObject, msgs);

            if (msgs.HasNoErrors())
            {
                SessionVariables.CurrentAccount.ProcessRegistrationWithPayment(
                    toAccount: participationObject.OwnerAccount,
                    purchaseObject: new PurchaseObject()
                    {
                        Coupon = null,
                        NumberOfSeats = 1,
                        ParticipationObject = participationObject,
                        PaymentPlan = null,
                        Products = new List<Product>(),
                        Purchaser = SessionVariables.CurrentAccount,
                        Registration = registration,
                        StripeTokenOrSourceId = stripetoken.IfEmpty(cardId),
                        SubscriptionPlan = null,
                        SavePaymentInfo = savepaymentinfo.IfEmpty("0").ToBoolean(),
                        Registrant = SessionVariables.CurrentRegistrant
                    },
                    loggedInUser: SessionVariables.LoggedInUser,
                    msgs: msgs);
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("thankyou", new { id = registration.Uid });

            return View();
        }

        [HttpPost]
        public ActionResult Payment(string id, string card, string stripetoken, string savepaymentinfo)
        {
            return HandleIsLoggedIn() ?? PaymentActionPost(id, stripetoken, card, savepaymentinfo);
        }

        #endregion

        #region ThankYou

        private Registration ThankYouViewBag(string registrationUid, ParticipationObject participationObject, SystemMessages msgs)
        {
            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);

            ViewBag.Registration = registration;
            ViewBag.Account = registration.GetAccount();
            ViewBag.Cards = SessionVariables.CurrentAccount.GetStripeCards(msgs);
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.Messages = msgs;

            return registration;
        }

        private ActionResult ThankYouActionGet(string registrationUid)
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);

            ThankYouViewBag(registrationUid, participationObject, msgs);

            return View();
        }

        [HttpGet]
        public ActionResult ThankYou(string id)
        {
            return HandleIsLoggedIn() ?? ThankYouActionGet(id);
        }

        #endregion

        #region Helpers

        private ActionResult HandleIsLoggedIn()
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            return null;
        }

        private Form GetAdministratorAccountForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetDefaultForm(msgs);
        }

        #endregion
    }
}