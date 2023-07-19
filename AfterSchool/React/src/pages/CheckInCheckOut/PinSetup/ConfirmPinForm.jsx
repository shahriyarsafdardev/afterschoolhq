import { Box, Button, Typography } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from '../Common/styles';
import PinInputField from '../Common/PinInputField';

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
      <PinInputField handleChange={handleChange} errorMessage={errorMessage} />
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
