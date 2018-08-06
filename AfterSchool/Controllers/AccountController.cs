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
using Hangfire;

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
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch(question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
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
                            schoolLocationId: null,
                            loggedInUser: user,
                            msgs: amsgs, 
                            dc: adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            registrant.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(), 
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
                                answerText: qvm.Value.IfEmpty("").Trim(),
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
                return RedirectToAction("addparent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddParent

        private List<FormQuestion> AddParentViewBag(SystemMessages msgs)
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
                    // case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

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

            questions.ForEach(question => {
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
        public ActionResult AddParent(string id)
        {
            var msgs = new SystemMessages();
            var questions = AddParentViewBag(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();

            questions.ForEach(question => {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Registrant otherParent = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create parent registrant record
                        otherParent = SessionVariables.CurrentAccount.CreateRegistrant(
                            associatedUser: null,
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: true,
                            allowTextMessages: false,
                            schoolLocationId: null,
                            loggedInUser: SessionVariables.LoggedInUser,
                            msgs: amsgs,
                            dc: adc);

                        qvms.ForEach(qvm =>
                        {
                            otherParent.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
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

            ViewBag.Cart = SessionVariables.Cart;

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

            questions.ForEach(question => {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
                
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.EmergencyContact1Phone:
                    case QuestionDataType.EmergencyContact2Phone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
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
                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
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

        private List<FormQuestion> AddStudentViewBag(Form administratorAccountForm, ParticipationObject participationObject, SystemMessages msgs)
        {            
            var studentQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs)
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

            var account = participationObject.OwnerAccount;
            var location = participationObject.Location;
            //var schools = new List<Location>();

            //if (location.IsNotEmpty())
            //    schools = Location.LoadByLocationTypeIdByCityByState(LocationType.School, location?.City, location?.State, msgs);

            ViewBag.Account = account;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.School = new Location();
            ViewBag.SchoolLocation = SessionVariables.CurrentAccount.GetDefaultLocation();

            return questions;
        }

        [HttpGet]
        public ActionResult AddStudent()
        {
            var results = CheckRedirect();
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var adminAccountForm = GetAdministratorAccountForm(msgs);
            var parentOrGuardian = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).First().GetParticipationObject(msgs);
            var questions = AddStudentViewBag(adminAccountForm, participationObject, msgs);
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
        public ActionResult AddStudent(string id, string samehomeaddress, string location)
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var adminAccountForm = GetAdministratorAccountForm(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant(msgs);
            var parentRegistrant2 = SessionVariables.CurrentAccount.GetSecondParentOrGuardianRegistrant(msgs);
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).First().GetParticipationObject(msgs);
            var questions = AddStudentViewBag(adminAccountForm, participationObject, msgs);

            questions.ForEach(question => {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
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
                            schoolLocationId: location.IsNumber() ? location.ToInteger() : new int?(),
                            loggedInUser: SessionVariables.LoggedInUser,
                            msgs: amsgs,
                            dc: adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            student.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
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
                                    answerText: answer.Text.IfEmpty("").Trim(),
                                    question: answer.ToFormQuestion(msgs: amsgs, dc: adc),
                                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
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

                        foreach(var key in parent1Map.Keys)
                        {
                            student.MapAndAddAnswer(
                               allquestions,
                               parentRegistrant,
                               key,
                               parent1Map[key],
                               SessionVariables.LoggedInUser,
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
                                   SessionVariables.LoggedInUser,
                                   amsgs,
                                   adc);
                            }
                        }
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
                    // case QuestionDataType.RegistrantDoctorPhone:
                    // case QuestionDataType.RegistrantAllergies:
                    // case QuestionDataType.RegistrantMedicalConditions:
                    // case QuestionDataType.RegistrantMedications:
                    // case QuestionDataType.RegistrantImmunizations:
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
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantDoctorPhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
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
                                answerText: qvm.Value.IfEmpty("").Trim(),
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
            var students = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs);
            
            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();

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
                    answers.Find(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).IsEmpty()
                )
                .Distinct()
                .ToList();

            questions.AddRange(
                    allQuestions
                        .Where(question => question.QuestionDataTypeId == QuestionDataType.NotSet)
                        .Where(question => answers.Find(answer => answer.FormQuestionId == question.ID).IsEmpty())
                );

            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Registrant = SessionVariables.CurrentRegistrant;

            return questions;
        }

        private ActionResult RegisterActionGet()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();

            // current registrant is already registered
            if (SessionVariables.CurrentRegistrant.IsRegisteredFor(participationObject, msgs))
                return RedirectToAction("select");

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
                        // add standardized questions that the student just now answered back to the main profile
                        // that means no registration id
                        qvms.Where(qvm => qvm.Question.QuestionDataTypeId != QuestionDataType.NotSet)
                        .ToList()
                        .ForEach(qvm =>
                        {
                            SessionVariables.CurrentRegistrant.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });

                        // account gets copy / snapshot of master registrant record
                        // required in order to make sure registration data doesn't change as registrant changes information in master record.
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
                            completionStatus: RegistrationCompletionStatus.Started,
                            loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                            msgs: amsgs,
                            dc: adc);                        

                        //copy all answers from the main account to the registrant & registration :)
                        SessionVariables.CurrentRegistrant.CopyAnswers(
                            copyingRegistrant: accountRegistrantCopy, 
                            registration: registration, 
                            loggedInUser: SessionVariables.LoggedInUser, 
                            msgs: amsgs, 
                            dc: adc);

                        // add custom answers to questions directly to this registration
                        qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.NotSet)
                        .ToList()
                        .ForEach(qvm =>
                        {
                            accountRegistrantCopy.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                registrationId: registration.ID,
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
            {
                if (participationObject.IsFree || participationObject.RegistrationMode == RegistrationMode.Apply)
                    return RedirectToAction("finalizeregistration", new { id = registration.Uid });
                else
                    return RedirectToAction("payment", new { id = registration.Uid });
            }

            ViewBag.QuestionViewModels = qvms;
            ViewBag.Messages = msgs;

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
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);

            // if you can't pay later
            if (!participationObject.AllowToPayLater)
            {
                // if it's not free and registration mode is not set to apply mode
                // if it costs something and you're forced to register then you need to pay
                if (!participationObject.IsFree && participationObject.RegistrationMode == RegistrationMode.Register)
                    return RedirectToAction("payment", new { id = registrationUid });
            }

            // need to actually register first :)
            if (!registrationUid.IsGuid())
                return RedirectToAction("register");

            // current registrant is already registered
            if (SessionVariables.CurrentRegistrant.IsRegisteredFor(participationObject, msgs))
                return RedirectToAction("select");

            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);            

            if (msgs.HasNoErrors())
            {
                SessionVariables.CurrentAccount.ProcessRegistrationWithoutPayment(
                    toAccount: participationObject.OwnerAccount,
                    checkoutObject: new CheckoutObject()
                    {
                        NumberOfSeats = 1,
                        ParticipationObject = participationObject,
                        Purchaser = SessionVariables.CurrentAccount,
                        Registration = registration,
                        Registrant = SessionVariables.CurrentRegistrant
                    },
                    sendConfirmationEmail: true,
                    loggedInUser: SessionVariables.LoggedInUser,
                    msgs: msgs);                
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("thankyou", new { id = registration.Uid });

            return View();
        }

        [HttpGet]
        public ActionResult FinalizeRegistration(string id)
        {
            return HandleIsLoggedIn() ?? FinalizeRegistrationGet(id);
        }

        #endregion

        #region Payment

        private Registration PaymentViewBag(string id, ParticipationObject participationObject, SystemMessages msgs)
        {
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            ViewBag.Registration = registration;
            ViewBag.Sources = SessionVariables.CurrentAccount.GetStripeSources(msgs);
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.Messages = msgs;

            return registration;
        }

        private ActionResult PaymentActionGet(string id)
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);
            var registration = PaymentViewBag(id, participationObject, msgs);            

            // current registrant is already registered
            if (SessionVariables.CurrentRegistrant.IsRegisteredFor(participationObject, msgs) && registration.IsPaid(msgs))
                return RedirectToAction("select");

            return View();
        }

        [HttpGet]
        public ActionResult Payment(string id)
        {
            return HandleIsLoggedIn() ?? PaymentActionGet(id);
        }

        private ActionResult PaymentActionPost(string id, string stripetoken, string sourceId, string savepaymentinfo, string paymentoption)
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);

            if (sourceId.IsEmpty() && stripetoken.IsEmpty())
                msgs.Add("Please enter your payment information", MessageType.UserError);

            var registration = PaymentViewBag(id, participationObject, msgs);

            if (msgs.HasNoErrors())
            {
                SubscriptionPlan paymentPlan = null;

                if (paymentoption.IsNotEmpty() && paymentoption.IsNumber())
                    paymentPlan = SubscriptionPlan.Load(paymentoption.ToInteger(), msgs);                

                SessionVariables.CurrentAccount.ProcessRegistrationWithPayment(
                    toAccount: participationObject.OwnerAccount,
                    checkoutObject: new CheckoutObject()
                    {
                        Coupon = null,
                        NumberOfSeats = 1,
                        ParticipationObject = participationObject,
                        PaymentPlan = paymentPlan,
                        SubscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault(),
                        Products = new List<Product>(),
                        Purchaser = SessionVariables.CurrentAccount,
                        Registration = registration,
                        StripeToken = stripetoken,
                        SourceId = sourceId,
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
        public ActionResult Payment(string id, string source, string stripetoken, string savepaymentinfo, string paymentoption)
        {
            return HandleIsLoggedIn() ?? PaymentActionPost(id, stripetoken, source, savepaymentinfo, paymentoption);
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
                var participationObject = registration.GetParticipationObject(msgs);
                var currentRegistrant = registration.GetRegistrant(msgs).GetMasterRegistrant(msgs);
                var parentRegistrant = currentRegistrant.GetParentOrGuardian(msgs);
                var parentAccount = parentRegistrant.GetAccount(msgs);

                if (parentAccount.ID != SessionVariables.CurrentAccount.ID)
                    return Logout(Request.Url.ToString());

                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.LoggedInUser, msgs: msgs);
                SessionVariables.Cart.AddCartItem(participationObject, msgs);
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

        #endregion

        #region ThankYou

        private Registration ThankYouViewBag(string registrationUid, SystemMessages msgs)
        {
            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);

            // this should be moved into the library
            //if (SessionVariables.Cart.IsNotEmpty())
            //    BackgroundJob.Enqueue(() => Registration.SendConfirmation(registration.ID.Value, true, null));

            // clear out session stuff after it sends confirmation, don't want to send on refresh
            ResetRegistrationSession();

            ViewBag.Registration = registration;
            ViewBag.Account = SessionVariables.RegistrationStartPageAccount;
            ViewBag.Messages = msgs;

            return registration;
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

        #endregion

        #region Helpers

        private ActionResult CheckRedirect(bool checkForEmptyCart = false)
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction(SessionVariables.RegistrationStartPageAccount.HomePageUrl);

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return Redirect(SessionVariables.RegistrationStartPageAccount.HomePageUrl);

            if (checkForEmptyCart && SessionVariables.Cart.IsEmpty())
                return RedirectToAction(SessionVariables.RegistrationStartPageAccount.HomePageUrl);

            return null;
        }

        private void ResetRegistrationSession()
        {
            SessionVariables.CurrentRegistrant = null;
            SessionVariables.Cart = null;
        }

        private ActionResult HandleIsLoggedIn()
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("index", "login", new { returnurl = Request.Url.ToString() });

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