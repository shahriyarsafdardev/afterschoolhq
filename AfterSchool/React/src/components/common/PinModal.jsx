import React from 'react';
import { Modal } from 'components';
import { Box, Button, CircularProgress, TextField, Typography } from '@material-ui/core';

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
        <Button
          color="primary"
          onClick={onSubmit}
          variant="contained"
          className={classes.modalElementMargin}
          disabled={submitDisabled}
        >
          {submitDisabled ? (
            <>
              {'Loading . . .'} <CircularProgress size={'20px'} />
            </>
          ) : (
            'Start'
          )}
        </Button>
      </Box>
    </Modal>
  );
}

export default PinModal;
