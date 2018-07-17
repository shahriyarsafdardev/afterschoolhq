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
    public class AccountController : Controller
    {
        private Form GetAdministratorAccountForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetDefaultForm(msgs);
        }

        private List<FormQuestion> GetStartedViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var parentQuestionGroup = form.GetQuestionGroups(msgs).Where(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information).First();
            var questions = parentQuestionGroup.GetAllQuestions(msgs);

            questions.ForEach(question => {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.Parent1FirstName:
                    case QuestionDataType.Parent1LastName:
                    case QuestionDataType.Parent1Email:
                    case QuestionDataType.Parent1MobilePhone:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        // GET: Account
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
            if (password1 == password2)
                msgs.Add("Passwords must match", MessageType.UserError);
            if (agree.IsBoolean() && agree.ToBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var firstName = questions.Where(question => question.QuestionDataTypeId == QuestionDataType.Parent1FirstName).First();
                var lastName = questions.Where(question => question.QuestionDataTypeId == QuestionDataType.Parent1LastName).First();
                var email = questions.Where(question => question.QuestionDataTypeId == QuestionDataType.Parent1Email).First();
                var phone = questions.Where(question => question.QuestionDataTypeId == QuestionDataType.Parent1MobilePhone).First();

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create user
                        var user = WebUserService.CreateNew(
                            firstName: Request[firstName.ToFormQuestionViewModel().Name],
                            lastName: Request[lastName.ToFormQuestionViewModel().Name],
                            email: Request[email.ToFormQuestionViewModel().Name],
                            phone: Request[phone.ToFormQuestionViewModel().Name],
                            password: password1,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            loggedInUser: null,
                            msgs: amsgs,
                            dc: adc);

                        // create account
                        var account = user.AddNewRegistrantAccount(
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
                        questions.ForEach(question =>
                        {
                            registrant.AddAnswer(
                                answerText: Request[question.ToFormQuestionViewModel().Name], 
                                questionDataType: question.QuestionDataTypeId, 
                                loggedInUserId: user.ID.Value, 
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

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpGet]
        public ActionResult AddStudent()
        {

            return View();
        }

        [HttpGet]
        public ActionResult Select()
        {

            return View();
        }
    }
}