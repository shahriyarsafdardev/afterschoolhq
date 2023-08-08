import React, { useState, useEffect } from 'react';
import { Dialog, Typography, TextField, Button } from '@material-ui/core';
import useStyles from './styles';

const DescriptionModal = ({
  text,
  isOpen,
  handleClose,
  maxLength = 4000,
  handleDescriptionSave,
  title = 'Add Description'
}) => {
  const classes = useStyles();
  const [descriptionValue, setDescriptionValue] = useState(text);

  const handleChange = event => {
    const {
      target: { value }
    } = event;
    setDescriptionValue(value);
  };

  const handleSave = () => {
    if (descriptionValue.length <= maxLength) {
      handleClose();
      setTimeout(() => {
        handleDescriptionSave(descriptionValue);
      }, 150);
    }
  };

  useEffect(() => {
    setTimeout(() => {
      setDescriptionValue(text);
    }, 200);
  }, [text, isOpen]);

  const isExceeded = descriptionValue && descriptionValue.length > maxLength;

  return (
    <Dialog
      open={isOpen}
      onClose={handleClose}
      classes={{
        paperWidthSm: classes.modal
      }}
    >
      <div className={classes.modalContentWrapper}>
        <Typography variant="subtitle1">{title}</Typography>
        <TextField
          multiline
          placeholder="Start writing..."
          rows={10}
          fullWidth
          variant="outlined"
          classes={{
            root: classes.descriptionField
          }}
          value={descriptionValue}
          onChange={handleChange}
          error={isExceeded}
          helperText={
            isExceeded
              ? `Character limit exceeded (maximum ${maxLength} characters)`
              : `${descriptionValue?.length || 0}/${maxLength}`
          }
        />
        <Button
          variant="contained"
          color="primary"
          className={classes.saveButton}
          onClick={handleSave}
          disabled={isExceeded}
        >
          Save
        </Button>
        <Button className={classes.skipButton} onClick={handleClose}>
          Skip
        </Button>
      </div>
    </Dialog>
  );
};

export default DescriptionModal;
