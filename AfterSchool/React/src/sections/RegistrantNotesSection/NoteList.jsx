import React from 'react';
import { Box, Typography } from '@material-ui/core';

import { useRegistrantNotes } from 'redux/hooks';
import useStyles from './styles';
import Note from './Note';

function NoteList({ handleOpenModal }) {
  const { registrantNotes } = useRegistrantNotes();

  const classes = useStyles();
  const noRecords = registrantNotes.length < 0;
  return (
    <Box className={classes.noteListWrapper}>
      {registrantNotes.map((note, key) => (
        <Note note={note} key={key} handleOpenModal={handleOpenModal} />
      ))}
      {noRecords && <Typography variant="body1">No Notes Found!</Typography>}
    </Box>
  );
}
export default NoteList;
