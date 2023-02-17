import axios from './axios/axios';

export default {
  getFormData: () => axios.get('http://localhost:54849/ReactApi/React/GetForm').then(res => res)
};
