import { useSelector } from 'react-redux';
import { useUserActions } from 'redux/slices/UserSlice';
import { useSurveyActions } from 'redux/slices/SurveySlice';
import { useHeaderActions } from 'redux/slices/HeaderSlice';
import { useProfileActions } from 'redux/slices/ProfileSlice';
import { useThankYouActions } from 'redux/slices/ThankYouSlice';
import { useAPAccessActions } from 'redux/slices/APAccessSlice';
import { useCheckInOutActions } from 'redux/slices/CheckInOutSlice';
import { useFormBuilderActions } from 'redux/slices/FormBuilderSlice';
import { useRegistrationActions } from 'redux/slices/RegistrationSlice';
import { useRegistrantNotesActions } from 'redux/slices/RegistrantNotesSlice';
import { useVolunteerProfileActions } from 'redux/slices/VolunteerProfileSlice';
import { useUpdateRegistrationActions } from 'redux/slices/UpdateRegistrationSlice';
import { usePostRegistrationActions } from 'redux/slices/PostRegistrationSetupSlice';
import { useSessionRegistrationActions } from 'redux/slices/SessionRegistrationSlice';
import { useVolunteerRegistrationActions } from 'redux/slices/VolunteerRegistrationSlice';

export const useUser = () => ({
  ...useSelector(state => state.userSlice),
  ...useUserActions()
});

export const useRegistration = () => ({
  ...useSelector(state => state.registrationSlice),
  ...useRegistrationActions()
});

export const useFormBuilder = () => ({
  ...useSelector(state => state.formBuilderSlice),
  ...useFormBuilderActions()
});

export const useUpdateRegistration = () => ({
  ...useSelector(state => state.updateRegistrationSlice),
  ...useUpdateRegistrationActions()
});

export const useHeader = () => ({
  ...useSelector(state => state.headerSlice),
  ...useHeaderActions()
});

export const useSurvey = () => ({
  ...useSelector(state => state.surveySlice),
  ...useSurveyActions()
});

export const useVolunteerRegistration = () => ({
  ...useSelector(state => state.volunteerRegistrationSlice),
  ...useVolunteerRegistrationActions()
});

export const useVolunteerProfile = () => ({
  ...useSelector(state => state.volunteerProfileSlice),
  ...useVolunteerProfileActions()
});

export const useProfile = () => ({
  ...useSelector(state => state.profileSlice),
  ...useProfileActions()
});

export const useThankYou = () => ({
  ...useSelector(state => state.thankYouSlice),
  ...useThankYouActions()
});

export const usePostRegistration = () => ({
  ...useSelector(state => state.postRegistrationSetupSlice),
  ...usePostRegistrationActions()
});

export const useCheckInOut = () => ({
  ...useSelector(state => state.checkInOutSlice),
  ...useCheckInOutActions()
});

export const useManageAPAccess = () => ({
  ...useSelector(state => state.APAccessSlice),
  ...useAPAccessActions()
});

export const useRegistrantNotes = () => ({
  ...useSelector(state => state.registrantNotesSlice),
  ...useRegistrantNotesActions()
});

export const useErrorMessages = () => ({
  ...useSelector(state => state.errorMessagesSlice)
});

export const useSessionRegistration = () => ({
  ...useSelector(state => state.sessionRegistrationSlice),
  ...useSessionRegistrationActions()
});
