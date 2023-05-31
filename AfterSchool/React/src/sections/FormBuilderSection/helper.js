export const newQuestion = formType => {
  const question = {
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

  if (formType !== 3) return question;

  return { ...question, QuestionSettings: [{ Audience: 4, Show: true, Required: true }] };
};

export const unSavedDialogProps = {
  title: 'Unsaved Changes',
  cancelButtonText: 'Stay on Page',
  dataTestId: 'unsavedChangesModal',
  confirmationButtonText: 'Leave This Page',
  text:
    'If you leave this page, any unsaved changes will be lost. Please close this window and click “Save” to keep progress.'
};

export const deleteItemDialogProps = item => ({
  title: `Delete ${item}`,
  text: `Do you want to delete this ${item.toLowerCase()}? This action is irreversible.`,
  dataTestId: `delete${item}`
});

export const infoText =
  'Selecting one of these options will apply it to existing questions (except for locked questions). Changing or de-selecting an option will override and apply it to existing questions. New questions will be shown/required for all audiences bu default. If an option below is selected, you can still make changes to individual questuions.';
