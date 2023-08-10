import axios from './axios/axios';
import ApiRoutes from 'routes';
const {
  addOrUpdateAuthorizedPersonsAPI,
  updateExistingPinForParentAPI,
  createNewPinForParentAPI,
  getAuthorizedPersonsAPI,
  resendInviteAPI
} = ApiRoutes;
export default {
  getAuthorizedPersons: () => axios.get(getAuthorizedPersonsAPI).then(res => res),
  addOrUpdateAuthorizedPersons: studentsList =>
    axios.post(addOrUpdateAuthorizedPersonsAPI, studentsList).then(res => res),
  resendInvite: data => axios.post(resendInviteAPI, data).then(res => res),
  createNewPinForParent: params => axios.post(createNewPinForParentAPI, params).then(res => res),
  updateExistingPinForParent: params =>
    axios.post(updateExistingPinForParentAPI, params).then(res => res)
};
