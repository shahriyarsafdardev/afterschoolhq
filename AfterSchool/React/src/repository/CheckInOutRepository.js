import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  getPinSetupData: (registrationUid, phone) =>
    axios.get(ApiRoutes.getPinSetupData(registrationUid, phone)).then(res => res),
  updatePinSetupData: data => axios.post(ApiRoutes.updatePinSetupData, data).then(res => res),
  getChildListData: (accountName, phone, pinCode) =>
    axios.get(ApiRoutes.getChildListData(accountName, phone, pinCode)).then(res => res)
};
