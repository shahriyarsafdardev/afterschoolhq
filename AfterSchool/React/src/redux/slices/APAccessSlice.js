import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';
import ErrorMessagesSlice from './ErrorMessagesSlice';
import ManageAPAccessRepository from 'repository/ManageAPAccessRepository';

const APAccessSlice = createSlice({
  name: 'APAccessSlice',
  initialState: {
    data: {},
    loading: true
  },
  reducers: {
    setData: (state, { payload: { obj } }) => {
      state.data = obj;
    },
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
    }
  }
});

export const useAPAccessActions = () => {
  const dispatch = useDispatch();
  const { setMsgs } = ErrorMessagesSlice.actions;
  const { setData, loadingStart, loadingCompleted } = APAccessSlice.actions;

  const loadAuthorizedPersonsList = async () => {
    dispatch(loadingStart());
    const data = await ManageAPAccessRepository.getAuthorizedPersons();
    dispatch(setData(data));
    dispatch(setMsgs(data.msgs));
    dispatch(loadingCompleted());
  };

  const updateAuthorizedPersons = async studentsList => {
    const params = { studentsList };
    return await ManageAPAccessRepository.addOrUpdateAuthorizedPersons(params).then(
      ({ data: { msgs, obj } }) => {
        dispatch(setMsgs(msgs));
        if (!msgs.length) {
          dispatch(setData({ obj }));
        }
        return msgs;
      }
    );
  };

  const resendInvite = async (studentUid, webUserId) => {
    const params = { studentUid, webUserId };
    return await ManageAPAccessRepository.resendInvite(params).then(({ data: { msgs } }) => {
      dispatch(setMsgs(msgs));
      return msgs;
    });
  };

  const createPinForParent = async (phone, pinCode, data) => {
    const params = { phone, pinCode };

    return await ManageAPAccessRepository.createNewPinForParent(params).then(
      ({ data: { msgs } }) => {
        dispatch(setMsgs(msgs));
        if (!msgs.length) {
          const obj = { ...data, IsPinSetup: true };
          if (phone) obj.IsPhoneSetup = true;
          dispatch(setData({ obj }));
        }
        return msgs;
      }
    );
  };

  const updateExistingPin = async (oldPinCode, newPinCode) => {
    const params = { oldPinCode, newPinCode };
    return await ManageAPAccessRepository.updateExistingPinForParent(params).then(
      ({ data: { msgs } }) => {
        dispatch(setMsgs(msgs));
        return msgs;
      }
    );
  };

  return {
    loadAuthorizedPersonsList,
    updateAuthorizedPersons,
    updateExistingPin,
    createPinForParent,
    resendInvite
  };
};

export default APAccessSlice;
