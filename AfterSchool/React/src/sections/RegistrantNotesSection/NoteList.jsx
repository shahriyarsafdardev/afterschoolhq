import React from 'react';
import { Box } from '@material-ui/core';

import Note from './Note';

function NoteList({ data, handleOpenModal }) {
  return (
    <Box>
      {data.map((note, key) => (
        <Note note={note} key={key} handleOpenModal={handleOpenModal} />
      ))}
    </Box>
  );
}
export default NoteList;
