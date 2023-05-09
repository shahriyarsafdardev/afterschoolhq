import axios from './axios/axios';
import ApiRoutes from 'routes';
const { VolunteerRegistrationThankYouAPI, ThankYouAPI } = ApiRoutes;
export default {
  getThankYouScreenData: (id, volunteerRegistration) =>
    axios
      .get(volunteerRegistration ? VolunteerRegistrationThankYouAPI(id) : ThankYouAPI(id))
      .then(res => res)
};
