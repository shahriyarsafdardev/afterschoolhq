import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  getCurrentUser: () => axios.get(ApiRoutes.currentUserAPI).then(res => res)
};
