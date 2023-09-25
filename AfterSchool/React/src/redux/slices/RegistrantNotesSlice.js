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
    pageNo: 0,
    loading: true,
    totalPages: 0,
    registrantName: '',
    registrantNotes: []
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
    loadingStart: state => {
      state.loading = true;
    },
    loadingCompleted: state => {
      state.loading = false;
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
    loadingStart,
    updateRegNote,
    loadingCompleted,
    setRegistrantName,
    setPaginationData,
    setRegistrantNotes
  } = RegistrantNotesSlice.actions;

  const getNotes = async (regUid, perPage, pageNo, searchQuery = '') => {
    dispatch(loadingStart());
    await getRegistrantNotes(regUid, perPage, pageNo, searchQuery).then(({ obj }) => {
      if (obj) {
        dispatch(setRegistrantName(obj.RegistrantNotes.RegistrantName));
        dispatch(setRegistrantNotes(obj.RegistrantNotes.Notes));
        dispatch(setPaginationData(obj));
      }
    });
    dispatch(loadingCompleted());
  };

  const createNote = async ({ ...params }) => {
    dispatch(loadingStart());
    return await createRegistrantNote(params).then(({ data: { msgs } }) => {
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  const updateNote = async params => {
    return await updateRegistrantNote(params).then(({ data: { msgs, obj } }) => {
      if (!msgs.length) dispatch(updateRegNote(obj.Note));

      return msgs;
    });
  };

  const deleteNote = async ({ noteID }) => {
    dispatch(loadingStart());
    return await deleteRegistrantNote({ noteID }).then(({ data: { msgs } }) => {
      dispatch(loadingCompleted());
      return msgs;
    });
  };

  return { getNotes, createNote, updateNote, deleteNote };
};

export default RegistrantNotesSlice;
