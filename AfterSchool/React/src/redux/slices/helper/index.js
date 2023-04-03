import RegistrationService from 'sevices/RegistrationService';

export const responseMapper = (formData, registrantType, ...restrictions) => {
  const [IsGradeRestricted = false, IsAgeRestricted = false] = restrictions;
  const { formQuestion, formQuestionGroup, formQuestionAnswer } = formData;
  const restrictedQuestionsDataTypeIds = { dateOfBirth: 4, grade: 44 };
  const filteredQs = formQuestion.filter(({ IsDeleted, IsActive, QuestionDataTypeId }) => {
    if ((IsAgeRestricted && QuestionDataTypeId === restrictedQuestionsDataTypeIds.dateOfBirth)||
        (IsGradeRestricted && QuestionDataTypeId === restrictedQuestionsDataTypeIds.grade))
      return true;

    return !(IsDeleted || !IsActive);
  });
  const registrationService = new RegistrationService(filteredQs);
  let [formQuestionUpdated, formQuestionGroupUpdated] = [[], []];

  const restrictTheQuestion = (sectionQuestions, questionDataTypeId) => {
    const gradeIndex = sectionQuestions.findIndex(q => q.QuestionDataTypeId === questionDataTypeId);
    if (gradeIndex !== -1) {
      sectionQuestions[gradeIndex].IsRequired = true;
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
      if (registrantType !== 'Adult' && IsGradeRestricted)
        restrictTheQuestion(sectionQuestions, restrictedQuestionsDataTypeIds.grade);
      if (IsAgeRestricted)
        restrictTheQuestion(sectionQuestions, restrictedQuestionsDataTypeIds.dateOfBirth);
    }

    if (sectionQuestions.length) {
      formQuestionUpdated = [...formQuestionUpdated, ...sectionQuestions];
      formQuestionGroupUpdated.push({ ...section, sectionSkipAble });
    }
  });

  return { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer };
};
