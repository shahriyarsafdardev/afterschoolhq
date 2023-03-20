import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  getThankYouScreenData: id => axios.get(ApiRoutes.ThankYouAPI(id)).then(res => res)
};
