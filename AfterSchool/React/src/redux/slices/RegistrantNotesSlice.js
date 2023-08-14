import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import RegistrantNotesRepository from 'repository/RegistrantNotesRepository';

const RegistrantNotesSlice = createSlice({
  name: 'registrantNotesSlice',
  initialState: {
    registrantName: '',
    registrantNotes: [],
    loading: true
  },
  reducers: {
    setRegistrantName: (state, { payload }) => {
      state.registrantName = payload;
    },
    setRegistrantNotes: (state, { payload }) => {
      state.registrantNotes = payload;
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
    loadingStart,
    loadingCompleted,
    setRegistrantName,
    setRegistrantNotes
  } = RegistrantNotesSlice.actions;
  const getNotes = async params => {};
  const createNote = async params => {};
  const updateNote = async params => {};
  const deleteNote = async params => {};

  return { getNotes, createNote, updateNote, deleteNote };
};

export default RegistrantNotesSlice;
