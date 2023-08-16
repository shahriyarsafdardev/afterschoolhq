import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';
import { Typography, Box, Button, TextField, Divider } from '@material-ui/core';

import useStyles from './styles';
import NoteList from './NoteList';
import { Loader, Pagination } from 'components';
import AddNewNoteModal from './AddNewNoteModal';
import { useRegistrantNotes } from 'redux/hooks';
import { Debounce, buildQueryParams } from 'utils';

const PER_PAGE = 5;
const RegistrantNotesSection = () => {
  const classes = useStyles();
  const { regUid } = useParams();
  const [query, setQuery] = useState('');
  const [open, setOpen] = useState(false);
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const { getNotes, registrantName, registrantNotes, msgs, loading } = useRegistrantNotes();
  const showPagination = registrantNotes.length > PER_PAGE;
  const handleOpen = () => setOpen(true);

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    setQuery(searchParams.get('searchQuery'));
    if (!searchParams.get('pageNo')) setSearchParams(buildQueryParams('pageNo', 1, searchParams));
    getNotes(regUid, PER_PAGE, searchParams.get('pageNo') || 1, searchParams.get('searchQuery'));
  }, []);

  useEffect(() => {
    getNotes(regUid, PER_PAGE, searchParams.get('pageNo'), searchParams.get('pageNo'));
  }, [searchParams]);

  const debouncedSearchChange = Debounce(searchQuery => {
    setSearchParams(buildQueryParams('searchQuery', searchQuery, searchParams));
    getNotes(regUid, PER_PAGE, searchParams.get('pageNo'), searchQuery);
    setQuery(searchQuery);
  });

  const handleSearch = e => {
    debouncedSearchChange(e.target.value);
  };

  if (loading) return <Loader />;

  return (
    <Box className={classes.notesWrapper}>
      <Typography variant="h5" className={classes.notesStudentName}>
        {registrantName}
      </Typography>
      <Divider className={classes.studentNameDivider} />
      <Box className={classes.notesSearchBar}>
        <Typography className={classes.notesHeading}>Student Notes</Typography>
        <Box className={classes.searchFieldWrapper}>
          <TextField
            placeholder="Search"
            className={classes.notesSearchField}
            variant="outlined"
            onChange={handleSearch}
            data-testid="notesSearchField"
            defaultValue={query}
          />
          <Button
            color="primary"
            variant="contained"
            className={classes.newNoteButton}
            data-testid="newNoteButton"
            onClick={() => handleOpen()}
          >
            New Note
          </Button>
        </Box>
      </Box>
      <Divider className={classes.headingDivider} />
      <NoteList handleOpenModal={() => {}} />
      {showPagination && (
        <Box className={classes.paginationBar}>
          <Pagination totalPages={50} />
        </Box>
      )}
      <AddNewNoteModal open={open} setOpen={setOpen} />
    </Box>
  );
};
export default RegistrantNotesSection;
