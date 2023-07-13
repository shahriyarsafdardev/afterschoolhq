import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  getData: () => axios.get(ApiRoutes.getRegistrantAPI).then(res => res),
  addNewRegistrant: () => axios.get(ApiRoutes.addNewRegistrantAPI).then(res => res)
};
