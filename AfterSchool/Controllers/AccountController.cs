using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using System.Linq;
using System.Collections.Specialized;
using AfterSchool.Models;
using System.Collections.Generic;
using AfterSchoolLibrary.Services;
using BasicLibrary.DataAccess;

namespace AfterSchool.Controllers
{
    public class AccountController : BaseController
    {
        private Form GetAdministratorAccountForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetDefaultForm(msgs);
        }

        private List<FormQuestion> GetStartedViewBag(SystemMessages msgs)
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
        public ActionResult GetStarted()
        {
            if (SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            var msgs = new SystemMessages();
            var questions = GetStartedViewBag(msgs);

            ViewBag.Messages = msgs;

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult GetStarted(string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();
            var questions = GetStartedViewBag(msgs);

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
                        var registrant = user.CreateRegistrant(
                            registrantType: RegistrantType.Individual, 
                            isMaster: true, 
                            isParentOrGuardian: true, 
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
                return RedirectToAction("addstudent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        private List<FormQuestion> AddStudentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var studentQuestionGroup = form.GetQuestionGroups(msgs)
                .Where(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation)
                .First();
            var questions = studentQuestionGroup.GetAllQuestions(msgs);

            questions = questions.Where(question => question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization)
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
            var parentOrGuardian = SessionVariables.LoggedInUser.GetParentOrGuardianRegistrant();
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
        public ActionResult AddStudent(string id)
        {
            if (SessionVariables.LoggedInUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
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
                        student = SessionVariables.LoggedInUser.CreateRegistrant(
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: false,
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
                return RedirectToAction("AddHealth");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

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

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpGet]
        public ActionResult Select()
        {

            return View();
        }
    }
}