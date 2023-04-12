import UserSlice from './slices/UserSlice';
import ProfileSlice from './slices/ProfileSlice';
import { configureStore } from '@reduxjs/toolkit';
import ThankYouSlice from './slices/ThankYouSlice';
import RegistrationSlice from './slices/RegistrationSlice';
import VolunteerProfileSlice from './slices/VolunteerProfileSlice';
import VolunteerRegistrationSlice from './slices/VolunteerRegistrationSlice';

export default configureStore({
  reducer: {
    [UserSlice.name]: UserSlice.reducer,
    [ProfileSlice.name]: ProfileSlice.reducer,
    [ThankYouSlice.name]: ThankYouSlice.reducer,
    [RegistrationSlice.name]: RegistrationSlice.reducer,
    [VolunteerProfileSlice.name]: VolunteerProfileSlice.reducer,
    [VolunteerRegistrationSlice.name]: VolunteerRegistrationSlice.reducer
  }
});
