import { useSelector } from 'react-redux';
import { useUserActions } from 'redux/slices/UserSlice';
import { useProfileActions } from 'redux/slices/ProfileSlice';
import { useRegistrationActions } from 'redux/slices/RegistrationSlice';

export const useUser = () => ({
  ...useSelector(state => state.userSlice),
  ...useUserActions()
});

export const useRegistration = () => ({
  ...useSelector(state => state.registrationSlice),
  ...useRegistrationActions()
});

export const useProfile = () => ({
  ...useSelector(state => state.profileSlice),
  ...useProfileActions()
});
