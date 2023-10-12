import ApiRoutes from 'routes';
import axios from './axios/axios';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.submitSessionFormDataAPI, params).then(res => res),
  getFormData: (newChild, accountName, sessionName, registrantUid) =>
    axios
      .get(
        newChild
          ? ApiRoutes.getNewChildSessionFormDataAPI(accountName, sessionName, registrantUid)
          : ApiRoutes.getSessionFormDataAPI(accountName, sessionName, registrantUid)
      )
      .then(res => res),
  submitSchoolData: params => axios.post(ApiRoutes.sessionSchoolAPI, params),
  exit: uid => axios.get(ApiRoutes.exitSessionAPI(uid)),
  register: params => axios.post(ApiRoutes.SessionAPI, params)
};
