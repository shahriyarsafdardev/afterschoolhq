import React, { useState } from 'react';
import { Box, Typography, Menu, MenuItem } from '@material-ui/core';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import { useSnackbar } from 'notistack';

import useStyles from './styles';
import { ConfirmationDialog } from 'components';
import { useRegistrantNotes } from 'redux/hooks';
import { buildQueryParams, formatDate } from 'utils';
import { useParams, useSearchParams } from 'react-router-dom';
import { PER_PAGE, NOTES_OPTIONS as OPTIONS, NOTES_VISIBILITY_LENGTH } from 'constants';

function Note({ note, handleEdit }) {
  const classes = useStyles();
  const { regUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [isOpen, setIsOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState(null);
  const [showMore, setShowMore] = useState(false);

  const { deleteNote, registrantNotes, totalPages, getNotes } = useRegistrantNotes();
  const { ID, Title: title, CreatedByName: writer, PostDate: date, Text: text, IsEditable } = note;
  const splittedTimeArray = date.split(' ');
  const open = Boolean(anchorEl);
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get('searchQuery') || '';
  const pageNo = Number(searchParams.get('pageNo')) || 1;
  const isExpandable = text.length > NOTES_VISIBILITY_LENGTH;
  const showMoreButtonText = showMore ? 'Show Less' : 'Show More';
  const noteText = showMore ? text : text.substring(0, NOTES_VISIBILITY_LENGTH);

  const handleClick = event => setAnchorEl(event.currentTarget);

  const handleDeletionDialog = () => {
    setIsOpen(true);
  };

  const handleCloseDialog = () => {
    setIsOpen(false);
  };

  const getRegNotes = () => {
    if (!pageNo) setSearchParams(buildQueryParams('pageNo', 1, searchParams));
    else getNotes(regUid, PER_PAGE, pageNo, query);
  };

  const handleNoteDeletion = async () => {
    await deleteNote({ noteID: ID }).then(msgs => {
      if (!msgs.length) enqueueSnackbar('Deleted.', { variant: 'success' });

      handleCloseDialog();

      if (pageNo === totalPages && registrantNotes.length === 1 && pageNo > 1) {
        setSearchParams(buildQueryParams('pageNo', pageNo - 1, searchParams));
      } else {
        getRegNotes();
      }
    });
  };

  const handleChange = option => {
    setAnchorEl(null);
    switch (option) {
      case 'Edit':
        return handleEdit(note.ID);
      case 'Delete':
        return handleDeletionDialog();
    }
  };

  return (
    <Box className={classes.noteWrapper}>
      <Box className={classes.noteHeaderBar}>
        <Box className={classes.noteHeader}>
          <Box className={classes.titlesSubHeader}>
            <Typography variant="h6">{title}</Typography>
            <Typography>{`Last Modified By ${writer}`}</Typography>
          </Box>
          <Box className={classes.dateSubHeader}>
            <Typography>{formatDate(date)}</Typography>
            <Typography
              className={classes.time}
            >{`${splittedTimeArray[1]} ${splittedTimeArray[2]}`}</Typography>
          </Box>
        </Box>
        {IsEditable && (
          <MoreVertIcon
            data-testid="options-icon"
            className={classes.options}
            onClick={handleClick}
          />
        )}
      </Box>
      <Typography>
        {noteText}
        {!showMore && isExpandable && '...'}
      </Typography>
      {isExpandable && (
        <Box className={classes.showMoreButtonWrapper}>
          <Typography
            data-testid="show-more-button"
            onClick={() => setShowMore(!showMore)}
            className={classes.showMoreButton}
          >
            {showMoreButtonText}
          </Typography>
        </Box>
      )}
      <Menu
        open={open}
        keepMounted
        id="simple-menu"
        anchorEl={anchorEl}
        disableScrollLock={true}
        onClose={() => setAnchorEl(null)}
      >
        {OPTIONS.map(option => (
          <MenuItem
            key={option}
            data-testid={`menu-item-${option}`}
            onClick={() => handleChange(option)}
          >
            {option}
          </MenuItem>
        ))}
      </Menu>

      <ConfirmationDialog
        open={isOpen}
        onClose={handleCloseDialog}
        onSubmit={handleNoteDeletion}
        title="Delete Note"
        data-testid="delete-selected-note"
        text={`Do you want to delete the ${title} note? This action is irreversible.`}
      />
    </Box>
  );
}
export default Note;
