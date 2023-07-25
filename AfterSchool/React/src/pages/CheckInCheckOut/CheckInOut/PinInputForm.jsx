import { Box, Button, Typography } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from '../Common/styles';
import PinInputField from '../Common/PinInputField';
import { useNavigate } from 'react-router-dom';

export default function PinInputForm({ setUserPin, resetPinLink }) {
  const classes = useStyles();
  const [pin, setPin] = useState('');
  const navigate = useNavigate();

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
        data-testid="next-button"
      >
        Next
      </Button>
      <Typography
        onClick={() => navigate(resetPinLink)}
        className={classes.link}
        variant="body1"
        data-testid="forgot-pin-button"
      >
        Forgot My PIN
      </Typography>
    </Box>
  );
}
