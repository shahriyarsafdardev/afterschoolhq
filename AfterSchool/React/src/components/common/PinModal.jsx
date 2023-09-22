import React from 'react';
import { Modal } from 'components';
import { LoadingButton } from 'components/Overrides';
import { Box, TextField, Typography } from '@material-ui/core';

import useStyles from './styles';
function PinModal({ isOpen, onChange, onSubmit, submitDisabled }) {
  const classes = useStyles();
  return (
    <Modal isOpen={isOpen}>
      <Box className={classes.pinModalContentWrapper}>
        <Typography variant="h3" className={classes.modalElementMargin}>
          Enter your PIN to Continue
        </Typography>
        <TextField
          type="number"
          variant="outlined"
          onChange={onChange}
          className={classes.modalElementMargin}
        />
        <LoadingButton
          type="submit"
          onClick={onSubmit}
          variant="contained"
          color="primary"
          text="Start"
          disabled={submitDisabled}
          loading={submitDisabled}
          className={classes.modalElementMargin}
        />
      </Box>
    </Modal>
  );
}

export default PinModal;
