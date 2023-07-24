import axios from './axios/axios';
import ApiRoutes from 'routes';
const { getAuthorizedPersonsAPI, addOrUpdateAuthorizedPersonsAPI, resendInviteAPI } = ApiRoutes;
export default {
  getAuthorizedPersons: () => axios.get(getAuthorizedPersonsAPI).then(res => res),
  addOrUpdateAuthorizedPersons: studentsList =>
    axios.post(addOrUpdateAuthorizedPersonsAPI, studentsList).then(res => res),
  resendInvite: data => axios.post(resendInviteAPI, data).then(res => res)
};
