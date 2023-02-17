import { configureStore } from '@reduxjs/toolkit';
import UserSlice from './slices/UserSlice';
import RegistrationSlice from './slices/RegistrationSlice';

export default configureStore({
  reducer: {
    [UserSlice.name]: UserSlice.reducer,
    [RegistrationSlice.name]: RegistrationSlice.reducer,
  },
  devTools: process.env.NODE_ENV !== 'production',
});
