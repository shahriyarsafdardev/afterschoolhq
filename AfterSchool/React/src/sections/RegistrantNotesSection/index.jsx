import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';
import { Typography, Box, Button, TextField, Divider } from '@material-ui/core';

import useStyles from './styles';
import NoteList from './NoteList';
import { Pagination } from 'components';
import AddNewNoteModal from './AddNewNoteModal';
import { useRegistrantNotes } from 'redux/hooks';
import { Debounce, buildQueryParams } from 'utils';

const PER_PAGE = 3;
const RegistrantNotesSection = () => {
  const classes = useStyles();
  const { regUid } = useParams();
  const [open, setOpen] = useState(false);
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const { getNotes, totalPages, registrantName, msgs } = useRegistrantNotes();
  const showPagination = totalPages > 1;
  const query = searchParams.get('searchQuery') || '';
  const pageNo = searchParams.get('pageNo');
  const handleOpen = () => setOpen(true);

  useEffect(() => {
    if (!pageNo) setSearchParams(buildQueryParams('pageNo', 1, searchParams));
    else getNotes(regUid, PER_PAGE, pageNo, query);
  }, [pageNo]);

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

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
      <NoteList handleOpenModal={() => {}} />
      {showPagination && (
        <Box className={classes.paginationBar}>
          <Pagination totalPages={totalPages} />
        </Box>
      )}
      <AddNewNoteModal open={open} setOpen={setOpen} />
    </Box>
  );
};
export default RegistrantNotesSection;
