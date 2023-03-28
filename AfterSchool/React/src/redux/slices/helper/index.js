import RegistrationService from 'sevices/RegistrationService';

export const responseMapper = (formData, registrantType, ...restrictions) => {
  const [IsGradeRestricted = false, IsAgeRestricted = false] = restrictions;
  const { formQuestion, formQuestionGroup, formQuestionAnswer } = formData;
  const filteredQs = formQuestion.filter(({ IsDeleted, IsActive }) => !(IsDeleted || !IsActive));
  const registrationService = new RegistrationService(filteredQs);
  let [formQuestionUpdated, formQuestionGroupUpdated] = [[], []];

  const restrictTheQuestion = (sectionQuestions, questionText) => {
    const gradeIndex = sectionQuestions.findIndex(q => q.Text === questionText);
    return (sectionQuestions[gradeIndex].IsRequired = true);
  };

  formQuestionGroup.forEach(section => {
    const { QuestionGroupDataTypeId, Name } = section;
    const sectionQuestions = registrationService.groupedQuestions(QuestionGroupDataTypeId, Name);
    const sectionSkipAble = sectionQuestions.every(q => !q.IsRequired);
    const isStudentInfoSection = QuestionGroupDataTypeId === 2;

    if (isStudentInfoSection) {
      if (registrantType !== 'Adult' && IsGradeRestricted)
        restrictTheQuestion(sectionQuestions, 'Grade');
      if (IsAgeRestricted) restrictTheQuestion(sectionQuestions, 'Date of Birth');
    }

    if (sectionQuestions.length) {
      formQuestionUpdated = [...formQuestionUpdated, ...sectionQuestions];
      formQuestionGroupUpdated.push({ ...section, sectionSkipAble });
    }
  });

  return { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer };
};
