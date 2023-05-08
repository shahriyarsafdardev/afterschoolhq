export const newQuestion = {
  ID: 0, //Need to set: Zero
  FormQuestionGroupId: null, //Need to set: Current Section ID
  Text: '',
  Link: null,
  Description: null,
  GroupTitle: 'Current Section Name', //Need to set: Current Section Name
  Options: '',
  IsRequired: false,
  DisplayOrder: 1, // Need to set: according to the position
  IsActive: true,
  IsDeleted: false,
  QuestionSettings: [
    { Audience: 1, Show: true, Required: true },
    { Audience: 2, Show: true, Required: true },
    { Audience: 3, Show: true, Required: true }
  ],
  IsAnswerDescriptionEnabled: false,
  QuestionTypeId: 1,
  QuestionDataTypeId: 0,
  FormQuestionGroupQuestionGroupDataTypeId: 0
};
