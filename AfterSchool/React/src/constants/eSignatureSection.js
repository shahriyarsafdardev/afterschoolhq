export default (isParent = true) => {
  const eSignSection = {
    questions: [],
    answeredQuestions: [],
    questionsGroups: [{ FormId: 999, Name: 'Confirm Registration', QuestionGroupDataTypeId: null }]
  };
  const question = isParent
    ? {
        ID: 6799,
        Options: null,
        IsRequired: true,
        Description: null,
        QuestionTypeId: 1,
        Text: 'E-Signature',
        QuestionDataTypeId: 999,
        FormQuestionGroupId: 91,
        GroupTitle: 'Confirm Registration',
        FormQuestionGroupQuestionGroupDataTypeId: null
      }
    : {
        ID: 6799,
        Options: null,
        IsRequired: true,
        Description: null,
        QuestionTypeId: 5,
        FormQuestionGroupId: 91,
        QuestionDataTypeId: 999,
        GroupTitle: 'Confirm Registration',
        FormQuestionGroupQuestionGroupDataTypeId: null,
        Text:
          "I confirm that I have entered both my information and my parent's information correctly and I intend to participate in Junior Leaders Club - 2nd Session."
      };
  eSignSection.questions.push(question);
  return eSignSection;
};
