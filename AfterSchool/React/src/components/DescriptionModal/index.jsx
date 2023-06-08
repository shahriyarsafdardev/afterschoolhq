import React, { useState, useEffect } from 'react';
import { Dialog, Typography, TextField, Button } from '@material-ui/core';
import useStyles from './styles';

const DescriptionModal = ({
  text,
  isOpen,
  handleClose,
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
    handleClose();
    setTimeout(() => {
      handleDescriptionSave(descriptionValue);
    }, 150);
  };

  useEffect(() => {
    setTimeout(() => {
      setDescriptionValue(text);
    }, 200);
  }, [text, isOpen]);

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
        />
        <Button
          variant="contained"
          color="primary"
          className={classes.saveButton}
          onClick={handleSave}
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
