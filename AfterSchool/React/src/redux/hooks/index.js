import { useSelector } from 'react-redux';
import { useUserActions } from '../slices/UserSlice';
import { useRegistrationActions } from '../slices/RegistrationSlice';

export const useUser = () => ({
  ...useSelector(state => state.userSlice),
  ...useUserActions()
});

export const useRegistration = () => ({
  ...useSelector(state => state.registrationSlice),
  ...useRegistrationActions()
});
