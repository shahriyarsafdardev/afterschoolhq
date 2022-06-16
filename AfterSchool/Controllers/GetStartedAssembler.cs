using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AfterSchool.Models;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Extensions = BasicLibrary.Extensions;

namespace AfterSchool.Controllers
{
    public class GetStartedAssembler
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public GetStartedAssembler(IFeatureFlagRepository featureFlagRepository)
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        public AccountController.GetStartedResult AssembleGetStartedResult(SystemMessages msgs,
            ParticipationObject participationObject, Account registrationAccount, WebUser currentUser,
            List<FormQuestion> questions, bool isStudentAllowedToRegister, NameValueCollection requestParams)
        {
            var questionsForParentRegistration = FilterAndTransformQuestionsForParentRegistration(currentUser, questions, requestParams);

            // pre-load phone number
            if (currentUser.IsNotEmpty())
            {
                questionsForParentRegistration.Find(item =>
                        item.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone)
                    .Value = currentUser.Phone;
            }

            var result = new AccountController.GetStartedResult
            {
                StudentRegistrationEnabled = isStudentAllowedToRegister,
                ParticipationObject = participationObject,
                CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso"),
                RegistrationAccount = registrationAccount,
                Messages = msgs,
                FormQuestionViewModels = questionsForParentRegistration,
            };
            return result;
        }

        public List<FormQuestionViewModel> FilterAndTransformQuestionsForParentRegistration(WebUser currentUser,
            List<FormQuestion> questions, NameValueCollection requestParams)
        {
            var nonIncludedQuestionsExistingUser = new List<QuestionDataType>
            {
                QuestionDataType.RegistrantFirstName,
                QuestionDataType.RegistrantLastName,
                QuestionDataType.RegistrantEmail,
                QuestionDataType.RegistrantMiddleName,
                QuestionDataType.RegistrantMobilePhone,
            };

            questions = questions.Where(question =>
                Extensions.NotIn(question.QuestionDataTypeId, FormConstants.QuestionsToNotIncludeInParentPanel())).ToList();

            questions.ForEach(question =>
            {
                question.IsRequired = question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName
                                      || question.QuestionDataTypeId == QuestionDataType.RegistrantLastName
                                      || question.QuestionDataTypeId == QuestionDataType.RegistrantEmail
                                      || question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone;
            });

            var qvms = questions.Select(question => new FormQuestionViewModel(question, requestParams)).ToList();

            if (currentUser.IsEmpty())
            {
                return qvms;
            }

            qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value = currentUser.FirstName;
            qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value = currentUser.LastName;
            qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value = currentUser.Phone;
            qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value = currentUser.Email;

            qvms.ForEach(qvm =>
            {
                if (qvm.Question.QuestionDataTypeId.In(nonIncludedQuestionsExistingUser))
                {
                    qvm.Hide = true;
                    qvm.Question.IsRequired = false;
                }
            });

            return qvms;
        }
    }
}