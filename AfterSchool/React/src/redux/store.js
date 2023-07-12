import utils from 'utils';
import UserSlice from './slices/UserSlice';
import HeaderSlice from './slices/HeaderSlice';
import SurveySlice from './slices/SurveySlice';
import ProfileSlice from './slices/ProfileSlice';
import { configureStore } from '@reduxjs/toolkit';
import ThankYouSlice from './slices/ThankYouSlice';
import PinSetupSlice from './slices/PinSetupSlice';
import FormBuilderSlice from './slices/FormBuilderSlice';
import RegistrationSlice from './slices/RegistrationSlice';
import VolunteerProfileSlice from './slices/VolunteerProfileSlice';
import UpdateRegistrationSlice from './slices/UpdateRegistrationSlice';
import PostRegistrationSetupSlice from './slices/PostRegistrationSetupSlice';
import VolunteerRegistrationSlice from './slices/VolunteerRegistrationSlice';
const { getEnv } = utils;

export default configureStore({
  reducer: {
    [UserSlice.name]: UserSlice.reducer,
    [HeaderSlice.name]: HeaderSlice.reducer,
    [SurveySlice.name]: SurveySlice.reducer,
    [ProfileSlice.name]: ProfileSlice.reducer,
    [PinSetupSlice.name]: PinSetupSlice.reducer,
    [ThankYouSlice.name]: ThankYouSlice.reducer,
    [FormBuilderSlice.name]: FormBuilderSlice.reducer,
    [RegistrationSlice.name]: RegistrationSlice.reducer,
    [VolunteerProfileSlice.name]: VolunteerProfileSlice.reducer,
    [UpdateRegistrationSlice.name]: UpdateRegistrationSlice.reducer,
    [PostRegistrationSetupSlice.name]: PostRegistrationSetupSlice.reducer,
    [VolunteerRegistrationSlice.name]: VolunteerRegistrationSlice.reducer
  },
  devTools: getEnv() === 'Local'
});
