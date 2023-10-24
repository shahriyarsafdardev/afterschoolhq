import HeaderSlice from './HeaderSlice';
import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';
import ThankYouRepository from 'repository/ThankYouRepository';

const ThankYouSlice = createSlice({
  name: 'thankYouSlice',
  initialState: {
    data: {},
    loading: true
  },
  reducers: {
    setData: (state, { payload: { activityDto, activityGroupDto } }) => {
      state.data = activityDto || activityGroupDto;
    },
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
    }
  }
});

export const useThankYouActions = () => {
  const { setHeaderData, loadingCompleted: headerLoadingCompleted } = HeaderSlice.actions;
  const dispatch = useDispatch();
  const { setData, loadingStart, loadingCompleted } = ThankYouSlice.actions;
  const loadThankYouScreenData = async (id, volunteerRegistration = false) => {
    dispatch(loadingStart());
    await ThankYouRepository.getThankYouScreenData(id, volunteerRegistration).then(({ obj }) => {
      if (obj) {
        const { activityGroupDto, activityDto } = obj;
        dispatch(setData(obj));
        const { HeaderImageURL, ActivityName, SessionName, Organization } =
          activityDto || activityGroupDto;
        const Name = ActivityName || SessionName;
        dispatch(setHeaderData({ HeaderImageURL, Name, Organization }));
        dispatch(headerLoadingCompleted());
        dispatch(loadingCompleted());
      }
    });
  };

  return { loadThankYouScreenData };
};

export default ThankYouSlice;
