import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  getPinSetupData: (registrationUid, phone) =>
    axios.get(ApiRoutes.getPinSetupData(registrationUid, phone)).then(res => res),
  updatePinSetupData: data => axios.post(ApiRoutes.updatePinSetupData, data).then(res => res),
  getChildListData: (accountName, phone, pinCode) =>
    axios.get(ApiRoutes.getChildListData(accountName, phone, pinCode)).then(res => res),
  updateStudentCheckInOutInfo: data =>
    axios.post(ApiRoutes.updateStudentCheckInOutInfoAPI, data).then(res => res),
  resetPin: data => axios.post(ApiRoutes.resetPinAPI, data).then(res => res)
};
