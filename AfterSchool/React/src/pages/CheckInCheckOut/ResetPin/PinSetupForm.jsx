import { Box, Button, Typography } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from '../Common/styles';
import PinInputField from '../Common/PinInputField';

function PinSetupForm({ setUserPin }) {
  const classes = useStyles();
  const [pin, setPin] = useState('');

  const handleSubmit = () => {
    setUserPin(pin);
  };

  return (
    <Box className={classes.content}>
      <Box className={classes.pinSetupForm}>
        <Typography className={classes.headerText}>Reset your PIN code.</Typography>
        <Typography className={classes.subHeaderText}>
          Please select four numbers to use for your PIN code during check in and check out.
        </Typography>
        <PinInputField handleChange={setPin} errorMessage={''} />
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
    </Box>
  );
}

export default PinSetupForm;
