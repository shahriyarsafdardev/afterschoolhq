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
                            question.IsRequired = true;
                            break;
                        case QuestionDataType.RegistrantMobilePhone:
                        case QuestionDataType.RegistrantEmail:
                            question.IsRequired = false;
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

        [HttpGet]
        public ActionResult EditPersonal(string id)
        {
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return Redirect(Request.Url.AbsoluteUri);

                var personalQuestions = GetQuestions(msgs, 1);
                var addressQuestions = GetQuestions(msgs, 2);
                var emergencyQuestions = FormQuestionService.GetEmergencyContactQuestions(msgs);

                var values = new NameValueCollection();
                personalQuestions.ForEach(question =>
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

                ViewBag.personalQuestions = personalQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.addressQuestions = addressQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.emergencyQuestions = emergencyQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            }

            ViewBag.msgs = msgs;
            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditPersonal(string id, FormCollection formCollection)
        {
            var msgs = new SystemMessages();
            Registrant registrant = new Registrant();
            var currentUser = SessionVariables.CurrentUser;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!currentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");
                
                var questions = new List<FormQuestion>();

                var personalQuestions = GetQuestions(msgs, 1);
                var addressQuestions = GetQuestions(msgs, 2);

                var emergencyQuestions = FormQuestionService.GetEmergencyContactQuestions(msgs);

                foreach (var ques in personalQuestions)
                {
                    questions.Add(ques);
                }
                
                foreach (var ques in addressQuestions)
                {
                    questions.Add(ques);
                }

                foreach (var qvm in emergencyQuestions.Where(qvm =>
                             qvm.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1))
                {
                    if (qvm.QuestionDataTypeId != QuestionDataType.EmergencyContact1RelationshipToRegistrant)
                    {
                        questions.Add(qvm);
                    }
                }

                foreach (var qvm in emergencyQuestions.Where(qvm =>
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
                            registrant.FirstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            registrant.MiddleName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMiddleName).Value;
                            registrant.LastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
                            registrant.LastModifiedBy = currentUser.ID;
                            registrant.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(registrant.Save(adc));

                            qvms.ForEach(qvm =>
                            {
                                registrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUser: currentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                            
                            currentUser.FirstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            //currentUser.MiddleName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMiddleName).ToString();
                            currentUser.LastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
                            currentUser.LastModifiedBy = currentUser.ID;
                            currentUser.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(
                                currentUser.Save(adc)
                            );
                        },
                        onSuccess: (SystemMessages smsgs) =>
                        {
                            msgs.Add("Updated Successfully", MessageType.Success);
                            msgs.AddRange(smsgs);
                        },
                        onFailure: (SystemMessages fmsgs) =>
                        {
                            msgs.Add("There was a problem. Please try again.", MessageType.Error);
                            msgs.AddRange(fmsgs);
                        },
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }
                
                
                var values = new NameValueCollection();
                personalQuestions.ForEach(question =>
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

                ViewBag.personalQuestions = personalQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.addressQuestions = addressQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.emergencyQuestions = emergencyQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                
            }
            
            ViewBag.msgs = msgs;
            return View(registrant);
        }
    }
}