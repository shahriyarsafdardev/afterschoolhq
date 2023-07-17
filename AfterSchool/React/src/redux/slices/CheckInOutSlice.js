import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import CheckInOutRepository from 'repository/CheckInOutRepository';

const CheckInOutSlice = createSlice({
  name: 'checkInOutSlice',
  initialState: {
    pinSetupData: {},
    childData: {},
    loading: false,
    selectedStudents: [],
    msgs: []
  },
  reducers: {
    setSelectedStudentsData: (state, { payload }) => {
      state.selectedStudents = payload;
    },
    setPinData: (state, { payload }) => {
      state.pinSetupData = payload || {};
    },
    setChildListData: (state, { payload }) => {
      state.childData = payload || {};
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

export const useCheckInOutActions = () => {
  const dispatch = useDispatch();
  const {
    setMsgs,
    setPinData,
    setSelectedStudentsData,
    setChildListData,
    loadingStart,
    loadingCompleted
  } = CheckInOutSlice.actions;
  const loadPinData = async (registrationUid, phone) => {
    dispatch(loadingStart());
    return await CheckInOutRepository.getPinSetupData(registrationUid, phone).then(
      ({ obj, msgs }) => {
        dispatch(setPinData(obj));
        dispatch(setMsgs(msgs));
        dispatch(loadingCompleted());
        return msgs;
      }
    );
  };

  const loadChildListData = async (accountName, phone, pinCode) => {
    dispatch(loadingStart());
    return await CheckInOutRepository.getChildListData(accountName, phone, pinCode).then(
      ({ obj, msgs }) => {
        dispatch(setChildListData(obj));
        dispatch(setMsgs(msgs));
        dispatch(loadingCompleted());
        return msgs;
      }
    );
  };

  const updatePinData = async (registrationUid, phone, pinCode) => {
    const params = { registrationUid, phone, pinCode };
    dispatch(loadingStart());
    return await CheckInOutRepository.updatePinSetupData(params).then(({ data: { msgs } }) => {
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  const updateStudentCheckInOutInfo = async (studentList, phone, pinCode) => {
    const params = { studentList, phone, pinCode };
    dispatch(loadingStart());
    return await CheckInOutRepository.updateStudentCheckInOutInfo(params).then(
      ({ data: { msgs } }) => {
        dispatch(loadingCompleted());
        return msgs;
      }
    );
  };

  const updateSelectedStudentsData = data => dispatch(setSelectedStudentsData(data));

  return {
    loadPinData,
    updatePinData,
    loadChildListData,
    updateSelectedStudentsData,
    updateStudentCheckInOutInfo
  };
};

export default CheckInOutSlice;
