import axios from './axios/axios';
import ApiRoutes from 'routes';
const { getRegistrationAPI, AddOrUpdateRegrantUserAPI } = ApiRoutes;
export default {
  getRegData: registrationUid => axios.get(getRegistrationAPI(registrationUid)).then(res => res),
  addOrUpdateRegistrantUser: params =>
    axios.post(AddOrUpdateRegrantUserAPI, params).then(res => res)
};
