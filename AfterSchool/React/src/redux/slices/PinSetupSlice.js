import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import PinSetupRepository from 'repository/PinSetupRepository';

const PinSetupSlice = createSlice({
  name: 'pinSetupSlice',
  initialState: {
    pinSetupData: {},
    loading: false,
    msgs: []
  },
  reducers: {
    setPinData: (state, { payload }) => {
      state.pinSetupData = payload || {};
    },
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const usePinSetupActions = () => {
  const dispatch = useDispatch();
  const { setMsgs, setPinData, loadingStart, loadingCompleted } = PinSetupSlice.actions;
  const loadPinData = async (registrationUid, phone) => {
    dispatch(loadingStart());
    return await PinSetupRepository.getPinSetupData(registrationUid, phone).then(
      ({ obj, msgs }) => {
        dispatch(setPinData(obj));
        dispatch(setMsgs(msgs));
        dispatch(loadingCompleted());
        return msgs;
      }
    );
  };

  const updatePinData = async (registrationUid, phone, pinCode) => {
    const params = { registrationUid, phone, pinCode };
    dispatch(loadingStart());
    return await PinSetupRepository.updatePinSetupData(params).then(({ data: { msgs } }) => {
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  return { loadPinData, updatePinData };
};

export default PinSetupSlice;
