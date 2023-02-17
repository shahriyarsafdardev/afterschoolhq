import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import UserRepository from '../../repository/UserRepository';

const UserSlice = createSlice({
  name: 'userSlice',
  initialState: {
    user: {}
  },
  reducers: {
    setUser: (state, { payload }) => {
      state.user = payload.obj;
    }
  }
});

export const useUserActions = () => {
  const dispatch = useDispatch();
  const loadUser = async () => {
    const user = await UserRepository.getCurrentUser();
    dispatch(UserSlice.actions.setUser(user));
  };

  return { loadUser };
};

export default UserSlice;
