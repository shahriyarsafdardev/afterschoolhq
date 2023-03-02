export default registrantType => {
  if (registrantType === 'Adult') return {};
  return {
    ID: 999999,
    Text: 'School',
    IsRequired: false,
    FormQuestionGroupQuestionGroupDataTypeId: 2,
    DisplayOrder: 20,
    QuestionTypeId: 1,
    FormQuestionGroupId: 2,
    Description: null,
    Options: null,
    QuestionDataTypeId: 999999
  };
};
