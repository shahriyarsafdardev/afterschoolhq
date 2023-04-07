import utils from 'utils';
import Routes from 'routes';
import RegistrationService from 'sevices/RegistrationService';

export const responseMapper = (formData, registrantType, ...restrictions) => {
  const [
    IsGradeRestricted = false,
    IsAgeRestricted = false,
    IsGenderRestricted = false
  ] = restrictions;
  const { formQuestion, formQuestionGroup, formQuestionAnswer } = formData;
  const restrictedQuestionsDataTypeIds = { dateOfBirth: 4, grade: 44, gender: 45 };
  const filteredQs = formQuestion.filter(({ IsDeleted, IsActive, QuestionDataTypeId }) => {
    const isRestrictedEnforcedQuestion =
      (IsAgeRestricted && QuestionDataTypeId === restrictedQuestionsDataTypeIds.dateOfBirth) ||
      (IsGradeRestricted && QuestionDataTypeId === restrictedQuestionsDataTypeIds.grade) ||
      (IsGenderRestricted && QuestionDataTypeId === restrictedQuestionsDataTypeIds.gender);

    if (isRestrictedEnforcedQuestion) return true;

    return !(IsDeleted || !IsActive);
  });
  const registrationService = new RegistrationService(filteredQs);
  let [formQuestionUpdated, formQuestionGroupUpdated] = [[], []];

  const restrictTheQuestion = (sectionQuestions, questionDataTypeId, IsRequired = true) => {
    const gradeIndex = sectionQuestions.findIndex(q => q.QuestionDataTypeId === questionDataTypeId);
    if (gradeIndex !== -1) {
      sectionQuestions[gradeIndex].IsRequired = IsRequired;
      if (Object.values(restrictedQuestionsDataTypeIds).includes(questionDataTypeId))
        sectionQuestions[gradeIndex].IsActive = true;
    }
  };

  formQuestionGroup.forEach(section => {
    const { QuestionGroupDataTypeId, Name } = section;
    const sectionQuestions = registrationService.groupedQuestions(QuestionGroupDataTypeId, Name);
    const sectionSkipAble = sectionQuestions.every(q => !q.IsRequired);
    const isStudentInfoSection = QuestionGroupDataTypeId === 2;

    if (isStudentInfoSection) {
      if (IsGradeRestricted)
        restrictTheQuestion(sectionQuestions, restrictedQuestionsDataTypeIds.grade);
      else if (registrantType === 'Adult')
        restrictTheQuestion(sectionQuestions, restrictedQuestionsDataTypeIds.grade, false);

      if (IsAgeRestricted)
        restrictTheQuestion(sectionQuestions, restrictedQuestionsDataTypeIds.dateOfBirth);

      if (IsGenderRestricted)
        restrictTheQuestion(sectionQuestions, restrictedQuestionsDataTypeIds.gender);
    }

    if (sectionQuestions.length) {
      formQuestionUpdated = [...formQuestionUpdated, ...sectionQuestions];
      formQuestionGroupUpdated.push({ ...section, sectionSkipAble });
    }
  });

  return { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer };
};

export const handleRedirection = ({
  IsFree,
  CanAccess,
  IsAlreadyPaid,
  RegistrantType,
  RegistrationUid,
  RegisterForNewChildUrl,
  RegistrationIsCompleted
}) => {
  const { navigateTo, redirect } = utils;

  if (!CanAccess) navigateTo('/login');
  else if (RegistrationIsCompleted) {
    if (RegistrantType === 'Parent') {
      navigateTo(RegisterForNewChildUrl);
    } else {
      if (IsFree) {
        navigateTo(Routes.thankyouUrl(RegistrationUid));
      } else {
        if (IsAlreadyPaid) navigateTo(Routes.paymentUrl(RegistrationUid));
        else redirect(Routes.paymentScreenPath(RegistrationUid));
      }
    }
  }
};
