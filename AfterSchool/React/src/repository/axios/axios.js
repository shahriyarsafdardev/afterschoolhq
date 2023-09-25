import axios from 'axios';
import utils from 'utils';
import ErrorMessagesSlice from 'redux/slices/ErrorMessagesSlice';

let store;

const { navigateTo } = utils;
axios.defaults.baseURL = window.location.origin;
axios.defaults.headers = {
  'Cache-Control': 'no-cache',
  Pragma: 'no-cache',
  Expires: '0'
};

axios.interceptors.response.use(responseHandler, errorHandler);

function responseHandler(response) {
  const { setMsgs } = ErrorMessagesSlice.actions;
  const { msgs = [], obj = {} } = response.data;

  if (msgs?.length && msgs[0].IsError) {
    if (msgs.find(msg => ['No Registrant', 'Unauthorized'].includes(msg.Text)))
      navigateTo('/login');
    else if (msgs.find(msg => ['Invalid Form.'].includes(msg.Text))) {
      navigateTo('/404.html');
    }
  }
  store.dispatch(setMsgs(msgs));

  return { ...response, data: { obj, msgs } };
}

function errorHandler(error) {
  if (error.response?.status === 404) navigateTo('/login');

  return Promise.reject(error);
}

export const injectStore = _store => {
  store = _store;
};

export default {
  get: async url => {
    const response = await axios.get(url).then(res => res.data);
    return response;
  },
  post: async (url, params) => {
    const response = await axios.post(url, params, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return response;
  }
};
