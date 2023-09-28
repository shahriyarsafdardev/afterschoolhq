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

export const setSectionIndexToStorage = id => {
  const lastCompletedSectionId = localStorage.getItem('lastCompletedSectionId');
  if (id >= Number(lastCompletedSectionId)) localStorage.setItem('lastCompletedSectionId', id);
};
