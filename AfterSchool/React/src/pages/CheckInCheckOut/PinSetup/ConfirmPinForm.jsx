import { Box, Button, Typography } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from './styles';
import PinInput from 'react-pin-input';
import { ErrorBand } from 'components';

export default function ConfirmPinForm({ userPin, handleSubmitPinCode }) {
  const classes = useStyles();
  const [pin, setPin] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  const handleSubmit = () => {
    if (!errorMessage) {
      handleSubmitPinCode(pin);
    }
  };

  const handleChange = value => {
    setPin(value);
    if (value !== userPin) {
      setErrorMessage("Pin didn't match");
    } else {
      setErrorMessage('');
    }
  };

  return (
    <Box className={classes.pinSetupForm}>
      <Typography className={classes.headerText}>Please enter your PIN code again.</Typography>
      <Box className={classes.marginAuto}>
        <PinInput
          length={4}
          onChange={value => {
            handleChange(value);
          }}
          type="numeric"
          inputMode="number"
          style={{ padding: '10px' }}
          inputStyle={{ border: '3px solid #E4E4E4', borderRadius: '6px' }}
          regexCriteria={/^[ A-Za-z0-9_@./#&+-]*$/}
        />
        {errorMessage && <ErrorBand error={errorMessage} />}
      </Box>
      <Button
        onClick={handleSubmit}
        className={classes.buttonNext}
        variant="contained"
        color="primary"
        disabled={errorMessage}
      >
        Next
      </Button>
    </Box>
  );
}
