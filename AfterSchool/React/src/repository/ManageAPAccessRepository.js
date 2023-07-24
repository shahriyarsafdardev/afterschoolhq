import axios from './axios/axios';
import ApiRoutes from 'routes';
const { getAuthorizedPersonsAPI, AddOrUpdateAuthorizedPersonsAPI } = ApiRoutes;
export default {
  getAuthorizedPersons: () => axios.get(getAuthorizedPersonsAPI).then(res => res),
  addOrUpdateAuthorizedPersons: studentsList =>
    axios.post(AddOrUpdateAuthorizedPersonsAPI, studentsList).then(res => res)
};
