import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';
import ManageAPAccessRepository from 'repository/ManageAPAccessRepository';

const APAccessSlice = createSlice({
  name: 'APAccessSlice',
  initialState: {
    data: {},
    msgs: [],
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
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useAPAccessActions = () => {
  const dispatch = useDispatch();
  const { setMsgs, setData, loadingStart, loadingCompleted } = APAccessSlice.actions;
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

  return {
    loadAuthorizedPersonsList,
    updateAuthorizedPersons,
    resendInvite
  };
};

export default APAccessSlice;
