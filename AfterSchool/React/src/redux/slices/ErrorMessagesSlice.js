import { createSlice } from '@reduxjs/toolkit';

const ErrorMessagesSlice = createSlice({
  name: 'errorMessagesSlice',
  initialState: {
    msgs: []
  },
  reducers: {
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export default ErrorMessagesSlice;
