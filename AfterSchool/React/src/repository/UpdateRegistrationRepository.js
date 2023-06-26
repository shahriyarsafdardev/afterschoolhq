import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.submitRegistrationFormDataAPI, params).then(res => res),
  getFormData: regUid => axios.get(ApiRoutes.getRegistrationFormDataAPI(regUid)).then(res => res),
  submitSchoolData: params => axios.post(ApiRoutes.UpdateRegistrationSchoolAPI, params),
  exit: uid => axios.get(ApiRoutes.exitRegistrationAPI(uid)),
  eSignature: params => axios.post(ApiRoutes.UpdateRegistrationWithESignature, params)
};
