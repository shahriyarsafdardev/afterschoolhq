using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Assemblers
{
    public class VolunteerProfileQuestions
    {
        public List<FormQuestionViewModel> personalQuestions;
        public List<FormQuestionViewModel> addressQuestions;
        public List<FormQuestionViewModel> emergencyContactQuestions;
    }
    public class VolunteerProfileQuestionsAssembler
    {
        public static VolunteerProfileQuestions AssembleVolunteerProfileQuestions(SystemMessages msgs, Registrant registrant)
        {
            var personalQuestions = GetQuestions(msgs, 1);
            var addressQuestions = GetQuestions(msgs, 2);
            var emergencyQuestions = FormQuestionService.GetEmergencyContactQuestions(msgs);
            var values = new NameValueCollection();

            VolunteerProfileQuestions questionsWithAnswers = new VolunteerProfileQuestions();
            
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
            
                

            questionsWithAnswers.personalQuestions = personalQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            questionsWithAnswers.addressQuestions = addressQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            questionsWithAnswers.emergencyContactQuestions = emergencyQuestions.Select(question => question.ToFormQuestionViewModel(values)).ToList();

            return questionsWithAnswers;
        }
        public static List<FormQuestion> GetQuestions(SystemMessages msgs, int type)
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
    }
    
}