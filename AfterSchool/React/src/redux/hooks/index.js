import { useSelector } from 'react-redux';
import { useUserActions } from 'redux/slices/UserSlice';
import { useSurveyActions } from 'redux/slices/SurveySlice';
import { useProfileActions } from 'redux/slices/ProfileSlice';
import { useThankYouActions } from 'redux/slices/ThankYouSlice';
import { useRegistrationActions } from 'redux/slices/RegistrationSlice';
import { useVolunteerProfileActions } from 'redux/slices/VolunteerProfileSlice';
import { useVolunteerRegistrationActions } from 'redux/slices/VolunteerRegistrationSlice';

export const useUser = () => ({
  ...useSelector(state => state.userSlice),
  ...useUserActions()
});

export const useRegistration = () => ({
  ...useSelector(state => state.registrationSlice),
  ...useRegistrationActions()
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
