import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.submitVolunteerFormDataAPI, params).then(res => res),
  getFormData: (accountName, activityName, regUid) =>
    axios
      .get(ApiRoutes.getVolunteerFormDataAPI(accountName, activityName, regUid))
      .then(res => res),
  register: params => axios.post(ApiRoutes.VolunteerRegisterAPI, params),
  exit: uid => axios.get(ApiRoutes.exitVolunteerRegistrationAPI(uid))
};
