import React, { useState } from 'react';
import { Box, Typography, Menu, MenuItem } from '@material-ui/core';
import MoreVertIcon from '@material-ui/icons/MoreVert';

import useStyles from './styles';
import { ConfirmationDialog } from 'components';

function Note({ note, handleOpenModal }) {
  const classes = useStyles();
  const [isOpen, setIsOpen] = useState(false);
  const { Title: title, CreatedByName: writer, PostDate: date, Text: text } = note;
  const options = ['EDIT', 'DELETE'];
  const [anchorEl, setAnchorEl] = useState(null);
  const open = Boolean(anchorEl);

  const handleClick = event => setAnchorEl(event.currentTarget);

  const handelDeletionModal = () => {
    setIsOpen(true);
  };

  const handleCloseDialog = () => {
    setIsOpen(false);
  };

  const handleChange = option => {
    setAnchorEl(null);
    switch (option) {
      case 'EDIT':
        return handleOpenModal();
      case 'DELETE':
        return handelDeletionModal();
    }
  };

  const [showMore, setShowMore] = useState(false);

  const shouldShow = text.length > 530;
  const noteText = showMore ? text : text.substring(0, 500);
  const showMoreButtonText = showMore ? 'Show Less' : 'Show More';

  return (
    <Box className={classes.noteWrapper}>
      <Box className={classes.noteHeader}>
        <Box>
          <Typography variant="h6">{title}</Typography>
          <Typography>{`By ${writer}`}</Typography>
        </Box>
        <Box className={classes.subHeader}>
          <Typography>{date}</Typography>
          <MoreVertIcon
            data-testid="options-icon"
            className={classes.options}
            onClick={handleClick}
          />
        </Box>
      </Box>
      <Typography>{noteText}</Typography>
      {shouldShow && (
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
        id="options"
        anchorEl={anchorEl}
        keepMounted
        open={open}
        onClose={() => setAnchorEl(null)}
        PaperProps={{
          style: {}
        }}
      >
        {options.map(option => (
          <MenuItem
            data-testid={`menu-item-${option}`}
            key={option}
            onClick={() => handleChange(option)}
          >
            {option}
          </MenuItem>
        ))}
      </Menu>
      <ConfirmationDialog
        open={isOpen}
        onClose={handleCloseDialog}
        title="Delete Note"
        data-testid="delete-selected-note"
        text="Do you want to delete the selected note? This action is irreversible."
      />
    </Box>
  );
}
export default Note;
