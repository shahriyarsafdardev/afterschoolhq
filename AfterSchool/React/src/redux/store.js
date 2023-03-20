import UserSlice from './slices/UserSlice';
import ProfileSlice from './slices/ProfileSlice';
import { configureStore } from '@reduxjs/toolkit';
import ThankYouSlice from './slices/ThankYouSlice';
import RegistrationSlice from './slices/RegistrationSlice';

export default configureStore({
  reducer: {
    [UserSlice.name]: UserSlice.reducer,
    [ProfileSlice.name]: ProfileSlice.reducer,
    [ThankYouSlice.name]: ThankYouSlice.reducer,
    [RegistrationSlice.name]: RegistrationSlice.reducer
  }
});
