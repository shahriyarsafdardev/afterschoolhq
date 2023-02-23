export default (isParent = true) => {
  const eSignSection = {
    questions: [],
    answeredQuestions: [],
    questionsGroups: [{ FormId: 999, Name: 'Confirm Registration', QuestionGroupDataTypeId: null }]
  };
  const question = isParent
    ? {
        Description: null,
        FormQuestionGroupId: 91,
        FormQuestionGroupQuestionGroupDataTypeId: null,
        ID: 6799,
        IsRequired: true,
        Options: null,
        QuestionDataTypeId: 999,
        QuestionTypeId: 1,
        Text: 'E-Signature'
      }
    : {
        Description: null,
        FormQuestionGroupId: 91,
        FormQuestionGroupQuestionGroupDataTypeId: null,
        ID: 6799,
        IsRequired: true,
        Options: null,
        QuestionDataTypeId: 999,
        QuestionTypeId: 5,
        Text:
          "I confirm that I have entered both my information and my parent's information correctly and I intend to participate in Junior Leaders Club - 2nd Session."
      };
  eSignSection.questions.push(question);
  return eSignSection;
};
