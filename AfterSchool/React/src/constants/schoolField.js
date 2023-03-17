export default registrantType => {
  if (registrantType === 'Adult') return {};
  return {
    ID: 99999,
    GroupTitle: 'Student Information',
    Text: 'School',
    IsRequired: true,
    FormQuestionGroupQuestionGroupDataTypeId: 2,
    DisplayOrder: 20,
    QuestionTypeId: 1,
    FormQuestionGroupId: 2,
    Description: null,
    Options: null,
    QuestionDataTypeId: 99999
  };
};
