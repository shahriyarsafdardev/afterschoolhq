import React from 'react';
import { Box, Typography } from '@material-ui/core';

import { useRegistrantNotes } from 'redux/hooks';
import { Loader } from 'components';
import useStyles from './styles';
import Note from './Note';

function NoteList({ handleOpenModal }) {
  const { registrantNotes, loading } = useRegistrantNotes();

  const classes = useStyles();
  const noRecords = registrantNotes.length === 0;

  if (loading) return <Loader />;

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
