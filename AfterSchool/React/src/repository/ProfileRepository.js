import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.updateProfileFormDataAPI, params).then(res => res),
  getFormData: userUid => axios.get(ApiRoutes.getProfileFormDataAPI(userUid)).then(res => res),
  submitSchoolData: params => axios.post(ApiRoutes.SchoolAPI, params)
};
