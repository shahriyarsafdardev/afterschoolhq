export const handleStatusAndRedirection = (
  setStatusCompleted,
  handleSection,
  resetForm,
  groupId
) => {
  setStatusCompleted(groupId);
  handleSection('next');
  resetForm();
};

export const setSkipableStatusAsCompleted = (questionsGroups, setStatusCompleted) => {
  const lastCompletedSectionId = localStorage.getItem('lastCompletedSectionId');
  if (!lastCompletedSectionId) return;
  for (let index = 0; index <= lastCompletedSectionId; index++) {
    setStatusCompleted(questionsGroups[index].QuestionGroupDataTypeId);
  }
};

export const setSectionIndexToStorage = id => {
  const lastCompletedSectionId = localStorage.getItem('lastCompletedSectionId');
  if (id >= Number(lastCompletedSectionId)) localStorage.setItem('lastCompletedSectionId', id);
};
