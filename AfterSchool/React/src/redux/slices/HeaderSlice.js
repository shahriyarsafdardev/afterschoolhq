import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';

const HeaderSlice = createSlice({
  name: 'headerSlice',
  initialState: {
    headerData: {},
    loading: true
  },
  reducers: {
    setHeaderData: (state, { payload }) => {
      state.headerData = payload;
    },
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
    }
  }
});

export const useHeaderActions = () => {
  const { setHeaderData, loadingStart, loadingCompleted } = HeaderSlice.actions;
  const dispatch = useDispatch();

  const loadHeader = headerData => {
    dispatch(loadingStart());
    dispatch(setHeaderData(headerData));
    dispatch(loadingCompleted());
  };

  return { loadHeader };
};

export default HeaderSlice;
