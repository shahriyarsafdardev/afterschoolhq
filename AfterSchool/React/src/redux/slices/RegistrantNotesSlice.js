import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import {
  createRegistrantNote,
  getRegistrantNotes,
  updateRegistrantNote,
  deleteRegistrantNote
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
    addNewRegistrantNote: (state, { payload }) => {
      const updatedNotes = [...state.registrantNotes];

      if (updatedNotes.length >= 3) {
        updatedNotes.pop();
      }
      state.registrantNotes = [payload, ...updatedNotes];
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
    },
    deleteRegNote: (state, { payload: { noteID } }) => {
      const updatedNotes = state.registrantNotes.filter(note => note.ID !== noteID);
      state.registrantNotes = [...updatedNotes];
    },
    updateRegNote: (state, { payload }) => {
      const updatedNotes = [...state.registrantNotes];

      const index = updatedNotes.findIndex(note => note.ID === payload.ID);

      updatedNotes[index] = payload;
      state.registrantNotes = updatedNotes;
    }
  }
});

export const useRegistrantNotesActions = () => {
  const dispatch = useDispatch();
  const {
    setMsgs,
    loadingStart,
    deleteRegNote,
    updateRegNote,
    loadingCompleted,
    setRegistrantName,
    setPaginationData,
    setRegistrantNotes,
    addNewRegistrantNote
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

  const createNote = async ({ ...params }) => {
    dispatch(loadingStart());
    return await createRegistrantNote(params).then(({ data: { msgs, obj } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) {
        dispatch(addNewRegistrantNote(obj.Note));
      }
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  const updateNote = async params => {
    dispatch(loadingStart());
    return await updateRegistrantNote(params).then(({ data: { msgs, obj } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(updateRegNote(obj.Note));
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  const deleteNote = async ({ noteID }) => {
    dispatch(loadingStart());
    return await deleteRegistrantNote({ noteID }).then(({ data: { msgs } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(deleteRegNote({ noteID }));
      dispatch(loadingCompleted());

      return msgs;
    });
  };

  return { getNotes, createNote, updateNote, deleteNote };
};

export default RegistrantNotesSlice;
