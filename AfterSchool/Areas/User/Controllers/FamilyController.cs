using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.User.Controllers
{
    public class FamilyController : BaseLoggedInController
    {
        public FamilyController() : base(new List<AccountType>() { AccountType.Registrant })
        {

        }

        // GET: User/Family
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var registrants = SessionVariables.CurrentUser.GetAllRegistrants(msgs);

            return View(registrants);
        }

        private List<FormQuestion> GetPersonalQuestions(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var studentQuestionGroup = form.GetQuestionGroups(msgs)
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
                    case QuestionDataType.RegistrantTShirtSize:
                        question.IsRequired = true;
                        break;
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantEmail:
                        question.IsRequired = false;
                        break;
                }
            });

            return questions;
        }

        [HttpGet]
        public ActionResult EditStudentPersonal(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetPersonalQuestions(msgs);
                
                var values = new NameValueCollection();
                questions.ForEach(question => {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.School = registrant.GetSchoolLocation(msgs);
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditStudentPersonal(string id, string location)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetPersonalQuestions(msgs);

                // Validation
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
                            registrant.SchoolLocationId = location.IsNumber() ? location.ToInteger() : new int?();
                            registrant.LastModifiedBy = SessionVariables.CurrentUser.ID;
                            registrant.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(registrant.Save(adc));

                            qvms.ForEach(qvm =>
                            {
                                registrant.AddAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }

                ViewBag.QuestionViewModels = qvms;
                ViewBag.School = registrant.GetSchoolLocation(msgs);
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("EditHealth", new { id = registrant.Uid });

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        private List<FormQuestion> GetHealthQuestions(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var questionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HealthInformation);
            var questions = questionGroup.GetAllQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            return questions;
        }

        [HttpGet]
        public ActionResult EditHealth(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetHealthQuestions(msgs);

                var values = new NameValueCollection();
                questions.ForEach(question => {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.School = registrant.GetSchoolLocation(msgs);
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditHealth(string id, string what)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetHealthQuestions(msgs);

                // Validation
                questions.ForEach(question =>
                {
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
                            registrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                            qvms.ForEach(qvm =>
                            {
                                registrant.AddAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }

                ViewBag.QuestionViewModels = qvms;
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("EditProfileDone", new { id = registrant.Uid });

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpGet]
        public ActionResult EditProfileDone(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        private Form GetAdministratorAccountForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetDefaultForm(msgs);
        }
    }
}