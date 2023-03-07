import RegistrationService from 'sevices/RegistrationService';

export const responseMapper = ({ formQuestion, formQuestionGroup, formQuestionAnswer }) => {
  const filteredQs = formQuestion.filter(({ IsDeleted, IsActive }) => !(IsDeleted || !IsActive));
  const registrationService = new RegistrationService(filteredQs);
  let [formQuestionUpdated, formQuestionGroupUpdated] = [[], []];

  formQuestionGroup.forEach(section => {
    const sectionQuestions = registrationService.groupedQuestions(section.QuestionGroupDataTypeId);
    const sectionSkipAble = sectionQuestions.every(q => !q.IsRequired);

    if (sectionQuestions.length) {
      formQuestionUpdated = [...formQuestionUpdated, ...sectionQuestions];
      formQuestionGroupUpdated.push({ ...section, sectionSkipAble });
    }
  });

  return { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer };
};
