import React from 'react';
import cs from 'classnames';

import {
  Dialog as MDialog,
  DialogContentText,
  DialogActions,
  DialogTitle,
  Button
} from '@material-ui/core';
import useStyles from './styles';

const Dialog = props => {
  const {
    open,
    text,
    title,
    onClose,
    onSubmit,
    dataTestId,
    cancelButtonText,
    confirmationButtonText
  } = props;

  const classes = useStyles();

  return (
    <MDialog
      open={open}
      onClose={onClose}
      aria-labelledby="alert-dialog-title"
      aria-describedby="alert-dialog-description"
      className={classes.dialogWrapper}
      disableScrollLock={true}
    >
      <DialogTitle id="alert-dialog-title" className={classes.dialogTitle}>
        {title}
      </DialogTitle>
      <DialogContentText id="alert-dialog-title" className={classes.dialogContentText}>
        <b>Caution: </b>
        {text}
      </DialogContentText>
      <DialogActions className={classes.dialogActions}>
        <Button
          onClick={onSubmit}
          variant="outlined"
          className={cs(classes.dialogButton, classes.warningButton)}
          data-testid={`modalSubmitButton-${dataTestId}`}
        >
          {confirmationButtonText || 'Yes'}
        </Button>
        <Button
          onClick={onClose}
          variant="outlined"
          color="primary"
          className={classes.dialogButton}
          data-testid={`modalCancelButton-${dataTestId}`}
        >
          {cancelButtonText || 'No'}
        </Button>
      </DialogActions>
    </MDialog>
  );
};

export default Dialog;
