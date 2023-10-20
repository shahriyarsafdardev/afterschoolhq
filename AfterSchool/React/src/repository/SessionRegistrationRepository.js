import ApiRoutes from 'routes';
import axios from './axios/axios';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.submitSessionFormDataAPI, params).then(res => res),
  getFormData: (newChild, accountName, activityGroupName, registrantUid) =>
    axios
      .get(ApiRoutes.getSessionFormDataAPI(accountName, activityGroupName, registrantUid))
      .then(res => res),
  submitSchoolData: params => axios.post(ApiRoutes.sessionSchoolAPI, params),
  exit: uid => axios.get(ApiRoutes.exitSessionAPI(uid)),
  sessionRegister: params => axios.post(ApiRoutes.SessionRegisterAPI, params)
};
