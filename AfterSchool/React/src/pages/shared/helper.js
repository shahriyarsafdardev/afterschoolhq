import utils from 'utils';

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

export const checkEmptyValues = values => !Object.values(values).some(x => x !== '');

export const completeLoadingAndRedirect = (loaded, url) => {
  const { redirect } = utils;
  loaded();
  redirect(url);
};
