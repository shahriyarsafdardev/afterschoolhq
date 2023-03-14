import axios from './axios/axios';
import ApiRoutes from 'routes/apiRoutes';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.updateProfileFormDataAPI, params).then(res => res),
  getFormData: (newChild, UserUid) =>
    axios.get(ApiRoutes.getProfileFormDataAPI(UserUid)).then(res => res),
  submitSchoolData: params => axios.post(ApiRoutes.SchoolAPI, params),
  register: params => axios.post(ApiRoutes.RegisterAPI, params)
};
