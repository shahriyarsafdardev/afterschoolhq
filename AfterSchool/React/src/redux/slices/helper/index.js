import RegistrationService from 'sevices/RegistrationService';

export const responseMapper = ({ formQuestion, formQuestionGroup, formQuestionAnswer }) => {
  let [formQuestionUpdated, formQuestionGroupUpdated] = [[], []];
  const registrationService = new RegistrationService(formQuestion);

  formQuestionGroup.forEach(section => {
    const sectionQuestions = registrationService.groupedQuestions(section.QuestionGroupDataTypeId);

    if (!sectionQuestions.every(({ IsDeleted, IsActive }) => IsDeleted || IsActive)) {
      formQuestionUpdated = [...formQuestionUpdated, ...sectionQuestions];
      formQuestionGroupUpdated.push(section);
    }
  });

  return { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer };
};
