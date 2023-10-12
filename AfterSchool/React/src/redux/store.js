import utils from 'utils';
import UserSlice from './slices/UserSlice';
import HeaderSlice from './slices/HeaderSlice';
import SurveySlice from './slices/SurveySlice';
import ProfileSlice from './slices/ProfileSlice';
import { configureStore } from '@reduxjs/toolkit';
import ThankYouSlice from './slices/ThankYouSlice';
import APAccessSlice from './slices/APAccessSlice';
import CheckInOutSlice from './slices/CheckInOutSlice';
import FormBuilderSlice from './slices/FormBuilderSlice';
import RegistrationSlice from './slices/RegistrationSlice';
import ErrorMessagesSlice from './slices/ErrorMessagesSlice';
import RegistrantNotesSlice from './slices/RegistrantNotesSlice';
import VolunteerProfileSlice from './slices/VolunteerProfileSlice';
import UpdateRegistrationSlice from './slices/UpdateRegistrationSlice';
import SessionRegistrationSlice from './slices/SessionRegistrationSlice';
import PostRegistrationSetupSlice from './slices/PostRegistrationSetupSlice';
import VolunteerRegistrationSlice from './slices/VolunteerRegistrationSlice';
const { getEnv } = utils;

export default configureStore({
  reducer: {
    [UserSlice.name]: UserSlice.reducer,
    [HeaderSlice.name]: HeaderSlice.reducer,
    [SurveySlice.name]: SurveySlice.reducer,
    [ProfileSlice.name]: ProfileSlice.reducer,
    [ThankYouSlice.name]: ThankYouSlice.reducer,
    [APAccessSlice.name]: APAccessSlice.reducer,
    [CheckInOutSlice.name]: CheckInOutSlice.reducer,
    [FormBuilderSlice.name]: FormBuilderSlice.reducer,
    [RegistrationSlice.name]: RegistrationSlice.reducer,
    [ErrorMessagesSlice.name]: ErrorMessagesSlice.reducer,
    [RegistrantNotesSlice.name]: RegistrantNotesSlice.reducer,
    [VolunteerProfileSlice.name]: VolunteerProfileSlice.reducer,
    [UpdateRegistrationSlice.name]: UpdateRegistrationSlice.reducer,
    [SessionRegistrationSlice.name]: SessionRegistrationSlice.reducer,
    [PostRegistrationSetupSlice.name]: PostRegistrationSetupSlice.reducer,
    [VolunteerRegistrationSlice.name]: VolunteerRegistrationSlice.reducer
  },
  devTools: getEnv() === 'Local'
});
