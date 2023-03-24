import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import UserRepository from 'repository/UserRepository';

const UserSlice = createSlice({
  name: 'userSlice',
  initialState: {
    user: {},
    loading: true
  },
  reducers: {
    setUser: (state, { payload }) => {
      state.user = payload.obj;
    },
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
    }
  }
});

export const useUserActions = () => {
  const dispatch = useDispatch();
  const { setUser, loadingStart, loadingCompleted } = UserSlice.actions;
  const loadUser = async () => {
    dispatch(loadingStart());
    const user = await UserRepository.getCurrentUser();
    dispatch(setUser(user));
    dispatch(loadingCompleted());
  };

  return { loadUser };
};

export default UserSlice;
