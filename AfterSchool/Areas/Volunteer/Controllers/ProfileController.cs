using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Assemblers;
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
                    return RedirectToAction("index","Login");
                
                var questionsWithAnswers = VolunteerProfileQuestionsAssembler.AssembleVolunteerProfileQuestions(msgs, registrant);
                ViewBag.personalQuestions = questionsWithAnswers.personalQuestions;
                ViewBag.addressQuestions = questionsWithAnswers.addressQuestions;
                ViewBag.emergencyQuestions = questionsWithAnswers.emergencyContactQuestions;
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
                    return RedirectToAction("index","Login");
                
                var questions = new List<FormQuestion>();

                var personalQuestions = VolunteerProfileQuestionsAssembler.GetQuestions(msgs, 1);
                var addressQuestions = VolunteerProfileQuestionsAssembler.GetQuestions(msgs, 2);
                var emergencyQuestions = FormQuestionService.GetEmergencyContactQuestions(msgs);

                questions.AddRange(personalQuestions);
                questions.AddRange(addressQuestions);
                questions.AddRange(
                    emergencyQuestions.Where(qvm =>
                        qvm.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 &&
                        qvm.QuestionDataTypeId != QuestionDataType.EmergencyContact1RelationshipToRegistrant)
                    );
                questions.AddRange(
                    emergencyQuestions.Where(qvm =>
                        qvm.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2 &&
                        qvm.QuestionDataTypeId != QuestionDataType.EmergencyContact1RelationshipToRegistrant)
                );
                
                // Validation
                questions.ForEach(question =>
                {
                    var answer = Request[question.ToFormQuestionViewModel().Name];

                    if (question.IsRequired && answer.IsEmpty())
                        msgs.Add($"{question.Text} is required", MessageType.UserError);

                    switch (question.QuestionDataTypeId)
                    {
                        case QuestionDataType.RegistrantMobilePhone:
                        case QuestionDataType.RegistrantDoctorPhone:
                            FormService.PhoneValidation(answer, question, msgs);
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
                var questionsWithAnswers = VolunteerProfileQuestionsAssembler.AssembleVolunteerProfileQuestions(msgs, registrant);
                ViewBag.personalQuestions = questionsWithAnswers.personalQuestions;
                ViewBag.addressQuestions = questionsWithAnswers.addressQuestions;
                ViewBag.emergencyQuestions = questionsWithAnswers.emergencyContactQuestions;
            }
            
            ViewBag.msgs = msgs;
            return View(registrant);
        }
    }
}