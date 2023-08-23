import React, { useEffect, useState } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';
import { Typography, Box, Button, TextField, Divider } from '@material-ui/core';
import useStyles from './styles';
import NoteList from './NoteList';
import AddNewNoteModal from './AddNewNoteModal';

import { Pagination } from 'components';
import { useRegistrantNotes } from 'redux/hooks';
import { Debounce, buildQueryParams } from 'utils';
import { getAndFormatDateTime, PER_PAGE } from 'constants';

const RegistrantNotesSection = () => {
  const classes = useStyles();
  const { regUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [open, setOpen] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [editNoteValue, setEditNoteValue] = useState(null);
  const [searchParams, setSearchParams] = useSearchParams();
  const { getNotes, totalPages, registrantName, msgs, registrantNotes } = useRegistrantNotes();

  const query = searchParams.get('searchQuery') || '';
  const pageNo = searchParams.get('pageNo');
  const showPagination = totalPages > 1;

  const getRegNotes = () => {
    if (!pageNo) setSearchParams(buildQueryParams('pageNo', 1, searchParams));
    else getNotes(regUid, PER_PAGE, pageNo, query);
  };

  useEffect(() => {
    getRegNotes(regUid, pageNo, setSearchParams, searchParams, query);
  }, [pageNo]);

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  const handleOpen = () => setOpen(true);

  const handleEdit = id => {
    const note = registrantNotes.find(note => note.ID === id);
    const updatedNote = { ...note, PostDate: getAndFormatDateTime(new Date(note.PostDate)) };
    setEditNoteValue(updatedNote);
    setIsUpdating(true);
    setOpen(true);
  };

  const handleClose = () => {
    setOpen(false);
    if (isUpdating) {
      setIsUpdating(false);
      setEditNoteValue(null);
    }
  };

  const debouncedSearchChange = Debounce(searchQuery => {
    setSearchParams(buildQueryParams('searchQuery', searchQuery, searchParams));
    getNotes(regUid, PER_PAGE, 1, searchQuery);
  });

  const handleSearch = e => {
    setSearchParams(buildQueryParams('pageNo', 1, searchParams));
    debouncedSearchChange(e.target.value);
  };

  return (
    <Box className={classes.notesWrapper}>
      <Typography variant="h5" className={classes.notesStudentName}>
        {registrantName}
      </Typography>
      <Divider className={classes.studentNameDivider} />
      <Box className={classes.notesSearchBar}>
        <Typography className={classes.notesHeading}>Student Notes</Typography>
        <TextField
          variant="outlined"
          placeholder="Search"
          defaultValue={query}
          onChange={handleSearch}
          data-testid="notes-search-field"
          className={classes.notesSearchField}
        />
        <Button
          color="primary"
          variant="contained"
          onClick={handleOpen}
          data-testid="new-note-button"
          className={classes.newNoteButton}
        >
          New Note
        </Button>
      </Box>
      <Divider className={classes.headingDivider} />
      <NoteList handleEdit={handleEdit} />
      {showPagination && (
        <Box className={classes.paginationBar}>
          <Pagination totalPages={totalPages} />
        </Box>
      )}
      {open && (
        <AddNewNoteModal
          open={open}
          isUpdating={isUpdating}
          handleEdit={handleEdit}
          handleClose={handleClose}
          noteValue={editNoteValue}
          getRegNotes={getRegNotes}
        />
      )}
    </Box>
  );
};
export default RegistrantNotesSection;
