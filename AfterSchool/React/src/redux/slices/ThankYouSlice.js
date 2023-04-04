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
    setData: (state, { payload: { activityDto } }) => {
      state.data = activityDto;
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
  const dispatch = useDispatch();
  const { setData, loadingStart, loadingCompleted } = ThankYouSlice.actions;
  const loadThankYouScreenData = async id => {
    dispatch(loadingStart());
    const data = await ThankYouRepository.getThankYouScreenData(id);
    dispatch(setData(data.obj));
    dispatch(loadingCompleted());
  };

  return { loadThankYouScreenData };
};

export default ThankYouSlice;
