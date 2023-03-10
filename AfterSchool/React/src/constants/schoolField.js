export default registrantType => {
  if (registrantType === 'Adult') return {};
  const IsRequired = registrantType === 'Student';
  return {
    ID: 99999,
    GroupTitle: 'Student Information',
    Text: 'School',
    IsRequired,
    FormQuestionGroupQuestionGroupDataTypeId: 2,
    DisplayOrder: 20,
    QuestionTypeId: 1,
    FormQuestionGroupId: 2,
    Description: null,
    Options: null,
    QuestionDataTypeId: 99999
  };
};
