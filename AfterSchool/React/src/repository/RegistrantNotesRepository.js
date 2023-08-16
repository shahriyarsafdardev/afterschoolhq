import axios from './axios/axios';
import ApiRoutes from 'routes';

export const getRegistrantNotes = (regUid, perPage, pageNo, searchQuery) =>
  axios
    .get(ApiRoutes.getRegistrationNotesAPI(regUid, perPage, pageNo, searchQuery))
    .then(res => res);
export const createRegistrantNote = data =>
  axios.post(ApiRoutes.createRegistrationNoteAPI, data).then(res => res);
export const updateRegistrantNote = data =>
  axios.post(ApiRoutes.updateRegistrantNoteAPI, data).then(res => res);
export const deleteRegistrantNote = data =>
  axios.post(ApiRoutes.deleteRegistrantNoteAPI, data).then(res => res);
