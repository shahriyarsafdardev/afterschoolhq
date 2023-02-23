import axios from './axios/axios';
import ApiRoutes from 'routes/apiRoutes';

export default {
  getFormData: () => axios.get(ApiRoutes.getFormDataAPI).then(res => res),
  submitFormSection: params => axios.post(ApiRoutes.submitFormDataAPI, params).then(res => res),
  register: params => axios.post(ApiRoutes.RegisterAPI, params).then(res => res)
};
