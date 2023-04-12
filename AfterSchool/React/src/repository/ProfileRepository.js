import axios from './axios/axios';
import ApiRoutes from 'routes';

const {
  getProfileFormDataAPI,
  updateProfileFormDataAPI,
  getVolunteerProfileFormDataAPI,
  updateVolunteerProfileFormDataAPI
} = ApiRoutes;
export default {
  submitFormSection: (params, isVolunteer = false) =>
    axios
      .post(isVolunteer ? updateVolunteerProfileFormDataAPI : updateProfileFormDataAPI, params)
      .then(res => res),

  getFormData: (userUid, isVolunteer = false) =>
    axios
      .get(isVolunteer ? getVolunteerProfileFormDataAPI(userUid) : getProfileFormDataAPI(userUid))
      .then(res => res),

  submitSchoolData: params => axios.post(ApiRoutes.SchoolAPI, params)
};
