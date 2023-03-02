import axios from './axios/axios';
import ApiRoutes from 'routes/apiRoutes';

export default {
  submitFormSection: params => axios.post(ApiRoutes.submitFormDataAPI, params).then(res => res),
  getFormData: () => axios.get(ApiRoutes.getFormDataAPI).then(res => res),
  submitSchoolData: params => axios.post(ApiRoutes.SchoolAPI, params),
  register: params => axios.post(ApiRoutes.RegisterAPI, params)
};
