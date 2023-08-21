import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import {
  createRegistrantNote,
  getRegistrantNotes,
  updateRegistrantNote
} from 'repository/RegistrantNotesRepository';

const RegistrantNotesSlice = createSlice({
  name: 'registrantNotesSlice',
  initialState: {
    registrantName: '',
    registrantNotes: [],
    totalPages: 0,
    pageNo: 0,
    loading: true,
    msgs: []
  },
  reducers: {
    setRegistrantName: (state, { payload }) => {
      state.registrantName = payload;
    },
    setRegistrantNotes: (state, { payload }) => {
      state.registrantNotes = payload;
    },
    setPaginationData: (state, { payload: { TotalPages, PageNo } }) => {
      state.totalPages = TotalPages;
      state.pageNo = PageNo;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    },
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
    }
  }
});

export const useRegistrantNotesActions = () => {
  const dispatch = useDispatch();
  const {
    setMsgs,
    loadingStart,
    loadingCompleted,
    setRegistrantName,
    setPaginationData,
    setRegistrantNotes
  } = RegistrantNotesSlice.actions;

  const getNotes = async (regUid, perPage, pageNo, searchQuery = '') => {
    dispatch(loadingStart());
    await getRegistrantNotes(regUid, perPage, pageNo, searchQuery).then(({ obj, msgs }) => {
      if (obj) {
        dispatch(setRegistrantName(obj.RegistrantNotes.RegistrantName));
        dispatch(setRegistrantNotes(obj.RegistrantNotes.Notes));
        dispatch(setPaginationData(obj));
      }
      dispatch(setMsgs(msgs));
    });
    dispatch(loadingCompleted());
  };

  const createNote = async ({ registrantNotes, ...params }) => {
    dispatch(loadingStart());
    return await createRegistrantNote(params).then(({ data: { msgs, obj } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) {
        dispatch(setRegistrantNotes([obj.Note, ...registrantNotes]));
      }
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  const updateNote = async params => {
    dispatch(loadingStart());
    return await updateRegistrantNote(params).then(({ data: { msgs, obj } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) {
        // TODO: Update the resgistrantNotes array with the API response
      }
      dispatch(loadingCompleted());
      return msgs;
    });
  };
  const deleteNote = async params => {};

  return { getNotes, createNote, updateNote, deleteNote };
};

export default RegistrantNotesSlice;
