import SelectRegistrantRepository from 'repository/SelectRegistrantRepository';
import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import HeaderSlice from './HeaderSlice';
import utils from 'utils';

const SelectRegistrantSlice = createSlice({
  name: 'selectRegistrantSlice',
  initialState: {
    msgs: [],
    activityData: {},
    students: [],
    loading: true
  },
  reducers: {
    setData: (state, { payload }) => {
      state.students = payload.Students;
      state.activityData = payload.activityDto;
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

export const useSelectRegistrantActions = () => {
  const dispatch = useDispatch();
  const {
    setHeaderData,
    loadingStart: headerLoadingStart,
    loadingCompleted: headerLoadingCompleted
  } = HeaderSlice.actions;
  const { setMsgs, setData, loadingStart, loadingCompleted } = SelectRegistrantSlice.actions;

  const loadData = async () => {
    const { navigateTo } = utils;

    dispatch(loadingStart());
    dispatch(headerLoadingStart());

    await SelectRegistrantRepository.getData().then(({ obj, msgs }) => {
      dispatch(setMsgs(msgs));

      if (msgs.length > 0) return navigateTo('/login');

      if (obj?.activityDto) {
        const {
          HeaderImageUrl,
          ActivityName: Name,
          OrganizationName: Organization
        } = obj?.activityDto;

        dispatch(setHeaderData({ Name, Organization, HeaderImageUrl }));
      }

      dispatch(setData(obj));
      dispatch(headerLoadingCompleted());
      dispatch(loadingCompleted());
    });
  };

  const addNewRegistrant = async () => {
    return await SelectRegistrantRepository.addNewRegistrant().then(
      ({ msgs, obj: { AddNewStudentUrl } }) => {
        const { redirect } = utils;
        dispatch(setMsgs(msgs));

        if (msgs.length > 0) return;
        redirect(AddNewStudentUrl);
      }
    );
  };

  return { loadData, addNewRegistrant };
};

export default SelectRegistrantSlice;
