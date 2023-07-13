import { Box, Button, Typography } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from '../Common/styles';
import PinInputField from '../PinSetup/PinInputField';

export default function PinInputForm({ setUserPin }) {
  const classes = useStyles();
  const [pin, setPin] = useState('');

  const handleSubmit = () => {
    setUserPin(pin);
  };

  const handleChange = value => {
    setPin(value);
  };

  return (
    <Box className={classes.pinSetupForm}>
      <Typography className={classes.headerText}>Enter your 4-digit PIN.</Typography>
      <PinInputField handleChange={handleChange} errorMessage={''} />
      <Button
        onClick={handleSubmit}
        className={classes.buttonNext}
        variant="contained"
        color="primary"
        disabled={pin.length !== 4}
      >
        Next
      </Button>
    </Box>
  );
}
