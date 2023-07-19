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
    },
    clearStudents: (state, { payload }) => {
      state.selectedStudents = [];
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
    loadingCompleted,
    clearStudents
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

  const clearSelectedStudents = () => {
    dispatch(clearStudents());
    return;
  };

  const updatePinData = async (registrationUid, phone, pinCode) => {
    const params = { registrationUid, phone, pinCode };
    dispatch(loadingStart());
    return await CheckInOutRepository.updatePinSetupData(params).then(({ data: { msgs } }) => {
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  const resetPin = async (phone, account) => {
    const params = { phone, account };
    dispatch(loadingStart());
    return await CheckInOutRepository.resetPin(params).then(({ data: { msgs } }) => {
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  const updateStudentCheckInOutInfo = async (studentList, phone, pinCode, userSignature) => {
    const formData = new FormData();
    formData.append('phone', phone);
    formData.append('pinCode', pinCode);
    if (userSignature && typeof userSignature === 'string')
      formData.append('signature', userSignature);
    else formData.append('digitalSignature', userSignature);
    studentList.forEach((obj, index) => {
      // Append each object as a separate field in the form data
      formData.append(`studentList[${index}].StudentUid`, obj.StudentUid);
      formData.append(`studentList[${index}].ActivityUid`, obj.ActivityUid);
      formData.append(`studentList[${index}].CheckInOut`, obj.CheckInOut);
    });

    dispatch(loadingStart());
    return await CheckInOutRepository.updateStudentCheckInOutInfo(formData).then(
      ({ data: { msgs } }) => {
        dispatch(loadingCompleted());
        return msgs;
      }
    );
  };

  const updateSelectedStudentsData = data => dispatch(setSelectedStudentsData(data));

  return {
    resetPin,
    loadPinData,
    updatePinData,
    loadChildListData,
    updateSelectedStudentsData,
    updateStudentCheckInOutInfo,
    clearSelectedStudents
  };
};

export default CheckInOutSlice;
