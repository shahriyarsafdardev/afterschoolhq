import axios from './axios/axios';
import ApiRoutes from 'routes/apiRoutes';

export default {
  getCurrentUser: () => axios.get(ApiRoutes.currentUserAPI).then(res => res)
};
