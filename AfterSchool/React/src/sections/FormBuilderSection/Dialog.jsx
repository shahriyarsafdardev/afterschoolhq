import React from 'react';
import cs from 'classnames';
import PropTypes from 'prop-types';

import {
  Dialog as MDialog,
  DialogContentText,
  DialogActions,
  DialogTitle,
  Button
} from '@material-ui/core';
import useStyles from './styles';

const Dialog = props => {
  const { onClose, onSubmit, open, title, text, dataTestId } = props;
  const classes = useStyles();

  return (
    <MDialog
      open={open}
      onClose={onClose}
      aria-labelledby="alert-dialog-title"
      aria-describedby="alert-dialog-description"
    >
      <DialogTitle id="alert-dialog-title">{title}</DialogTitle>
      <DialogContentText id="alert-dialog-title">{text}</DialogContentText>
      <DialogActions>
        <Button
          onClick={onClose}
          variant="outlined"
          color="primary"
          className={classes.dialogButton}
          data-testid={`modalCancelButton-${dataTestId}`}
        >
          {'No'}
        </Button>
        <Button
          onClick={onSubmit}
          variant="outlined"
          className={cs(classes.warningButton, classes.dialogButton)}
          data-testid={`modalSubmitButton-${dataTestId}`}
        >
          {'Yes'}
        </Button>
      </DialogActions>
    </MDialog>
  );
};

Dialog.propTypes = {
  onClose: PropTypes.func.isRequired,
  onSubmit: PropTypes.func.isRequired,
  open: PropTypes.bool.isRequired,
  title: PropTypes.string,
  text: PropTypes.string
};

export default Dialog;
