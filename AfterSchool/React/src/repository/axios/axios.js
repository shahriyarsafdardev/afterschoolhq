import axios from 'axios';
import utils from '../../utils';

const { navigateTo } = utils;

axios.interceptors.response.use(
  (response) => {
    const { msgs } = response.data;
    if (msgs.length && msgs[0].IsError) {
      if (msgs[0].Text === 'Unauthorized') {
        alert(msgs[0].Text); // TODO: For testing purpose/phase
        navigateTo('/login');
      }
    }
    return response;
  },
  (error) => {
    if (error.response.status === 404) {
      navigateTo('/login');
    }

    return Promise.reject(error);
  },
);

export default {
  get: async (url) => {
    const response = await axios.get(url).then((res) => (res.data));

    return response;
  },
};
