import HeaderSlice from './HeaderSlice';
import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';
import ThankYouRepository from 'repository/ThankYouRepository';

const ThankYouSlice = createSlice({
  name: 'thankYouSlice',
  initialState: {
    msgs: [],
    data: {},
    loading: true
  },
  reducers: {
    setData: (state, { payload: { activityDto } }) => {
      state.data = activityDto;
    },
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useThankYouActions = () => {
  const { setHeaderData, loadingCompleted: headerLoadingCompleted } = HeaderSlice.actions;
  const dispatch = useDispatch();
  const { setMsgs, setData, loadingStart, loadingCompleted } = ThankYouSlice.actions;
  const loadThankYouScreenData = async (id, volunteerRegistration = false) => {
    dispatch(loadingStart());
    await ThankYouRepository.getThankYouScreenData(id, volunteerRegistration).then(
      ({ obj, msgs }) => {
        dispatch(setMsgs(msgs));
        if (obj) {
          dispatch(setData(obj));
          const { HeaderImageURL, ActivityName: Name, Organization } = obj?.activityDto;
          dispatch(setHeaderData({ HeaderImageURL, Name, Organization }));
          dispatch(headerLoadingCompleted());
          dispatch(loadingCompleted());
        }
      }
    );
  };

  return { loadThankYouScreenData };
};

export default ThankYouSlice;
