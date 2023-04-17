import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: params => axios.post(ApiRoutes.submitFormDataAPI, params).then(res => res),
  getFormData: (newChild, accountName, activityName, registrantUid) =>
    axios
      .get(
        newChild
          ? ApiRoutes.getNewChildFormDataAPI(accountName, activityName, registrantUid)
          : ApiRoutes.getFormDataAPI(accountName, activityName, registrantUid)
      )
      .then(res => res),
  submitSchoolData: params => axios.post(ApiRoutes.SchoolAPI, params),
  register: params => axios.post(ApiRoutes.RegisterAPI, params)
};
