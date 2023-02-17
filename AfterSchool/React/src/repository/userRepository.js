import axios from './axios/axios';

export default {
  getCurrentUser: () =>
    axios.get('http://localhost:54849/ReactApi/React/GetCurrentUser').then(res => res)
};
