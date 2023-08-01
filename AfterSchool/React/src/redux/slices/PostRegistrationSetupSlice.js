import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';
import PostRegistrationSetupRepository from 'repository/PostRegistrationSetupRepository';
import { useHeaderActions } from './HeaderSlice';

const PostRegistrationSetupSlice = createSlice({
  name: 'postRegistrationSetupSlice',
  initialState: {
    data: {},
    msgs: [],
    loading: true
  },
  reducers: {
    setData: (state, { payload }) => {
      state.data = payload || {};
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

export const usePostRegistrationActions = () => {
  const dispatch = useDispatch();
  const { loadHeader } = useHeaderActions();
  const { setMsgs, setData, loadingStart, loadingCompleted } = PostRegistrationSetupSlice.actions;

  const postRegistration = async regUid => {
    dispatch(loadingStart());
    await PostRegistrationSetupRepository.getRegData(regUid).then(({ obj }) => {
      dispatch(setData(obj));
      const { Name, Organization } = obj.activityDto;
      loadHeader({ Name, Organization });
    });
    dispatch(loadingCompleted());
  };

  const updateData = async params => {
    return await PostRegistrationSetupRepository.addOrUpdateRegistrantUser(params).then(
      ({ data: { msgs } }) => {
        dispatch(setMsgs(msgs));
        return msgs;
      }
    );
  };

  return { postRegistration, updateData };
};

export default PostRegistrationSetupSlice;
