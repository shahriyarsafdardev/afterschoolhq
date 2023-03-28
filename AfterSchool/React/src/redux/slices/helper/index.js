import RegistrationService from 'sevices/RegistrationService';

export const responseMapper = (formData, registrantType) => {
  const { formQuestion, formQuestionGroup, formQuestionAnswer } = formData;
  const filteredQs = formQuestion.filter(({ IsDeleted, IsActive }) => !(IsDeleted || !IsActive));
  const registrationService = new RegistrationService(filteredQs);
  let [formQuestionUpdated, formQuestionGroupUpdated] = [[], []];

  formQuestionGroup.forEach(section => {
    const { QuestionGroupDataTypeId, Name } = section;
    const sectionQuestions = registrationService.groupedQuestions(QuestionGroupDataTypeId, Name);
    const sectionSkipAble = sectionQuestions.every(q => !q.IsRequired);

    const isStudentInfoSection = QuestionGroupDataTypeId === 2;
    if (registrantType !== 'Adult' && isStudentInfoSection) {
      const gradeIndex = sectionQuestions.findIndex(q => q.Text === 'Grade');
      sectionQuestions[gradeIndex].IsRequired = true;
    }

    if (sectionQuestions.length) {
      formQuestionUpdated = [...formQuestionUpdated, ...sectionQuestions];
      formQuestionGroupUpdated.push({ ...section, sectionSkipAble });
    }
  });

  return { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer };
};
