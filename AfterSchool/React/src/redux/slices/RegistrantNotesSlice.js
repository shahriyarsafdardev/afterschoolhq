import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import { createRegistrantNote } from 'repository/RegistrantNotesRepository';

const RegistrantNotesSlice = createSlice({
  name: 'registrantNotesSlice',
  initialState: {
    registrantName: '',
    registrantNotes: [],
    msgs: [],
    loading: true
  },
  reducers: {
    setRegistrantName: (state, { payload }) => {
      state.registrantName = payload;
    },
    setRegistrantNotes: (state, { payload }) => {
      state.registrantNotes = payload;
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
    setRegistrantNotes
  } = RegistrantNotesSlice.actions;
  const getNotes = async params => {};
  const createNote = async params => {
    dispatch(loadingStart());
    return await createRegistrantNote(params).then(({ data: { msgs, obj } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) {
        dispatch(setRegistrantNotes(obj.Note));
      }
      dispatch(loadingCompleted());
      return msgs;
    });
  };
  const updateNote = async params => {};
  const deleteNote = async params => {};

  return { getNotes, createNote, updateNote, deleteNote };
};

export default RegistrantNotesSlice;
