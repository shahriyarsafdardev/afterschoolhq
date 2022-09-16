using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchool.Areas.Volunteer.Controllers
{
    public class ProfileController : BaseLoggedInController
    {
        public ProfileController() : base(new List<AccountType> {AccountType.Volunteer})
        {
        }


        private List<FormQuestion> GetQuestions(SystemMessages msgs, int type)
        {
            var form = FormService.GetAdministratorAccountStudentForm(msgs);

            if (type == 1)
            {
                var questionGroup = form.GetQuestionGroups(msgs)
                    .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
                var questions = questionGroup.GetAllQuestions(msgs);

                questions = questions.Where(question => question.QuestionDataTypeId.NotIn(FormConstants.QuestionsToNotIncludeInParentPanel())).ToList();

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

                return questions;
            }

            if (type == 2)
            {
                var questionGroup = form.GetQuestionGroups(msgs)
                    .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress);
                var questions = questionGroup.GetAllQuestions(msgs);
                var personalContactGroup = form.GetQuestionGroups(msgs)
                    .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);

                var questionsWithPersonalInfo = personalContactGroup.GetAllQuestions(msgs);
                var allQuestionsWithPersonalInfo = questionsWithPersonalInfo.Where(question => question.QuestionDataTypeId.In(new List<QuestionDataType>
                    {
                        QuestionDataType.RegistrantHomePhone,
                        QuestionDataType.RegistrantWorkPhone,
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

            if (type == 3)
            {
                var questionGroup = form.GetQuestionGroups(msgs)
                    .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 &&
                                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2); //FirstOrDefault but then null handling?
                var questions = questionGroup.GetAllQuestions(msgs);

                questions = questions.Where(que =>
                    que.QuestionDataTypeId != QuestionDataType.EmergencyContact1RelationshipToRegistrant &&
                    que.QuestionDataTypeId != QuestionDataType.EmergencyContact2RelationshipToRegistrant).ToList();

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

            return null;
        }


        // Need functions for 
        // 1- To validate User Information (Personal Information)
        // 2- To Update User Information to Database. 
        // 3- To update Questions information (Emergency Contact)
        // 4- Add disclaimer text for passwords and personal information. 
        // 5- Do CSS Styling. 

        [HttpGet]
        public ActionResult EditPersonal(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return Redirect(Request.Url.AbsoluteUri);

                var peronalQuestions = GetQuestions(msgs, 1);
                var addressQuestions = GetQuestions(msgs, 2);
                var emergencyQuestions = FormQuestionService.GetEmergencyContactQuestions(msgs);

                var values = new NameValueCollection();
                peronalQuestions.ForEach(question =>
                {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                addressQuestions.ForEach(question =>
                {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });
                emergencyQuestions.ForEach(question =>
                {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.peronalQuestions = peronalQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.addressQuestions = addressQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.emergencyQuestions = emergencyQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditPersonal(string id, FormCollection formCollection)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;
            WebUser user = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetQuestions(msgs, 1);
                var emergencyContactQuestions = FormQuestionService.GetEmergencyContactQuestions(msgs);

                // This could be potentially refactored.
                foreach (var ques in GetQuestions(msgs, 2))
                {
                    questions.Add(ques);
                }

                foreach (var qvm in emergencyContactQuestions.Where(qvm =>
                             qvm.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1))
                {
                    if (qvm.QuestionDataTypeId != QuestionDataType.EmergencyContact1RelationshipToRegistrant)
                    {
                        questions.Add(qvm);
                    }
                }

                foreach (var qvm in emergencyContactQuestions.Where(qvm =>
                             qvm.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2))
                {
                    if (qvm.QuestionDataTypeId != QuestionDataType.EmergencyContact1RelationshipToRegistrant)
                    {
                        questions.Add(qvm);
                    }
                }


                // Validation
                questions.ForEach(question =>
                {
                    var answer = Request[question.ToFormQuestionViewModel().Name];

                    if (question.IsRequired && answer.IsEmpty())
                        msgs.Add($"{question.Text} is required", MessageType.UserError);

                    switch (question.QuestionDataTypeId)
                    {
                        case QuestionDataType.RegistrantMobilePhone:
                            FormService.PhoneValidation(answer, question, msgs);
                            break;
                        case QuestionDataType.RegistrantDoctorPhone:
                            if (answer.IsNotEmpty() && !answer.IsRegExMatch(Utilities.PhoneRegEx))
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
                            registrant.LastModifiedBy = SessionVariables.CurrentUser.ID;
                            registrant.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(registrant.Save(adc));

                            qvms.ForEach(qvm =>
                            {
                                registrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });

                            user = WebUser.LoadByEmail(formCollection["email"], amsgs, adc);
                            user.FirstName = formCollection["question-4230"].MaxChars(50);
                            user.LastName = formCollection["question-4231"].MaxChars(50);
                            user.Email = formCollection["question-4231"].MaxChars(50);
                            user.Phone = formCollection["question-4231"].MaxChars(50);
                            user.Phone = formCollection["question-4231"].MaxChars(50);
                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }
            }

            return Redirect(Request.Url.AbsoluteUri);
        }
    }
}